using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using sync2Live.Syncronizers;
using sync2Live.Utils;
using Newtonsoft.Json;
using System.Data;
using System.ComponentModel;
using System.Xml;

namespace sync2Live
{
    class ApiSynchronizer
    {
        Parameters _params;

        const string srvAPIURL = "http://collectionsystem.grebinnyk.com/exports";
        public ApiSynchronizer(Parameters prms)
        {
            _params = prms;
            DatabaseReader.OpenConnection(_params);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                DatabaseReader.CloseConnection();
            }
        } 

        public void Synchronize()
        {
            
            DataTable iPat = DatabaseReader.ReadSQLDataByCommand("Select [Id],[PolicyNumber],rtrim([AccountId]) as AccID,[FirstName],[MiddleName],[LastName],[DOB],[UpdateDate],[CreateDate],[SSN],[Policy],[Access] FROM [CollectionSystem-Imports].[dbo].[Patients] where id>0");
            DataTable iIns = DatabaseReader.ReadSQLDataByCommand("select * from [collectionsystem-imports].dbo.InsuranceCompanies where id>0");
            DataTable iPract = DatabaseReader.ReadSQLDataByCommand("select * from [collectionsystem-imports].dbo.practices where id>0");
            DataTable iDoc = DatabaseReader.ReadSQLDataByCommand("select * from [collectionsystem-imports].dbo.Doctors where id>0");
            DataTable iCPT = DatabaseReader.ReadSQLDataByCommand("select * from [collectionsystem-imports].dbo.CPTs where id>0");
            DataTable LiveID = DatabaseReader.ReadSQLDataByCommand("select liveid from [collectionsystem-imports].dbo.charges where id>0");

            if (iPat.Rows.Count == 0 || iIns.Rows.Count == 0 || iPract.Rows.Count == 0 || iDoc.Rows.Count == 0 || iCPT.Rows.Count == 0)
            {
                Logger.Write("Local table data not extracted");
                Console.WriteLine("Local table data not extracted");
                return;
            }

            DataTable wPract = getWebPractice();

            if (!syncDTPract(ref iPract, wPract))
            {
                return;
            }
            


            string clientID = "805";

            foreach (DataRow r in wPract.Rows)
            {
                string practiceID = r["id"].ToString();
                DataTable wID = getWebChargeIDs(clientID, practiceID);
                syncDT_deleteWildCharges(LiveID, wID);
            }


            wPract.Dispose();


            DataTable wPat = getWebPatients();

            if (!syncDTPatients(ref iPat, wPat))
            {
                return;
            }
            wPat.Dispose();


            DataTable wIns = getWebInsurance();

            if (!syncDTIns(ref iIns, wIns))
            {
                return;
            }
            wIns.Dispose();





            DataTable wDoc = getWebDoctor();
            
            if (!syncDTDoc(ref iDoc, wDoc))
            {
                return;
            }
            wDoc.Dispose();

            DataTable wCPT = getWebCPT();

            if (!syncDTCPT(ref iCPT, wCPT))
            {
                return;
            }
            wCPT.Dispose();








            syncDTCharges(ref iPat, ref iIns, ref iPract, ref iDoc, ref iCPT );



            return;




                /*        if (!SyncPatients())
                        {
                            Logger.Write("sync patients table failed.");
                            Console.WriteLine("sync patients table failed.");
                            return;
                        }

                        //SyncClients();                      //== OK

                        if (!SyncInsuranceConpanies())
                        {
                            Logger.Write("sync Insurance Companies table failed.");
                            Console.WriteLine("sync Insurance Companies table failed.");
                            return;
                        }

                        if (!SyncPractices())
                        {
                            Logger.Write("sync Practices table failed.");
                Console.WriteLine("sync Practices table failed.");
                return;
                        }

                        //SyncClientPractices();                  // ok
                        if (!SyncDoctors())
                        {
                            Logger.Write("sync Doctors table failed.");
                Console.WriteLine("sync Doctors table failed.");
                return;
                        }

                        //SyncTransactionTypes();  
                        if (!SyncCPT())
                        {
                            Logger.Write("sync CPT table failed.");
                Console.WriteLine("sync CPT table failed.");
                return;
                        }




            DeleteChargesTransactions(); //-3
            SyncChargesTransactionsSplitted(); //-1 
            UpdateChargesTransactions(); //-2
            */

        }

        private void syncDT_deleteWildCharges(DataTable LiveID, DataTable wID)
        {

            if (wID == null) return;
            DataTable selectedID = new DataTable();
            DataColumn colInt64 = new DataColumn("webID");
            colInt64.DataType = System.Type.GetType("System.Int64");
            selectedID.Columns.Add(colInt64);


           

            //wID.Columns.Remove("id"); 
                // wID.Columns.Remove("rootNode_Id");

                foreach (DataRow wr in wID.Rows)
            {
             
                DataRow[] foundrow = LiveID.Select("LiveID=" + wr[0].ToString());
                if (foundrow.Length == 1)
                {
                    //
                }
                else if (foundrow.Length > 1)
                {
                    Logger.Write("More then 1 live records linked to local DB:" + wr[0].ToString());
                    Console.WriteLine("More then 1 live records linked to local DB:" + wr[0].ToString());

                    return;
                }
                else if (foundrow.Length == 0)
                {
                    // deleting Wild charge 
                    // keep in tbl to delete 
                   
                    DataRow nr =  selectedID.NewRow();
                    nr[0] = long.Parse(wr[0].ToString());
                    selectedID.Rows.Add(nr);

                }

            }

            if (selectedID.Rows.Count ==0 )
            {
                return;
            }
            //selectedID.Columns.Remove("WebId2");
        

            foreach (DataRow r in selectedID.Rows)
            {

                StringBuilder s = new StringBuilder();
                s.Append("{\"id\":");
                s.Append(r[0].ToString());
                s.Append("}");

                StringBuilder cmd = new StringBuilder();

                if (syncDT_deleteTransaction((Int64)r[0]))
                {

                    string resp = HttpHandler.PostJson(srvAPIURL + "/Charge/Kill", s.ToString());
                    DataSet rds = ReadDataFromJson(resp);
                    if (rds != null)
                    {
                        string respStatus = rds.Tables[0].Rows[0]["Status"].ToString();

                        Logger.Write("Got server respond while killing LiveID: " + s.ToString() + " :: " + respStatus)  ;
                        Console.WriteLine("Got server respond while killing LiveID: " + s.ToString() + " :: " + respStatus);
                    }
                    else
                    {
                        Logger.Write("Error while killing LiveID: " + s.ToString());
                        Console.WriteLine("Error while killing LiveID: " + s.ToString());
                    }
                }

            }
            return;
        }

        private DataTable getWebChargeIDs(string clientID, string practiceID)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();


            try
            {
                //Charge/GetChargesIds?clientId={clientId}&practiceId={practiceId}
                string json = getHTTPData("/Charge/GetChargesIds?clientId=" + clientID +"&practiceId=" + practiceID);
                ds = ReadDataFromJson(json);
                dt = ds.Tables["Model"];
                return dt;
            }
            catch (Exception e)
            {
                Logger.Write("Unable to convert retrieved list of Doctiors from server into Table - " + e.ToString());
                return null;
            }
        }

        public bool syncDTCharges(ref DataTable iPat, ref DataTable iIns, ref DataTable iPract, ref DataTable iDoc, ref DataTable iCPT)
        {
            // add check for "wild"


            syncDT_deleteCharges();

            syncDT_uploadChargesNew(ref iPat, ref iIns, ref iPract, ref iDoc, ref iCPT);

            syncDT_ChargesUpdate(ref iPat, ref iIns, ref iPract, ref iDoc, ref iCPT);

            return true;
        }


        public bool syncDT_ChargesUpdate(ref DataTable iPat, ref DataTable iIns, ref DataTable iPract, ref DataTable iDoc, ref DataTable iCPT)
        {
            DataTable tblToUpload = DatabaseReader.ReadSQLDataByCommand(@"
                    select PracticeId, PatientId, CPTId, InsuranceCompanyId, DoctorId as PhysicianId, 805 as ClientID, 
                    DateOfService, DateOfPosting,Billed,Payment,Adjustment,Balance, Id, liveID  
                    from [collectionsystem-imports].dbo.charges ch where id > 0 and updated=1 and liveid>0");

            if (tblToUpload.Rows.Count == 0) return true;

            DataTable initList = tblToUpload.Copy();

            DataColumn colInt64 = new DataColumn("webID");
            colInt64.DataType = System.Type.GetType("System.Int64");
            initList.Columns.Add(colInt64);

            int pageNum = 1;
            int pageSize = 100;

            DataTable List = new DataTable();

            while ((pageNum - 1) * pageSize < tblToUpload.Rows.Count)
            {
                
                List = tblToUpload.Rows.Cast<System.Data.DataRow>().Skip((pageNum - 1) * pageSize).Take(pageSize).CopyToDataTable();

                if (List.Rows.Count > 0)
                {

                    foreach (DataRow lRow in List.Rows)
                    {
                        long webID;

                        webID = getWebID(iPat, (long)lRow["PatientID"]);
                        if (webID == -1) return false;
                        lRow["PatientID"] = webID;

                        webID = getWebID(iIns, (long)lRow["InsuranceCompanyId"]);
                        if (webID == -1) return false;
                        lRow["InsuranceCompanyId"] = webID;

                        webID = getWebID(iPract, (long)lRow["PracticeID"]);
                        if (webID == -1) return false;
                        lRow["PracticeID"] = webID;

                        webID = getWebID(iDoc, (long)lRow["PhysicianId"]);
                        if (webID == -1) return false;
                        lRow["PhysicianId"] = webID;

                        webID = getWebID(iCPT, (long)lRow["CPTId"]);
                        if (webID == -1) return false;
                        lRow["CPTId"] = webID;

                    }

                    bool updateMode = true;
                    if (syncDT_uploadChargesList(ref List, updateMode  ))
                    {
                        //update LiveID 

                        StringBuilder sb_tmp = new StringBuilder();
                        sb_tmp.AppendLine("BEGIN TRANSACTION tr1 ");
                        sb_tmp.AppendLine(@"DECLARE @PreTbl TABLE ([Id] BIGINT, [webID] BIGINT);");
                        foreach (DataRow row in List.Rows)
                        {
                            sb_tmp.Append("insert into @PreTbl (id, webID) values (");
                            sb_tmp.Append(row["id"].ToString());
                            sb_tmp.Append(",");
                            sb_tmp.Append(row["webid"].ToString());
                            sb_tmp.AppendLine(")");
                        }
                        sb_tmp.AppendLine(@"UPDATE [CollectionSystem-Imports].[dbo].[Charges] 
                            SET updated = 0
                            FROM [CollectionSystem-Imports].[dbo].[Charges] INNER JOIN 
                            @PreTbl AS pTbl On [CollectionSystem-Imports].[dbo].[Charges].id = pTbl.Id WHERE pTbl.Id > 0 and updated =1; ");


                        sb_tmp.AppendLine("COMMIT TRANSACTION tr1;");

                        bool res = MultipleExecutor.ExecuteTimes(5, sb_tmp.ToString(), "Charges");

                        int emptyList = 0;
                        StringBuilder idList = new StringBuilder();
                        if (res == true)
                        {

                            Logger.Write("Charges uploaded: " + List.Rows.Count.ToString());
                            Console.WriteLine("Charges uploaded: " + List.Rows.Count.ToString());


                            // insert transactions 


                            idList.Insert(0, "select [DateOfService],[DateOfPosting],[TransactionId],[Billed],[Payment],[Adjustment],[Balance],[ChargeId] from [collectionsystem-imports].dbo.transactions where chargeid in (");


                            foreach (DataRow row in List.Rows)
                            {

                                if (emptyList == 0)
                                {
                                    idList.Append(row["id"].ToString());
                                }
                                else
                                {
                                    idList.Append(row["id"].ToString());
                                }
                                emptyList++;
                                if (emptyList < List.Rows.Count) idList.Append(",");
                            }
                        }
                        else
                        {
                            Logger.Write("Error while Update Charges LiveID");
                            Console.WriteLine("Error while Update Charges LiveID");
                            return false;
                        }

                        if (emptyList > 0)
                        {

                            idList.Append(");");

                            DataTable iTrans = DatabaseReader.ReadSQLDataByCommand(idList.ToString());

                            if (iTrans.Rows.Count > 0)
                            {

                                foreach (DataRow r in iTrans.Rows)
                                {
                                    DataRow[] tempRow;

                                    tempRow = List.Select("id=" + r["chargeID"].ToString());

                                    if (tempRow.Length == 1)
                                    {
                                        r["chargeID"] = tempRow[0]["webId"];
                                    }
                                    else
                                    {
                                        Logger.Write("Error while Assign LiveID to Transactions");
                                        Console.WriteLine("Error while Assign LiveID to Transactions");

                                    }

                                }

                                DataRow[] foundrows;
                                foundrows = iTrans.Select("Payment<>0");
                                if (foundrows.Length > 0)
                                {

                                    if (!syncDT_uploadTransaction(foundrows.CopyToDataTable()))
                                    {
                                        Logger.Write("Error while Uploading Transactions");
                                        Console.WriteLine("Error while Uploading Transactions");
                                        return false;
                                    }
                                }


                                foundrows = iTrans.Select("Payment=0");
                                if (foundrows.Length > 0)
                                {
                                    if (!syncDT_uploadTransaction(foundrows.CopyToDataTable()))
                                    {
                                        Logger.Write("Error while Uploading Transactions");
                                        Console.WriteLine("Error while Uploading Transactions");
                                        return false;
                                    }
                                }
                            }



                        }


                    }
                    else
                    {
                        Logger.Write("Error while upload Charges list");
                        Console.WriteLine("Error while upload Charges list");
                        return false;
                    }

                }

                pageNum++;
            }

            return true;
        }

 

        public bool syncDT_uploadChargesNew(ref DataTable iPat, ref DataTable iIns, ref DataTable iPract, ref DataTable iDoc, ref DataTable iCPT)      
                {
            DataTable tblToUpload = DatabaseReader.ReadSQLDataByCommand(@"
                    select PracticeId, PatientId, CPTId, InsuranceCompanyId, DoctorId as PhysicianId, 805 as ClientID, 
                    DateOfService, DateOfPosting,Billed,Payment,Adjustment,Balance, Id  
                    from [collectionsystem-imports].dbo.charges ch where id > 0 and liveID is null");



            DataTable initList = tblToUpload.Copy();

            DataColumn colInt64 = new DataColumn("webID");
            colInt64.DataType = System.Type.GetType("System.Int64");
            initList.Columns.Add(colInt64);

            int pageNum = 1;
            int pageSize = 100;

            DataTable List = new DataTable();
            if (tblToUpload.Rows.Count > 0)
            {

                while ((pageNum - 1)*pageSize  < tblToUpload.Rows.Count )
                {
                    
                    List = tblToUpload.Rows.Cast<System.Data.DataRow>().Skip((pageNum - 1) * pageSize).Take(pageSize).CopyToDataTable();

                    if (List.Rows.Count > 0)
                    {

                        foreach (DataRow lRow in List.Rows)
                        {
                            long webID;

                            webID = getWebID(iPat, (long)lRow["PatientID"]);
                            if (webID == -1) return false;
                            lRow["PatientID"] = webID;

                            webID = getWebID(iIns, (long)lRow["InsuranceCompanyId"]);
                            if (webID == -1) return false;
                            lRow["InsuranceCompanyId"] = webID;

                            webID = getWebID(iPract, (long)lRow["PracticeID"]);
                            if (webID == -1) return false;
                            lRow["PracticeID"] = webID;

                            webID = getWebID(iDoc, (long)lRow["PhysicianId"]);
                            if (webID == -1) return false;
                            lRow["PhysicianId"] = webID;

                            webID = getWebID(iCPT, (long)lRow["CPTId"]);
                            if (webID == -1) return false;
                            lRow["CPTId"] = webID;

                        }


                        if (syncDT_uploadChargesList(ref List, false))
                        {
                            //update LiveID 

                            StringBuilder sb_tmp = new StringBuilder();
                            sb_tmp.AppendLine("BEGIN TRANSACTION tr1 ");
                            sb_tmp.AppendLine(@"DECLARE @PreTbl TABLE ([Id] BIGINT, [webID] BIGINT);");
                            foreach (DataRow row in List.Rows)
                            {
                                sb_tmp.Append("insert into @PreTbl (id, webID) values (");
                                sb_tmp.Append(row["id"].ToString());
                                sb_tmp.Append(",");
                                sb_tmp.Append(row["webid"].ToString());
                                sb_tmp.AppendLine(")");
                            }
                            sb_tmp.AppendLine(@"UPDATE [CollectionSystem-Imports].[dbo].[Charges] 
                            SET [CollectionSystem-Imports].[dbo].[Charges].LiveID = pTbl.webId, updated = 0
                            FROM [CollectionSystem-Imports].[dbo].[Charges] INNER JOIN 
                            @PreTbl AS pTbl On [CollectionSystem-Imports].[dbo].[Charges].id = pTbl.Id WHERE pTbl.Id > 0 and liveID is null; ");


                            sb_tmp.AppendLine("COMMIT TRANSACTION tr1;");

                            bool res = MultipleExecutor.ExecuteTimes(5, sb_tmp.ToString(), "Charges");

                            int emptyList = 0;
                            StringBuilder idList = new StringBuilder();
                            if (res == true)
                            {

                                Logger.Write("Charges uploaded: " + List.Rows.Count.ToString());
                                Console.WriteLine("Charges uploaded: " + List.Rows.Count.ToString());


                                // insert transactions 


                                idList.Insert(0, "select [DateOfService],[DateOfPosting],[TransactionId],[Billed],[Payment],[Adjustment],[Balance],[ChargeId] from [collectionsystem-imports].dbo.transactions where chargeid in (");


                                foreach (DataRow row in List.Rows)
                                {

                                    if (emptyList == 0)
                                    {
                                        idList.Append(row["id"].ToString());
                                    }
                                    else
                                    {
                                        idList.Append(row["id"].ToString());
                                    }
                                    emptyList++;
                                    if (emptyList < List.Rows.Count) idList.Append(",");
                                }
                            }
                            else
                            {
                                Logger.Write("Error while Update Charges LiveID");
                                Console.WriteLine("Error while Update Charges LiveID");
                                return false;
                            }

                            if (emptyList > 0)
                            {

                                idList.Append(");");

                                DataTable iTrans = DatabaseReader.ReadSQLDataByCommand(idList.ToString());

                                if (iTrans.Rows.Count > 0)
                                {

                                    foreach (DataRow r in iTrans.Rows)
                                    {
                                        DataRow[] tempRow;

                                        tempRow = List.Select("id=" + r["chargeID"].ToString());

                                        if (tempRow.Length == 1)
                                        {
                                            r["chargeID"] = tempRow[0]["webId"];
                                        }
                                        else
                                        {
                                            Logger.Write("Error while Assign LiveID to Transactions");
                                            Console.WriteLine("Error while Assign LiveID to Transactions");

                                        }

                                    }
                                    DataRow[] foundrows;
                                    foundrows = iTrans.Select("Payment<>0");
                                    if (foundrows.Length > 0)
                                    {
                                        if (!syncDT_uploadTransaction(foundrows.CopyToDataTable()))
                                        {
                                            Logger.Write("Error while Uploading Transactions");
                                            Console.WriteLine("Error while Uploading Transactions");
                                            return false;
                                        }
                                    }

                                    foundrows = iTrans.Select("Payment=0");
                                    if (foundrows.Length > 0)
                                    {
                                        if (!syncDT_uploadTransaction(foundrows.CopyToDataTable()))
                                        {
                                            Logger.Write("Error while Uploading Transactions");
                                            Console.WriteLine("Error while Uploading Transactions");
                                            return false;
                                        }
                                    }
                                }



                            }


                        }
                        else
                        {
                            Logger.Write("Error while upload Charges list");
                            Console.WriteLine("Error while upload Charges list");
                            return false;
                        }

                    }

                    pageNum++;
                }
            }
            return true;
        }

        public long getWebID(DataTable iTbl, Int64 id)
        {
            DataRow[] foundrows = iTbl.Select("Id=" + id.ToString());
            if (foundrows.Length == 1)
            {
                return (long)foundrows[0]["webid"];
            }
            else
            {
                Logger.Write("Error while WebID search " );
                Console.WriteLine("Error while WebID search " );

            }
            return -1;
        }

        public bool syncDT_uploadTransaction(DataTable iTrans)
        {

            if (iTrans.Rows.Count == 0) return true;

            DataSet ds = new DataSet();
            DataTable List = iTrans.Copy();
            List.TableName = "List";
            ds.Tables.Add(List);

            string req = JsonConvert.SerializeObject(ds, Newtonsoft.Json.Formatting.Indented);
            string resp = HttpHandler.PostJson(srvAPIURL + "/Transaction/SaveList", req);
            DataSet rds = new DataSet();

            rds = ReadDataFromJson(resp);
            int uploadCnt = List.Rows.Count;

            string respStatus = rds.Tables[0].Rows[0]["Status"].ToString();

            if (respStatus != "true")
            {
                Logger.Write("Error while upload transactions: " + resp);
                Console.WriteLine("Error while upload transactions: " + resp);
                return false;
            }
            else
            {
                Logger.Write("Transactions uploaded: " + List.Rows.Count.ToString());
                Console.WriteLine("Transactions uploaded: " + List.Rows.Count.ToString());


            }
            return true; 
        }

        private bool syncDT_uploadChargesList(ref DataTable List, bool updateMode)
        {

            DataTable initList = List.Copy();
            
            
            List.TableName = "List";
            
            if (updateMode)
            {
                if (List.Columns["id"] != null)
                {
                    List.Columns.Remove("id");
                }
                List.Columns["liveId"].ColumnName = "id";

                foreach (DataRow charge in List.Rows)
                {
                    long i = long.Parse( charge["id"].ToString()) ;
                    syncDT_deleteTransaction(i);
                }
            }
            else
            {
                if (List.Columns["webID"] != null)
                {
                    List.Columns.Remove("WebId");
                }
                if (List.Columns["id"] != null)
                {
                    List.Columns.Remove("id");
                }

            }



            DataSet ds = new DataSet();
            List.TableName = "List";
            ds.Tables.Add(List);


            string req = JsonConvert.SerializeObject(ds, Newtonsoft.Json.Formatting.Indented);
            string resp = HttpHandler.PostJson(srvAPIURL + "/Charge/SaveList", req);


            DataSet rds = new DataSet();
            DataTable wCharge;
            rds = ReadDataFromJson(resp);
            int uploadCnt = List.Rows.Count;
            if (rds.Tables.Count ==0)
            {
                Logger.Write("Server sent error respond while charges list save");
                Console.WriteLine("Server sent error respond while charges list save");
                return false;

            }

            string respStatus = rds.Tables[0].Rows[0]["Status"].ToString();
            //var errCnt = rds.Tables[0].Rows[0]["Errors"].ToString();
            if (respStatus == "true")
            {
                if (rds.Tables["Model"] != null)
                {
                    wCharge = rds.Tables["Model"];
                }
                else
                {

                    wCharge = new DataTable();
                    DataColumn colInt16 = new DataColumn("id");
                    colInt16.DataType = System.Type.GetType("System.Int64");
                    wCharge.Columns.Add(colInt16);
                    wCharge.Rows.Add();
                    wCharge.Rows[0][0] = rds.Tables[0].Rows[0]["Model"];
                }
                int i = 0;
                if (uploadCnt == wCharge.Rows.Count)
                {
                    if (List.Columns["id"] == null)
                    {
                        DataColumn id = new DataColumn("id");
                        id.DataType = System.Type.GetType("System.Int64");
                        List.Columns.Add(id);
                    }
                    DataColumn webID = new DataColumn("webID");
                    webID.DataType = System.Type.GetType("System.Int64");
                    List.Columns.Add(webID);

                    DataRow[] foundRows;

                    foreach (DataRow row in List.Rows)
                    {

                        row["id"] = initList.Rows[i]["id"]; 
                        foundRows = List.Select("id=" + row["id"].ToString());
                        if (foundRows.Length == 1)
                        {
                            foundRows[0]["webID"] = wCharge.Rows[i][0];
                        }
                        else
                        {
                            Logger.Write("Cant assign LiveId to Charge from respond");
                            Console.WriteLine("Cant assign LiveId to Charge from respond");
                            return false;
                        }
                        i++;
                    }

                    foundRows = List.Select("webID=0");
                    if (foundRows.Length != 0)
                    {
                        Logger.Write("Cant assign LiveId to Charge from respond");
                        Console.WriteLine("Cant assign LiveId to Charge from respond");
                        return false;
                    }
                }
                else
                {
                    Logger.Write("Got improper respond from server Charge upload");
                    Console.WriteLine("Got improper respond from server Charge upload");
                    return false;
                }
            }
            else
            {
                Logger.Write("Got improper respond from server CPT sync");
                Console.WriteLine("Got improper respond from server CPT sync");
                return false;
            }



            return true;
        }

        public bool syncDT_deleteTransaction(Int64 Id)
        {
            StringBuilder s = new StringBuilder();
            s.Append("{\"id\":");
            s.Append(Id.ToString());
            s.Append("}");

            string resp = HttpHandler.PostJson(srvAPIURL + "/Transaction/KillByCharge", s.ToString());

            DataSet rds = new DataSet();
            rds = ReadDataFromJson(resp);
            string respStatus = "";
            if (rds.Tables.Count > 0)
            {
                respStatus = rds.Tables[0].Rows[0]["Status"].ToString();
            }
            if (respStatus == "true")
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool syncDT_deleteCharges()
        {

            DataTable tblToDelete = DatabaseReader.ReadSQLDataByCommand("select id, liveid from [collectionsystem-imports].dbo.charges where id > 0 and toDelete=1");

            foreach (DataRow r in tblToDelete.Rows)
            {

                StringBuilder s = new StringBuilder();
                s.Append("{\"id\":");
                s.Append(r["LiveId"].ToString());
                s.Append("}");

                StringBuilder cmd = new StringBuilder();
                
                if (syncDT_deleteTransaction((Int64)r["LiveId"]))
                {

                    string resp = HttpHandler.PostJson(srvAPIURL + "/Charge/Kill", s.ToString());
                    DataSet rds = ReadDataFromJson(resp);
                    if (rds.Tables.Count > 0)
                    {
                        string respStatus = rds.Tables[0].Rows[0]["Status"].ToString();

                        if (respStatus == "true")
                        {
                            cmd.AppendLine("delete from [collectionsystem-imports].dbo.transactions where chargeid=" + r["id"].ToString() + ";");
                            cmd.AppendLine("Delete from [collectionsystem-imports].dbo.charges where id=");
                            cmd.Append(r["Id"].ToString() + ";");
                            Logger.Write("Deleting Charges ID/LiveID " + r["id"].ToString() + "/" + r["liveid"].ToString());
                            Console.WriteLine("Deleting Charges ID/LiveID " + r["id"].ToString() + "/" + r["liveid"].ToString());
                            MultipleExecutor.ExecuteTimes(5, cmd.ToString(), "Transactions & Charges");
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                }
                
            }
            return true;
        }

        public bool syncDTCPT(ref DataTable iCPT, DataTable wCPT)
        {
            if (iCPT.Columns["webID"] == null)
            {
                DataColumn colInt64 = new DataColumn("webID");
                colInt64.DataType = System.Type.GetType("System.Int64");

                iCPT.Columns.Add(colInt64);
            }
            /*            if (iPract.Columns["ClientID"] == null)
                        {
                            DataColumn colInt16 = new DataColumn("Clientid");
                            colInt16.DataType = System.Type.GetType("System.Int16");

                            iPract.Columns.Add(colInt16);
                        }
            */
            int i = 0;
            foreach (DataRow row in iCPT.Rows)
            {

                //                row["ClientID"] = 805;

                DataRow[] foundRows;

                foundRows = wCPT.Select("Name='" + row["Name"].ToString().Replace("\'", "\'\'").Replace("\"", "\"\"") + "'");
                if (foundRows.Length == 1)
                {
                    row["webID"] = foundRows[0]["Id"];
                }
                else
                {
                    // doublecheck 
                    foundRows = wCPT.Select("trim(Name)='" + row["Name"].ToString() + "'");

                    switch (foundRows.Length)
                    {

                        case 0:
                            row["webID"] = 0;
                            break;
                        case 1:
                            row["webID"] = foundRows[0]["ID"];
                            break;

                        default:
                            row["webID"] = -foundRows.Length;
                            Logger.Write("Found more then 1 matches for CPT " + row["Name"].ToString());
                            Console.WriteLine("Found more then 1 matches for CPT" + row["Name"].ToString());
                            break;
                    }
                }
                i++;
            }

            if (!uploadCPT(ref iCPT))
            {
                return false;
            }
            GC.Collect();
            return true;
        }

        public bool uploadCPT(ref DataTable iCPT)
        {
            DataRow[] foundRows;

            foundRows = iCPT.Select("webID=0");

            if (foundRows.Length > 0)
            {
                DataTable List = foundRows.CopyToDataTable();
                List.TableName = "List";


                DataTable initList = List.Copy();
                List.Columns.Remove("id");
                if (List.Columns["id"] == null)
                {
                    DataColumn colInt16 = new DataColumn("id");
                    colInt16.DataType = System.Type.GetType("System.Int64");
                    List.Columns.Add(colInt16);
                }



                int uploadCnt = List.Rows.Count;
                List.Columns.Remove("webId");
                DataSet ds = new DataSet();
                ds.Tables.Add(List);

                string req = JsonConvert.SerializeObject(ds, Newtonsoft.Json.Formatting.Indented);
                string resp = HttpHandler.PostJson(srvAPIURL + "/CPT/SaveList", req);


                DataSet rds = new DataSet();
                DataTable wCPT;
                rds = ReadDataFromJson(resp);


                string respStatus = rds.Tables[0].Rows[0]["Status"].ToString();
                //var errCnt = rds.Tables[0].Rows[0]["Errors"].ToString();
                if (respStatus == "true")
                {
                    if (rds.Tables["Model"] != null)
                    {
                        wCPT = rds.Tables["Model"];
                    }
                    else
                    {

                        wCPT = new DataTable();
                        DataColumn colInt16 = new DataColumn("id");
                        colInt16.DataType = System.Type.GetType("System.Int64");
                        wCPT.Columns.Add(colInt16);
                        wCPT.Rows.Add();
                        wCPT.Rows[0][0] = rds.Tables[0].Rows[0]["Model"];
                    }
                    int i = 0;
                    if (uploadCnt == wCPT.Rows.Count)
                    {
                        DataColumn webID = new DataColumn();
                        webID.DataType = System.Type.GetType("System.Int64");
                        initList.Columns.Add(webID);

                        foreach (DataRow row in initList.Rows)
                        {

                            foundRows = iCPT.Select("id=" + row["id"].ToString());
                            if (foundRows.Length == 1)
                            {
                                foundRows[0]["webID"] = wCPT.Rows[i][0];
                            }
                            else
                            {
                                Logger.Write("Cant assign CPT's webID from respond");
                                Console.WriteLine("Cant assign CPT's webID from respond");
                                return false;
                            }
                            i++;
                        }

                        foundRows = iCPT.Select("webID=0");
                        if (foundRows.Length != 0)
                        {
                            Logger.Write("Cant assign CPT's webID from respond");
                            Console.WriteLine("Cant assign CPT's webID from respond");
                            return false;
                        }
                    }
                    else
                    {
                        Logger.Write("Got improper respond from server CPT sync");
                        Console.WriteLine("Got improper respond from server CPT sync");
                        return false;
                    }
                }
                else
                {
                    Logger.Write("Got improper respond from server CPT sync");
                    Console.WriteLine("Got improper respond from server CPT sync");
                    return false;
                }

            }
            return true;
        }



        public bool syncDTDoc(ref DataTable iDoc,DataTable wDoc)
        {
            if (iDoc.Columns["webID"] == null)
            {
                DataColumn colInt64 = new DataColumn("webID");
                colInt64.DataType = System.Type.GetType("System.Int64");

                iDoc.Columns.Add(colInt64);
            }
/*            if (iPract.Columns["ClientID"] == null)
            {
                DataColumn colInt16 = new DataColumn("Clientid");
                colInt16.DataType = System.Type.GetType("System.Int16");

                iPract.Columns.Add(colInt16);
            }
*/            
            int i = 0;
            foreach (DataRow row in iDoc.Rows)
            {

//                row["ClientID"] = 805;

                DataRow[] foundRows;

                foundRows = wDoc.Select("ProviderId='" + row["ProviderID"].ToString().Replace("\'", "\'\'").Replace("\"", "\"\"") + "'");
                if (foundRows.Length == 1)
                {
                    row["webID"] = foundRows[0]["Id"];
                }
                else
                {
                    // doublecheck 
                    foundRows = wDoc.Select("trim(ProviderID)='" + row["ProviderID"].ToString() + "'");

                    switch (foundRows.Length)
                    {

                        case 0:
                            row["webID"] = 0;
                            break;
                        case 1:
                            row["webID"] = foundRows[0]["ID"];
                            break;

                        default:
                            row["webID"] = -foundRows.Length;
                            Logger.Write("Found more then 1 matches for Doctor " + row["Name"].ToString());
                            Console.WriteLine("Found more then 1 matches for Doctor" + row["Name"].ToString());
                            break;
                    }
                }
                i++;
            }

            if (!uploadDoc(ref iDoc))
            {
                return false;
            }
            GC.Collect();
            return true;
        }

        public bool uploadDoc(ref DataTable iDoc)
        {
            DataRow[] foundRows;

            foundRows = iDoc.Select("webID=0");
            foreach (DataRow r in foundRows)
            {
                if (r["Firstname"].ToString().Trim() == "") r["Firstname"] = "_";
            }

            if (foundRows.Length > 0)
            {
                DataTable List = foundRows.CopyToDataTable();
                List.TableName = "List";


                DataTable initList = List.Copy();
                List.Columns.Remove("id");
                if (List.Columns["id"] == null)
                {
                    DataColumn colInt16 = new DataColumn("id");
                    colInt16.DataType = System.Type.GetType("System.Int64");
                    List.Columns.Add(colInt16);
                }



                int uploadCnt = List.Rows.Count;
                List.Columns.Remove("webId");
                DataSet ds = new DataSet();
                ds.Tables.Add(List);

                string req = JsonConvert.SerializeObject(ds, Newtonsoft.Json.Formatting.Indented);
                string resp = HttpHandler.PostJson(srvAPIURL + "/Physician/SaveList", req);


                DataSet rds = new DataSet();
                DataTable wDoc;
                rds = ReadDataFromJson(resp);


                string respStatus = rds.Tables[0].Rows[0]["Status"].ToString();
                //var errCnt = rds.Tables[0].Rows[0]["Errors"].ToString();
                if (respStatus == "true")
                {
                    if (rds.Tables["Model"] != null)
                    {
                        wDoc = rds.Tables["Model"];
                    }
                    else
                    {

                        wDoc = new DataTable();
                        DataColumn colInt16 = new DataColumn("id");
                        colInt16.DataType = System.Type.GetType("System.Int64");
                        wDoc.Columns.Add(colInt16);
                        wDoc.Rows.Add();
                        wDoc.Rows[0][0] = rds.Tables[0].Rows[0]["Model"];
                    }
                    int i = 0;
                    if (uploadCnt == wDoc.Rows.Count)
                    {
                        DataColumn webID = new DataColumn();
                        webID.DataType = System.Type.GetType("System.Int64");
                        initList.Columns.Add(webID);

                        foreach (DataRow row in initList.Rows)
                        {

                            foundRows = iDoc.Select("id=" + row["id"].ToString());
                            if (foundRows.Length == 1)
                            {
                                foundRows[0]["webID"] = wDoc.Rows[i][0];
                            }
                            else
                            {
                                Logger.Write("Cant assign Doc's webID from respond");
                                Console.WriteLine("Cant assign Doc's webID from respond");
                                return false;
                            }
                            i++;
                        }

                        foundRows = iDoc.Select("webID=0");
                        if (foundRows.Length != 0)
                        {
                            Logger.Write("Cant assign Doc's webID from respond");
                            Console.WriteLine("Cant assign Doc's webID from respond");
                            return false;
                        }
                    }
                    else
                    {
                        Logger.Write("Got improper respond from server Doc sync");
                        Console.WriteLine("Got improper respond from server Doc sync");
                        return false;
                    }
                }
                else
                {
                    Logger.Write("Got improper respond from server Doc sync");
                    Console.WriteLine("Got improper respond from server Doc sync");
                    return false;
                }

            }
            return true;
        }


        public bool syncDTPract(ref DataTable iPract, DataTable wPract)
        {
            if (iPract.Columns["webID"] == null)
            {
                DataColumn colInt64 = new DataColumn("webID");
                colInt64.DataType = System.Type.GetType("System.Int64");

                iPract.Columns.Add(colInt64);
            }
            if (iPract.Columns["ClientID"] == null)
            {
                DataColumn colInt16 = new DataColumn("Clientid");
                colInt16.DataType = System.Type.GetType("System.Int64");

                iPract.Columns.Add(colInt16);
            }


            int i = 0;
            foreach (DataRow row in iPract.Rows)
            {

                row["ClientID"] = 805;

                DataRow[] foundRows;

                foundRows = wPract.Select("Name='" + row["Name"].ToString().Replace("\'", "\'\'").Replace("\"", "\"\"") + "'");
                if (foundRows.Length == 1)
                {
                    row["webID"] = foundRows[0]["Id"];
                }
                else
                {
                    // doublecheck 
                    foundRows = wPract.Select("trim(Name)='" + row["Name"].ToString() + "'");

                    switch (foundRows.Length)
                    {

                        case 0:
                            row["webID"] = 0;
                            break;
                        case 1:
                            row["webID"] = foundRows[0]["ID"];
                            break;

                        default:
                            row["webID"] = -foundRows.Length;
                            Logger.Write("Found more then 1 matches for Practice " + row["Name"].ToString());
                            Console.WriteLine("Found more then 1 matches for Practice" + row["Name"].ToString());
                            break;
                    }
                }
                i++;
            }

            if (!uploadPractice(ref iPract))
            {
                return false;
            }
            GC.Collect();
            return true;
        }


        public bool uploadPractice(ref DataTable iPract)
        {
            DataRow[] foundRows;

            foundRows = iPract.Select("webID=0");
            if (foundRows.Length > 0)
            {
                DataTable List = foundRows.CopyToDataTable();
                List.TableName = "List";


                DataTable initList = List.Copy();
                List.Columns.Remove("id");
                if (List.Columns["id"] == null)
                {
                    DataColumn colInt16 = new DataColumn("id");
                    colInt16.DataType = System.Type.GetType("System.Int64");
                    List.Columns.Add(colInt16);
                }



                int uploadCnt = List.Rows.Count;
                List.Columns.Remove("webId");
                DataSet ds = new DataSet();
                ds.Tables.Add(List);

                string req = JsonConvert.SerializeObject(ds, Newtonsoft.Json.Formatting.Indented);
                string resp = HttpHandler.PostJson(srvAPIURL + "/Practice/SaveList", req);


                DataSet rds = new DataSet();
                DataTable wPract;
                rds = ReadDataFromJson(resp);


                string respStatus = rds.Tables[0].Rows[0]["Status"].ToString();
                //var errCnt = rds.Tables[0].Rows[0]["Errors"].ToString();
                if (respStatus == "true")
                {
                    if (rds.Tables["Model"] != null)
                    {
                        wPract = rds.Tables["Model"];
                    }
                    else
                    {

                        wPract = new DataTable();
                        DataColumn colInt16 = new DataColumn("id");
                        colInt16.DataType = System.Type.GetType("System.Int64");
                        wPract.Columns.Add(colInt16);
                        wPract.Rows.Add();
                        wPract.Rows[0][0] = rds.Tables[0].Rows[0]["Model"];
                    }
                    int i = 0;
                    if (uploadCnt == wPract.Rows.Count)
                    {
                        DataColumn webID = new DataColumn();
                        webID.DataType = System.Type.GetType("System.Int64");
                        initList.Columns.Add(webID);

                        foreach (DataRow row in initList.Rows)
                        {

                            foundRows = iPract.Select("id=" + row["id"].ToString());
                            if (foundRows.Length == 1)
                            {
                                foundRows[0]["webID"] = wPract.Rows[i][0];
                            }
                            else
                            {
                                Logger.Write("Cant assign Practice's webID from respond");
                                Console.WriteLine("Cant assign Practice's webID from respond");
                                return false;
                            }
                            i++;
                        }

                        foundRows = iPract.Select("webID=0");
                        if (foundRows.Length != 0)
                        {
                            Logger.Write("Cant assign Practice's webID from respond");
                            Console.WriteLine("Cant assign Practice's webID from respond");
                            return false;
                        }
                    }
                    else
                    {
                        Logger.Write("Got improper respond from server Practice sync");
                        Console.WriteLine("Got improper respond from server Practice sync");
                        return false;
                    }
                }
                else 
                {
                    Logger.Write("Got improper respond from server Practice sync");
                    Console.WriteLine("Got improper respond from server Practice sync");
                    return false;
                }

            }
            return true;
        }
            public bool syncDTIns(ref DataTable iIns, DataTable wIns)
        {
            if (iIns.Columns["webID"] == null)
            {
                DataColumn colInt64 = new DataColumn("webID");
                colInt64.DataType = System.Type.GetType("System.Int64");

                iIns.Columns.Add(colInt64);
            }
            if (iIns.Columns["ClientID"] == null)
            {
                DataColumn colInt16 = new DataColumn("Clientid");
                colInt16.DataType = System.Type.GetType("System.Int64");
                iIns.Columns.Add(colInt16);
            }

            int i = 0;
            foreach (DataRow row in iIns.Rows)
            {
                row["ClientID"] = 805;
                DataRow[] foundRows;

                foundRows = wIns.Select("InsuranceId='" + row["InsuranceId"].ToString().Replace("\'","\'\'").Replace("\"","\"\"") + "'");
                if (foundRows.Length == 1)
                {
                    row["webID"] = foundRows[0]["Id"];
                }
                else
                {
                    // doublecheck 
                    foundRows = wIns.Select("trim(InsuranceID)='" + row["InsuranceId"].ToString() + "'");

                    switch (foundRows.Length)
                    {

                        case 0:
                            row["webID"] = 0;
                            break;
                        case 1:
                            row["webID"] = foundRows[0]["ID"];
                            break;

                        default:
                            row["webID"] = -foundRows.Length;
                            Logger.Write("Found more then 1 matches for Insurance " + row["InsuranceID"].ToString());
                            Console.WriteLine("Found more then 1 matches for Insurance" + row["InsuranceID"].ToString());
                            break;
                    }
                }
                i++;
            }

            if (!uploadInsurance(ref iIns))
            {
                return false;
            }
            GC.Collect();
            return true;
        }



        public bool uploadInsurance(ref DataTable iIns)
        {
            DataRow[] foundRows;

            foundRows = iIns.Select("webID=0");
            if (foundRows.Length > 0)
            {
                DataTable List = foundRows.CopyToDataTable();
                List.TableName = "List";


                DataTable initList = List.Copy();
                List.Columns.Remove("id");
                if (List.Columns["id"] == null)
                {
                    DataColumn colInt16 = new DataColumn("id");
                    colInt16.DataType = System.Type.GetType("System.Int64");
                    List.Columns.Add(colInt16);
                }




                    int uploadCnt = List.Rows.Count;
                List.Columns.Remove("webId");
                DataSet ds = new DataSet();
                ds.Tables.Add(List);

                string req = JsonConvert.SerializeObject(ds, Newtonsoft.Json.Formatting.Indented);
                string resp = HttpHandler.PostJson(srvAPIURL + "/InsuranceCompany/SaveList", req);


                DataSet rds = new DataSet();
                DataTable wIns;
                rds = ReadDataFromJson(resp);


                string respStatus = rds.Tables[0].Rows[0]["Status"].ToString();
                //var errCnt = rds.Tables[0].Rows[0]["Errors"].ToString();
                if (respStatus == "true" )
                {
                    if (rds.Tables["Model"] != null)
                    {
                        wIns = rds.Tables["Model"];
                    }
                    else
                    {

                        wIns = new DataTable();
                        DataColumn colInt16 = new DataColumn("id");
                        colInt16.DataType = System.Type.GetType("System.Int64");
                        wIns.Columns.Add(colInt16);
                        wIns.Rows.Add();
                        wIns.Rows[0][0] = rds.Tables[0].Rows[0]["Model"] ;
                    }
                    int i = 0;
                    if (uploadCnt == wIns.Rows.Count)
                    {
                        DataColumn webID = new DataColumn();
                        webID.DataType = System.Type.GetType("System.Int64");
                        initList.Columns.Add(webID);

                        foreach (DataRow row in initList.Rows)
                        {

                            foundRows = iIns.Select("id=" + row["id"].ToString());
                            if (foundRows.Length == 1)
                            {
                                foundRows[0]["webID"] = wIns.Rows[i][0];
                            }
                            else
                            {
                                Logger.Write("Cant assign insurer's webID from respond");
                                Console.WriteLine("Cant assign insurer's webID from respond");
                                return false;
                            }
                            i++;
                        }

                        foundRows = iIns.Select("webID=0");
                        if (foundRows.Length != 0)
                        {
                            Logger.Write("Cant assign insurer's webID from respond");
                            Console.WriteLine("Cant assign insurer's webID from respond");
                            return false;
                            }
                    }
                    else
                    {
                        Logger.Write("Got improper respond from server insurers sync");
                        Console.WriteLine("Got improper respond from server insurers sync");
                        return false;
                    }
                }
                else
                {
                    Logger.Write("Got improper respond from server insurers sync");
                    Console.WriteLine("Got improper respond from server insurers sync");
                    return false;
                }

            }
            return true;
        }

        public bool syncDTPatients(ref DataTable iPat, DataTable wPat)
        {
            if (iPat.Columns["webID"] ==null) 
            {
                DataColumn colInt64 = new DataColumn("webID");
                colInt64.DataType = System.Type.GetType("System.Int64");

                iPat.Columns.Add(colInt64);
            }
            if (iPat.Columns["ClientID"] == null)
            {
                DataColumn colInt16 = new DataColumn("Clientid");
                colInt16.DataType = System.Type.GetType("System.Int64");

                iPat.Columns.Add(colInt16);
            }

            iPat.Columns["AccID"].ColumnName = "Accountid";
            int i = 0;
            foreach (DataRow row in iPat.Rows)
            {
                row["ClientID"] = 805;
                DataRow[] foundRows;

                foundRows = wPat.Select("AccountID='" + row["Accountid"].ToString() + "'");
                if (foundRows.Length == 1)
                {
                    row["webID"] = foundRows[0]["ID"];
                }
                else
                {
                    // doublecheck 
                    foundRows = wPat.Select("trim(AccountID)='" + row["Accountid"].ToString() + "'");

                    switch (foundRows.Length)
                    {

                        case 0:
                            row["webID"] = 0;
                            break;
                        case 1:
                            row["webID"] = foundRows[0]["ID"];
                            break;

                        default:
                            row["webID"] = -foundRows.Length;
                            Logger.Write("Found more then 1 matches for Account " + row["AccID"].ToString() );
                            Console.WriteLine("Found more then 1 matches for Account " + row["AccID"].ToString());
                            break;
                    }
                }
                i++;
            }

            if (!uploadPatients(ref iPat) ) 
            {
                return false;
            }

            GC.Collect();
            

            return true;
        }


        public bool uploadPatients(ref DataTable iPat)
        {
            DataRow[] foundRows;

            foundRows = iPat.Select("webID=0");
            if (foundRows.Length > 0)
            {
                DataTable List = foundRows.CopyToDataTable();
                List.TableName = "List";


                DataTable initList = List.Copy();
                List.Columns.Remove("id");
                if (List.Columns["id"] == null)
                {
                    DataColumn colInt16 = new DataColumn("id");
                    colInt16.DataType = System.Type.GetType("System.Int64");
                    List.Columns.Add(colInt16);
                }

                int uploadCnt = List.Rows.Count;
                List.Columns.Remove("webId");
                DataSet ds = new DataSet();
                ds.Tables.Add(List);

                string req = JsonConvert.SerializeObject(ds, Newtonsoft.Json.Formatting.Indented);
                string resp = HttpHandler.PostJson(srvAPIURL + "/Patient/SaveList", req);


                DataSet rds = new DataSet();
                DataTable wPat;
                rds = ReadDataFromJson(resp);


                string respStatus = rds.Tables[0].Rows[0]["Status"].ToString();
                //var errCnt = rds.Tables[0].Rows[0]["Errors"].ToString();
                if (respStatus == "true" )
                {

                    if (rds.Tables["Model"] != null)
                    {
                        wPat = rds.Tables["Model"];
                    }
                    else
                    {
                        wPat = new DataTable();
                        DataColumn colInt16 = new DataColumn("id");
                        colInt16.DataType = System.Type.GetType("System.Int64");
                        wPat.Columns.Add(colInt16);
                        wPat.Rows.Add();
                        wPat.Rows[0][0] = rds.Tables[0].Rows[0]["Model"];
                    }
                    
                    int i = 0;
                    if (uploadCnt == wPat.Rows.Count)
                    {
                        DataColumn webID = new DataColumn();
                        webID.DataType = System.Type.GetType("System.Int64");
                        initList.Columns.Add(webID);

                        foreach (DataRow row in initList.Rows)
                        {

                            foundRows = iPat.Select("id=" + row["id"].ToString());
                            if (foundRows.Length == 1)
                            {
                                foundRows[0]["webID"] = wPat.Rows[i][0];
                            }
                            else
                            {
                                Logger.Write("Cant assign patient webID from respond");
                                Console.WriteLine("Cant assign patient webID from respond");
                                return false;
                            }
                            i++;
                        }

                        foundRows = iPat.Select("webID=0");
                        if (foundRows.Length != 0) 
                        {
                            Logger.Write("Cant assign patient webID from respond");
                            Console.WriteLine("Cant assign patient webID from respond");
                                return false;
                        }
                    }
                
                
                else
                {
                        Logger.Write("Got improper respond from server patients sync");
                        Console.WriteLine("Got improper respond from server patients sync");
                        return false;

                    }


                }
               
            }
            return true;
        }

       
        public DataTable getSQLData()
        {

            DataTable dt = new DataTable();
            string connString = @"your connection string here";
            string query = "select * from table";

            SqlConnection conn = new SqlConnection(connString);
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();
            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dt);
            conn.Close();
            da.Dispose();

            return dt;
        }



        bool SyncPatientsSplitted()
        {
            SqlDataReader patients = null;
            DatabaseReader.ReadPatients(ref patients);

            if (patients == null)
            {
                Logger.Write("Nothing to sync on patients table");
                Console.WriteLine("Nothing to sync on patients table");
                return true;
            }
            bool list = false;
            List<SyncEntities.SyncPatient> listPat = new List<SyncEntities.SyncPatient>();

            Dictionary<string, List<SyncEntities.SyncPatient>> requests_pat = Request.CreatePatientsRequestList(ref patients);
            patients.Close();
            /* string req = Request.CreatePatientsRequest(
                 ref patients,
                 ref list,
                 ref listPat);*/


            /*if (String.IsNullOrEmpty(req))
            {
                Logger.Write("Sync patients - already done");
                patients.Close();
                return true;
            }*/

            // Logger.Write("Request - " + req);


            if (requests_pat != null)
            {
                foreach (var patient in requests_pat)
                {
                    string response = null;
                    response = HttpHandler.PostJson(srvAPIURL + "/Patient/SaveList", patient.Key);
                    //Logger.Write("Sync patients table respomse- " + response);       



                    List<string> newIds = Utils.JsonUtil.GetListOfReturnedIds(response);
                    if (newIds.Count == listPat.Count)
                    {
                        for (int j = 0; j < newIds.Count; ++j)
                        {
                            listPat[j].NewId = newIds[j];

                        }
                    }


                    StringBuilder sb_tmp = new StringBuilder(
                        @"DECLARE @PreTbl TABLE ([Id] BIGINT, 
                                         [NewId] BIGINT, 
                                         [PolicyNumber] nvarchar(50)
                                        ,[AccountId] nvarchar(50)
                                        ,[FirstName] nvarchar(50)
                                        ,[MiddleName] nvarchar(50)
                                        ,[LastName] nvarchar(50)
                                        ,[DOB] datetime
                                        ,[UpdateDate] datetime
                                        ,[CreateDate] datetime
                                        ,[SSN] nvarchar(50)
                                        ,[Policy] nvarchar(50)
                                        ,[Access] bit);");

                    string r = @"INSERT INTO @PreTbl
                     ([Id]
                     ,[NewId]
                     ,[PolicyNumber]
                     ,[AccountId]
                     ,[FirstName]
                     ,[MiddleName]
                     ,[LastName]
                     ,[DOB]
                     ,[UpdateDate]
                     ,[CreateDate]
                     ,[SSN]
                     ,[Policy]
                     ,[Access])
                      VALUES ";

                    sb_tmp.Append(r);


                    int k = 0;
                    foreach (var o in listPat)
                    {
                        if (k > 0)
                        {
                            sb_tmp.Append(",");
                        }

                        if (k > 999)
                        {
                            sb_tmp.Remove(sb_tmp.Length - 1, 1);
                            sb_tmp.Append(";");
                            sb_tmp.Append(r);
                            k = 0;
                        }

                        string first = o.FirstName;
                        if (first.Contains("'"))
                        {
                            first = o.FirstName.Replace("'", "''");
                        }

                        string middle = o.MiddleName;
                        if (middle.Contains("'"))
                        {
                            middle = o.MiddleName.Replace("'", "''");
                        }

                        string last = o.LastName;
                        if (last.Contains("'"))
                        {
                            last = o.LastName.Replace("'", "''");
                        }

                        int access = 1;
                        if (0 == String.Compare(o.Access, "true"))
                        {
                            access = 1;
                        }
                        else if (0 == String.Compare(o.Access, "false"))
                        {
                            access = 0;
                        }

                        string ssn = null;
                        if (!String.IsNullOrEmpty(o.SSN))
                            ssn = o.SSN;


                        sb_tmp.AppendFormat("({0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', {12}) ",
                       o.Id, o.NewId, o.PolicyNumber, o.AccountId, first, middle, last, o.DOB, o.UpdateDate,
                       o.CreateDate, ssn, o.Policy, access);

                        ++k;
                    }

                    sb_tmp.Append("\r\n");
                    sb_tmp.Append("BEGIN TRANSACTION tr1 ALTER TABLE [CollectionSystem-Imports].[dbo].[Charges] NOCHECK CONSTRAINT ALL ");
                    sb_tmp.Append("UPDATE [CollectionSystem-Imports].[dbo].[Charges] SET [CollectionSystem-Imports].[dbo].[Charges].PatientId = pTbl.NewId FROM [CollectionSystem-Imports].[dbo].[Charges] INNER JOIN @PreTbl AS pTbl On [CollectionSystem-Imports].[dbo].[Charges].PatientId = pTbl.Id WHERE pTbl.Id > 0; ");

                    sb_tmp.Append("ALTER TABLE[CollectionSystem-Imports].[dbo].[Patients] NOCHECK CONSTRAINT ALL DELETE FROM [CollectionSystem-Imports].[dbo].[Patients] WHERE Id in (select Id from @PreTbl);");

                    sb_tmp.Append(@"set identity_insert [CollectionSystem-Imports].dbo.Patients on INSERT INTO[CollectionSystem-Imports].dbo.Patients
                         (Id, PolicyNumber, AccountId, FirstName, MiddleName, LastName, DOB, UpdateDate, CreateDate, SSN, Policy, Access)
SELECT        NewId, Policy, AccountId, FirstName, MiddleName, LastName, DOB, UpdateDate, CreateDate, SSN, Policy, Access
FROM @PreTbl where id > 0
set identity_insert [CollectionSystem-Imports].dbo.Patients off COMMIT TRANSACTION tr1");



                    MultipleExecutor.ExecuteTimes(5, sb_tmp.ToString(), "Patients");
                }
            }
            return true;
        }

        public DataTable getWebCPT()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();


            try
            {
                string json = getHTTPData("/CPT/List");
                ds = ReadDataFromJson(json);
                dt = ds.Tables["List"];
                return dt;
            }
            catch (Exception e)
            {
                Logger.Write("Unable to convert retrieved list of Doctiors from server into Table - " + e.ToString());
                return null;
            }
        }

        public DataTable getWebDoctor()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();


            try
            {
                string json = getHTTPData("/Physician/List");
                ds = ReadDataFromJson(json);
                dt = ds.Tables["List"];
                return dt;
            }
            catch (Exception e)
            {
                Logger.Write("Unable to convert retrieved list of Doctiors from server into Table - " + e.ToString());
                return null;
            }
        }

        public DataTable getWebPractice()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();


            try
            {
                string json = getHTTPData("/Practice/List"); //?clientId=805
                ds = ReadDataFromJson(json);
                dt = ds.Tables["List"];
                return dt;
            }
            catch (Exception e)
            {
                Logger.Write("Unable to convert retrieved list of Insurance Companies from server into Table - " + e.ToString());
                return null;
            }
        }



        public DataTable getWebInsurance()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();


            try
            {
                string json = getHTTPData("/InsuranceCompany/List?clientId=805");
                ds = ReadDataFromJson(json);
                dt = ds.Tables["List"];
                return dt;
            }
            catch (Exception e)
            {
                Logger.Write("Unable to convert retrieved list of Insurance Companies from server into Table - " + e.ToString());
                return null;
            }
        }

        public DataTable  getWebPatients()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            try
            {
                string json = getHTTPData("/Patient/List");//?clientId=805
                ds = ReadDataFromJson(json);
                dt = ds.Tables["List"];
                return dt;
            }
            catch (Exception e)
            {
                Logger.Write("Unable to convert retrieved list of Patients from server into Table - " + e.ToString());
                return null;
            }
        }

        private static DataSet ReadDataFromJson(String jsonString)
        {
            var result = new DataSet();
            try
            {
                var xd = new XmlDocument();
                jsonString = "{ \"rootNode\": {" + jsonString.Trim().TrimStart('{').TrimEnd('}') + @"} }";

                xd = JsonConvert.DeserializeXmlNode(jsonString);

                result.ReadXml(new XmlNodeReader(xd));
            }
            catch (Exception e)
            {
                Logger.Write("Error while server respond: " + e.ToString());
                Console.WriteLine("Error while server respond: " + e.ToString());
                return result;

            }
            return result;
        }

        public string getHTTPData(string APIcall)
        {
            try
            {     
                return HttpHandler.Get(srvAPIURL + APIcall);
            }
            catch (Exception e)
            {
                Logger.Write("Unable to retrieve list of Patients from server - " + e.ToString());
                return "";
            }
            
        }


        bool SyncPatients()
        {
            SqlDataReader patients = null;
            DatabaseReader.ReadPatients(ref patients);
            if(patients == null)
            {
                Logger.Write("Nothing to sync on patients table");
                Console.WriteLine("Nothing to sync on patients table");
                return true; 
            }
            bool list = false;
            List<SyncEntities.SyncPatient> listPat = new List<SyncEntities.SyncPatient>();
            string req = Request.CreatePatientsRequest(
                ref patients, 
                ref list, 
                ref listPat);
            if (String.IsNullOrEmpty(req))
            {
                Logger.Write("Sync patients - already done");
                Console.WriteLine("Sync patients - already done");
                patients.Close();
                return true; 
            }

           // Logger.Write("Request - " + req);

            string response = null;
            if (list)
            {
                response = HttpHandler.PostJson(srvAPIURL+"/Patient/SaveList", req);
                //Logger.Write("Sync patients table respomse- " + response);
            }
            else
            {           
                response = HttpHandler.PostJson(srvAPIURL+"/Patient/Save", req);
                //Logger.Write("Sync patients table response - " + response);
            }
            patients.Close();

            List<string> newIds = Utils.JsonUtil.GetListOfReturnedIds(response);
            if (newIds.Count == listPat.Count)
            {
                for (int j = 0; j < newIds.Count; ++j)
                {
                    listPat[j].NewId = newIds[j];

                }
            }

            if (newIds.Count > 0)
            {
                StringBuilder sb_tmp = new StringBuilder(
                    @"DECLARE @PreTbl TABLE ([Id] BIGINT, 
                                         [NewId] BIGINT, 
                                         [PolicyNumber] nvarchar(50)
                                        ,[AccountId] nvarchar(50)
                                        ,[FirstName] nvarchar(50)
                                        ,[MiddleName] nvarchar(50)
                                        ,[LastName] nvarchar(50)
                                        ,[DOB] datetime
                                        ,[UpdateDate] datetime
                                        ,[CreateDate] datetime
                                        ,[SSN] nvarchar(50)
                                        ,[Policy] nvarchar(50)
                                        ,[Access] bit);");

                string r = @"INSERT INTO @PreTbl
                     ([Id]
                     ,[NewId]
                     ,[PolicyNumber]
                     ,[AccountId]
                     ,[FirstName]
                     ,[MiddleName]
                     ,[LastName]
                     ,[DOB]
                     ,[UpdateDate]
                     ,[CreateDate]
                     ,[SSN]
                     ,[Policy]
                     ,[Access])
                      VALUES ";

                sb_tmp.Append(r);


                int k = 0;
                foreach (var o in listPat)
                {
                    if (k > 0)
                    {
                        sb_tmp.Append(",");
                    }

                    if (k > 999)
                    {
                        sb_tmp.Remove(sb_tmp.Length - 1, 1);
                        sb_tmp.Append(";");
                        sb_tmp.Append(r);
                        k = 0;
                    }

                    string first = o.FirstName;
                    if (first.Contains("'"))
                    {
                        first = o.FirstName.Replace("'", "''");
                    }

                    string middle = o.MiddleName;
                    if (middle.Contains("'"))
                    {
                        middle = o.MiddleName.Replace("'", "''");
                    }

                    string last = o.LastName;
                    if (last.Contains("'"))
                    {
                        last = o.LastName.Replace("'", "''");
                    }

                    int access = 1;
                    if (0 == String.Compare(o.Access, "true"))
                    {
                        access = 1;
                    }
                    else if (0 == String.Compare(o.Access, "false"))
                    {
                        access = 0;
                    }

                    string ssn = null;
                    if (!String.IsNullOrEmpty(o.SSN))
                        ssn = o.SSN;


                    sb_tmp.AppendFormat("({0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', {12}) ",
                   o.Id, o.NewId, o.PolicyNumber, o.AccountId, first, middle, last, o.DOB, o.UpdateDate,
                   o.CreateDate, ssn, o.Policy, access);

                    ++k;
                }

                sb_tmp.Append("\r\n");
                sb_tmp.Append("BEGIN TRANSACTION tr1 ALTER TABLE [CollectionSystem-Imports].[dbo].[Charges] NOCHECK CONSTRAINT ALL ");
                sb_tmp.Append("UPDATE [CollectionSystem-Imports].[dbo].[Charges] SET [CollectionSystem-Imports].[dbo].[Charges].PatientId = pTbl.NewId FROM [CollectionSystem-Imports].[dbo].[Charges] INNER JOIN @PreTbl AS pTbl On [CollectionSystem-Imports].[dbo].[Charges].PatientId = pTbl.Id WHERE pTbl.Id > 0; ");

                sb_tmp.Append("ALTER TABLE [CollectionSystem-Imports].[dbo].[Patients] NOCHECK CONSTRAINT ALL DELETE FROM [CollectionSystem-Imports].[dbo].[Patients] WHERE Id in (select Id from @PreTbl);");

                sb_tmp.Append(@"set identity_insert [CollectionSystem-Imports].dbo.Patients on INSERT INTO[CollectionSystem-Imports].dbo.Patients
                         (Id, PolicyNumber, AccountId, FirstName, MiddleName, LastName, DOB, UpdateDate, CreateDate, SSN, Policy, Access)
SELECT        NewId, Policy, AccountId, FirstName, MiddleName, LastName, DOB, UpdateDate, CreateDate, SSN, Policy, Access
FROM @PreTbl where id > 0
set identity_insert [CollectionSystem-Imports].dbo.Patients off COMMIT TRANSACTION tr1");



                return MultipleExecutor.ExecuteTimes(5, sb_tmp.ToString(), "Patients");
            }
            else return false;

}

        void SyncClients()
        {
            SqlDataReader clients = null;
            DatabaseReader.ReadClients(ref clients);
            if (clients == null)
            {
                Logger.Write("Nothing to sync on clients table");
                Console.WriteLine("Nothing to sync on clients table");
                return;
            }
            List<string> req_list = new List<string>();
            List<SyncEntities.SyncClient> listCli = new List<SyncEntities.SyncClient>();
            
            Request.CreateClientsRequest(ref clients, ref req_list, ref listCli);
            if (req_list.Count == 0)
            {
                Logger.Write("Sync clients - already done");
                Console.WriteLine("Sync clients - already done");
                clients.Close();
                return;
            }

            List<string> newIds = new List<string>();
            foreach (var req in req_list)
            {
                try {
                    string response = HttpHandler.PostJson(srvAPIURL + "/Client/Save", req);

                    //string rr = "{ \"Model\":808,\"Status\":true,\"Message\":null,\"Errors\":null}";

                    var vl = Utils.JsonUtil.GetFirstModelId(response);
                    newIds.Add(vl);
                    //Logger.Write("Sync clients table - " + response);
                }
                catch(Exception e)
                {

                }
            }

            clients.Close();


          //  if (newIds.Count == listCli.Count)
            {
                for (int j = 0; j < newIds.Count; ++j)
                {
                    listCli[j].NewId = newIds[j];

                }
            }


            StringBuilder sb_tmp = new StringBuilder(
                    @"DECLARE @PreTbl TABLE ([Id] BIGINT 
                                        ,[NewId] BIGINT
                                        ,[UpdateDate] datetime
                                        ,[CreateDate] datetime
                                        ,[Name] nvarchar(200)
                                        ,[Active] bit);");

            string r = @"INSERT INTO @PreTbl
                     ([Id]
                     ,[NewId]
                     ,[UpdateDate]
                     ,[CreateDate]
                     ,[Name]
                     ,[Active])
                      VALUES ";

            sb_tmp.Append(r);

            int k = 0;
            foreach (var o in listCli)
            {
                if (k > 0)
                {
                    sb_tmp.Append(",");
                }

                if (k > 999)
                {
                    sb_tmp.Remove(sb_tmp.Length - 1, 1);
                    sb_tmp.Append(";");
                    sb_tmp.Append(r);
                    k = 0;
                }

                string first = o.Name;
                if (first.Contains("'"))
                {
                    first = o.Name.Replace("'", "''");
                }

                sb_tmp.AppendFormat("({0}, {1}, '{2}', '{3}', '{4}', '{5}')", //'{2}', '{3}',
                o.Id, o.NewId, o.UpdateDate, o.CreateDate, first, o.Active);

                ++k;

            }

            sb_tmp.Append("\r\n");

            sb_tmp.Append("BEGIN TRANSACTION tr1 ALTER TABLE[CollectionSystem-Imports].[dbo].[Clients] NOCHECK CONSTRAINT ALL DELETE FROM [CollectionSystem-Imports].[dbo].[Clients] WHERE Id in (select Id from @PreTbl);");

            sb_tmp.Append(@"set identity_insert [CollectionSystem-Imports].dbo.Clients on INSERT INTO[CollectionSystem-Imports].dbo.Clients
                         (Id, UpdateDate, CreateDate, Name, Active)
SELECT        NewId, UpdateDate, CreateDate, Name, Active
FROM @PreTbl where id > 0
set identity_insert [CollectionSystem-Imports].dbo.Clients off COMMIT TRANSACTION tr1");

            DatabaseReader.ExecuteCommand(sb_tmp.ToString());
            //Logger.Write("Insert rows to temporary table: " + sb_tmp.ToString());
        }

        bool SyncInsuranceConpanies()
        {
            SqlDataReader ics = null;
            DatabaseReader.ReadInsuranceCompanies(ref ics);
            if(ics == null)
            {
                Logger.Write("Nothing to Sync on Insurance companies table");
                Console.WriteLine("Nothing to Sync on Insurance companies table");
                return true;
            }
            bool list = false;
            List<SyncEntities.SyncIC> listIC = new List<SyncEntities.SyncIC>();
            string req = Request.CreateInsuranceCompaniesRequest(ref ics, ref list, ref listIC);
            if (String.IsNullOrEmpty(req))
            {
                Logger.Write("Sync Insurance companies table - already done");
                Console.WriteLine("Sync Insurance companies table - already done");
                ics.Close();
                return true;
            }

            string response = null;
            List<string> newIds = null;

            if (list)
            {
                response = HttpHandler.PostJson(srvAPIURL+"/InsuranceCompany/SaveList", req);
                newIds = Utils.JsonUtil.GetListOfReturnedIds(response);
                //Logger.Write("Sync Insurance Companies table - " + response);
            }
            else
            {
                response = HttpHandler.PostJson(srvAPIURL+"/InsuranceCompany/Save", req);
                newIds = new List<string>();
                newIds.Add(Utils.JsonUtil.GetFirstModelId(response));
                //Logger.Write("Sync Insurance Companies table - " + response);
            }
            ics.Close();
            
            if (newIds.Count == listIC.Count)
            {
                for (int j = 0; j < newIds.Count; ++j)
                {
                    listIC[j].NewId = newIds[j];
                }
            }
            else
            {
                Logger.Write("Sync Insurance Companies table - http error: " + response);
                Console.WriteLine("Sync Insurance Companies table - http error: " + response);
                return false;
            }

            StringBuilder sb_tmp = new StringBuilder(
              @"DECLARE @PreTbl TABLE ([Id] BIGINT, 
                                         [NewId] BIGINT, 
                                         [InsuranceId] nvarchar(100)
                                        ,[Name] nvarchar(100)
                                        ,[UpdateDate] datetime
                                        ,[CreateDate] datetime
                                        ,[ClientId] bigint
                                        ,[DisplayName] nvarchar(100));");

            string r = @"INSERT INTO @PreTbl
                     ([Id]
                     ,[NewId]
                     ,[InsuranceId]
                     ,[Name]
                     ,[UpdateDate]
                     ,[CreateDate]
                     ,[ClientId]
                     ,[DisplayName])
                      VALUES ";

        sb_tmp.Append(r);


            int k = 0;
            foreach (var o in listIC)
            {
                if (k > 0)
                {
                    sb_tmp.Append(",");
                }

                if (k > 999)
                {
                    sb_tmp.Remove(sb_tmp.Length - 1, 1);
                    sb_tmp.Append(";");
                    sb_tmp.Append(r);
                    k = 0;
                }

                string first = o.Name;
                if (first.Contains("'"))
                {
                    first = o.Name.Replace("'", "''");
                }

                string display = o.DisplayName;
                if (display.Contains("'"))
                {
                    display = o.DisplayName.Replace("'", "''");
                }


                sb_tmp.AppendFormat("({0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}') ",
               o.ID, o.NewId, o.InsuranceId, first, o.UpdateDate, o.CreateDate, o.ClientId, display);

                ++k;
            }



            /*ALTER TABLE[CollectionSystem-Imports].[dbo].[InsuranceCompanies] NOCHECK CONSTRAINT ALL

                ALTER TABLE[CollectionSystem-Imports].[dbo].[InsuranceCompanySettings] NOCHECK CONSTRAINT ALL
             ALTER TABLE[CollectionSystem-Imports].[dbo].[ClientStatusInsuranceSettings] NOCHECK CONSTRAINT ALL */

            sb_tmp.Append("\r\n");
        sb_tmp.Append("BEGIN TRANSACTION tr1 ALTER TABLE [CollectionSystem-Imports].[dbo].[Charges] NOCHECK CONSTRAINT ALL ");
        sb_tmp.Append("UPDATE [CollectionSystem-Imports].[dbo].[Charges] SET [CollectionSystem-Imports].[dbo].[Charges].InsuranceCompanyId = pTbl.NewId FROM [CollectionSystem-Imports].[dbo].[Charges] INNER JOIN @PreTbl AS pTbl On [CollectionSystem-Imports].[dbo].[Charges].InsuranceCompanyId = pTbl.Id WHERE pTbl.Id > 0; ");

        sb_tmp.Append(@" ALTER TABLE [CollectionSystem-Imports].[dbo].[InsuranceCompanySettings] NOCHECK CONSTRAINT ALL
             ALTER TABLE[CollectionSystem-Imports].[dbo].[ClientStatusInsuranceSettings] NOCHECK CONSTRAINT ALL DELETE FROM [CollectionSystem-Imports].[dbo].[InsuranceCompanies] WHERE Id in (select Id from @PreTbl);");

        sb_tmp.Append(@"set identity_insert [CollectionSystem-Imports].dbo.InsuranceCompanies on INSERT INTO [CollectionSystem-Imports].dbo.InsuranceCompanies
                     (Id, InsuranceId, Name, UpdateDate, CreateDate, ClientId, DisplayName)
SELECT        NewId, InsuranceId, Name, UpdateDate, CreateDate, ClientId, DisplayName
FROM @PreTbl where id > 0
set identity_insert [CollectionSystem-Imports].dbo.InsuranceCompanies off COMMIT TRANSACTION tr1");


        return MultipleExecutor.ExecuteTimes(5, sb_tmp.ToString(), "Insurance Companies");

    }

    bool SyncPractices()
    {
        SqlDataReader prcts = null;
        DatabaseReader.ReadPractices(ref prcts);
        if (prcts == null)
        {
            Logger.Write("Nothing to Sync on Practices table");
                Console.WriteLine("Nothing to Sync on Practices table");
                return true;
        }
        bool list = false;
        List<SyncEntities.SyncPractice> listPrc = new List<SyncEntities.SyncPractice>();
        string req = Request.CreatePracticesRequest(ref prcts, ref list, ref listPrc);
        if (String.IsNullOrEmpty(req))
        {
            Logger.Write("Sync Practices table - already done");
                Console.WriteLine("Sync Practices table - already done");
                prcts.Close();
            return true;
        }

        List<string> newIds = null;
        if (list)
        {
            string response= HttpHandler.PostJson(srvAPIURL+"/Practice/SaveList", req);
                newIds = Utils.JsonUtil.GetListOfReturnedIds(response);
               // Logger.Write("Sync Practices table - " + response);
        }
        else
        {
            string response = HttpHandler.PostJson(srvAPIURL+"/Practice/Save", req);
                newIds = new List<string>();
                newIds.Add(Utils.JsonUtil.GetFirstModelId(response));
                //Logger.Write("Sync Practices table - " + response);
        }
        prcts.Close();

            if (newIds.Count == listPrc.Count)
            {
                for (int j = 0; j < newIds.Count; ++j)
                {
                    listPrc[j].NewId = newIds[j];
                }
            }

            StringBuilder sb_tmp = new StringBuilder(
  @"DECLARE @PreTbl TABLE ([Id] BIGINT, 
                                         [NewId] BIGINT
                                        ,[Name] nvarchar(100)
                                        ,[UpdateDate] datetime
                                        ,[CreateDate] datetime);");

            string r = @"INSERT INTO @PreTbl
                     ([Id]
                     ,[NewId]
                     ,[Name]
                     ,[UpdateDate]
                     ,[CreateDate])
                      VALUES ";

            sb_tmp.Append(r);


            int k = 0;
            foreach (var o in listPrc)
            {
                if (k > 0)
                {
                    sb_tmp.Append(",");
                }

                if (k > 999)
                {
                    sb_tmp.Remove(sb_tmp.Length - 1, 1);
                    sb_tmp.Append(";");
                    sb_tmp.Append(r);
                    k = 0;
                }

                string first = o.Name;
                if (first.Contains("'"))
                {
                    first = o.Name.Replace("'", "''");
                }

                sb_tmp.AppendFormat("({0}, {1}, '{2}', '{3}', '{4}') ",
               o.Id, o.NewId, first, o.UpdateDate, o.CreateDate);

                ++k;
            }

            sb_tmp.Append("\r\n");
            sb_tmp.Append("BEGIN TRANSACTION tr1 ALTER TABLE [CollectionSystem-Imports].[dbo].[Charges] NOCHECK CONSTRAINT ALL ");
            sb_tmp.Append(" UPDATE [CollectionSystem-Imports].[dbo].[Charges] SET [CollectionSystem-Imports].[dbo].[Charges].PracticeId = pTbl.NewId FROM [CollectionSystem-Imports].[dbo].[Charges] INNER JOIN @PreTbl AS pTbl On [CollectionSystem-Imports].[dbo].[Charges].PracticeId = pTbl.Id WHERE pTbl.Id > 0; ");
            sb_tmp.Append(" ALTER TABLE [CollectionSystem-Imports].[dbo].[ClientPractices] NOCHECK CONSTRAINT ALL ");
            sb_tmp.Append(" UPDATE [CollectionSystem-Imports].[dbo].[ClientPractices] SET [CollectionSystem-Imports].[dbo].[ClientPractices].PracticeId = pTbl.NewId FROM [CollectionSystem-Imports].[dbo].[ClientPractices] INNER JOIN @PreTbl AS pTbl On [CollectionSystem-Imports].[dbo].[ClientPractices].PracticeId = pTbl.Id WHERE pTbl.Id > 0; ");

            sb_tmp.Append(" ALTER TABLE [CollectionSystem-Imports].[dbo].[Practices] NOCHECK CONSTRAINT ALL DELETE FROM [CollectionSystem-Imports].[dbo].[Practices] WHERE Id in (select Id from @PreTbl);");

            sb_tmp.Append(@" set identity_insert [CollectionSystem-Imports].dbo.Practices on INSERT INTO[CollectionSystem-Imports].dbo.Practices
                     (Id, Name, UpdateDate, CreateDate)
SELECT        NewId, Name, UpdateDate, CreateDate
FROM @PreTbl where id > 0
set identity_insert [CollectionSystem-Imports].dbo.Practices off COMMIT TRANSACTION tr1");


            return MultipleExecutor.ExecuteTimes(5, sb_tmp.ToString(), "Practices");
        }

        /*   void SyncClientPractices()
           {
               Logger.Write("Sync ClientPractices table");

               SqlDataReader clprcts = null;
               DatabaseReader.ReadClientPractices(ref clprcts);

               bool list = false;
               string req = Request.CreateClientPracticesRequest(ref clprcts, ref list);

               if (String.IsNullOrEmpty(req))
               {
                   Logger.Write("Sync Practices table - Error - empty request");
                   clprcts.Close();
                   return;
               }

               if (list)
               {
                   string response = HttpHandler.PostJson(srvAPIURL+"/ClientPractice/SaveList", req);
                   Logger.Write("Sync Practices table - " + response);
               }
               else
               {
                   string response = HttpHandler.PostJson(srvAPIURL+"/ClientPractice/Save", req);
                   Logger.Write("Sync Practices table - " + response);
               }
               clprcts.Close();

               // send the http request

           }*/

        void SyncClientPracticesOld()
        {
            Logger.Write("Sync ClientPractices table - old version - need to implement using API");

            DatabaseReader.ExecuteCommand(@"INSERT INTO ClientPractices
                         (ClientId, PracticeId, UpdateDate, CreateDate)
                         SELECT ClientId, PracticeId, UpdateDate, CreateDate
                         FROM[CollectionSystem - Imports].dbo.ClientPractices 
                     where practiceID not in (select practiceID from[CollectionSystem].dbo.ClientPractices)");

        }


        bool SyncDoctors()
        {

            
            SqlDataReader doctors = null;
            DatabaseReader.ReadDoctors(ref doctors);
            if (doctors == null)
            {
                Logger.Write("Nothing to Sync on Doctors table");
                Console.WriteLine("Nothing to Sync on Doctors table");
                return true;
            }
            bool list = false;

            List<SyncEntities.SyncDoc> listDoc = new List<SyncEntities.SyncDoc>();

            //DataTable List =doctors.ToDataTable();


            string req = Request.CreateDoctorsRequest(ref doctors, ref list, ref listDoc);
            //--------
            

            //req = "";
            if (String.IsNullOrEmpty(req))
            {
                Logger.Write("Sync Doctors table - synchronized already");
                Console.WriteLine("Sync Doctors table - synchronized already");
                doctors.Close();
                return true;
            }

            List<string> newIds = null;
            if (list)
            {
                string response = HttpHandler.PostJson(srvAPIURL+"/Physician/SaveList", req);
                newIds = Utils.JsonUtil.GetListOfReturnedIds(response);
                //Logger.Write("Sync Doctors table - " + response);
            }
            else
            {
                string response = HttpHandler.PostJson(srvAPIURL+"/Physician/Save", req);
                newIds = new List<string>();
                newIds.Add(Utils.JsonUtil.GetFirstModelId(response));
               // Logger.Write("Sync Doctors table - " + response);
            }
            doctors.Close();

            if (newIds.Count == listDoc.Count)
            {
                for (int j = 0; j < newIds.Count; ++j)
                {
                    listDoc[j].NewId = newIds[j];
                }
            }
            if (newIds.Count>0){

                StringBuilder sb_tmp = new StringBuilder(
    @"DECLARE @PreTbl TABLE ([Id] BIGINT, 
                                         [NewId] BIGINT
                                        ,[FirstName] nvarchar(50)
                                        ,[LastName] nvarchar(50)
                                        ,[ProviderId] nvarchar(100)
                                        ,[UpdateDate] datetime
                                        ,[CreateDate] datetime
                                        ,[MiddleName] nvarchar(50));");

                string r = @"INSERT INTO @PreTbl
                     ([Id]
                     ,[NewId]
                     ,[FirstName]
                     ,[LastName]
                     ,[ProviderId]
                     ,[UpdateDate]
                     ,[CreateDate]
                     ,[MiddleName])
                      VALUES ";

                sb_tmp.Append(r);


                int k = 0;
                foreach (var o in listDoc)
                {
                    if (k > 0)
                    {
                        sb_tmp.Append(",");
                    }

                    if (k > 999)
                    {
                        sb_tmp.Remove(sb_tmp.Length - 1, 1);
                        sb_tmp.Append(";");
                        sb_tmp.Append(r);
                        k = 0;
                    }

                    string first = o.FirstName;
                    if (first.Contains("'"))
                    {
                        first = o.FirstName.Replace("'", "''");
                    }

                    string last = o.LastName;
                    if (last.Contains("'"))
                    {
                        last = o.LastName.Replace("'", "''");
                    }

                    string middle = o.MiddleName;
                    if (middle.Contains("'"))
                    {
                        middle = o.MiddleName.Replace("'", "''");
                    }

                    sb_tmp.AppendFormat("({0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}') ",
                   o.Id, o.NewId, first, last, o.ProviderId, o.UpdateDate, o.CreateDate, middle);

                    ++k;
                }

                sb_tmp.Append("\r\n");
                sb_tmp.Append("BEGIN TRANSACTION tr1 ALTER TABLE [CollectionSystem-Imports].[dbo].[Charges] NOCHECK CONSTRAINT ALL ");
                sb_tmp.Append("UPDATE [CollectionSystem-Imports].[dbo].[Charges] SET [CollectionSystem-Imports].[dbo].[Charges].DoctorId = pTbl.NewId FROM [CollectionSystem-Imports].[dbo].[Charges] INNER JOIN @PreTbl AS pTbl On [CollectionSystem-Imports].[dbo].[Charges].DoctorId = pTbl.Id WHERE pTbl.Id > 0; ");

                sb_tmp.Append("ALTER TABLE[CollectionSystem-Imports].[dbo].[Doctors] NOCHECK CONSTRAINT ALL DELETE FROM [CollectionSystem-Imports].[dbo].[Doctors] WHERE Id in (select Id from @PreTbl);");

                sb_tmp.Append(@"set identity_insert [CollectionSystem-Imports].dbo.Doctors on INSERT INTO [CollectionSystem-Imports].dbo.Doctors
                         (Id, FirstName, LastName, ProviderId, UpdateDate, CreateDate, MiddleName)
SELECT        NewId, FirstName, LastName, ProviderId, UpdateDate, CreateDate, MiddleName
FROM @PreTbl where id > 0
set identity_insert [CollectionSystem-Imports].dbo.Doctors off COMMIT TRANSACTION tr1");

                return MultipleExecutor.ExecuteTimes(5, sb_tmp.ToString(), "Doctors");
            }
            return true;
        }

        void SyncTransactionTypes()
        {
            SqlDataReader ttypes = null;
            DatabaseReader.ReadTransactionTypes(ref ttypes);
            if (ttypes == null)
            {
                Logger.Write("Nothing to Sync on TransactionTypes table");
                return;
            }
            bool list = false;
            Dictionary<string, string> idsToUpdate = new Dictionary<string, string>();
            string req = Request.CreateTransactionTypesRequest(ref ttypes, ref list, ref idsToUpdate);
            if (String.IsNullOrEmpty(req))
            {
                Logger.Write("Sync TransactionTypes table - Error - empty request");
                ttypes.Close();
                return;
            }
            if (list)
            {
                string response = HttpHandler.PostJson(srvAPIURL+"/TransactionType/SaveList", req);
                Logger.Write("Sync TransactionTypes table - " + response);
            }
            else
            {
                string response = HttpHandler.PostJson(srvAPIURL+"/TransactionType/Save", req);
                Logger.Write("Sync TransactionTypes table - " + response);
            }
            ttypes.Close(); 
        }

        bool SyncCPT()
        {
            List<string> exIds = new List<string>();

            SqlDataReader cpt = null;
            DatabaseReader.ReadCPT(ref cpt);
            if (cpt == null)
            {
                Logger.Write("Nothing to Sync on CPT table");
                Console.WriteLine("Nothing to Sync on CPT table");
                return true;
            }
            bool list = false;
            List<SyncEntities.SyncCPT> listCPT = new List<SyncEntities.SyncCPT>();

            string req = Request.CreateCPTRequest(ref cpt, ref list, ref listCPT);
            if (String.IsNullOrEmpty(req))
            {
                Logger.Write("Sync CPT table - already synchronized");
                Console.WriteLine("Sync CPT table - already synchronized");
                cpt.Close();
                return true;
            }
            List<string> newIds = null;
            if (list)
            {
                string response = HttpHandler.PostJson(srvAPIURL+"/CPT/SaveList", req);
                if (!response.Contains("error"))
                {
                    newIds = Utils.JsonUtil.GetListOfReturnedIds(response);
                    //Logger.Write("Sync CPT table - " + response);
                }
            }
            else
            {
                string response = HttpHandler.PostJson(srvAPIURL+"/CPT/Save", req);
                newIds = new List<string>();
                newIds.Add(Utils.JsonUtil.GetFirstModelId(response));
                //Logger.Write("Sync CPT table - " + response);
            }
            cpt.Close();

            if (newIds.Count == listCPT.Count)
            {
                for (int j = 0; j < newIds.Count; ++j)
                {
                    listCPT[j].NewId = newIds[j];
                }
            }

            StringBuilder sb_tmp = new StringBuilder(
@"DECLARE @PreTbl TABLE ([Id] BIGINT, [NewId] BIGINT, [ClassId] BIGINT, [Name] nvarchar(100), [Description] nvarchar(200),
                                        [UpdateDate] datetime, [CreateDate] datetime); ");

            string r = @"INSERT INTO @PreTbl ([Id],[NewId],[ClassId],[Name],[Description],[UpdateDate],[CreateDate]) VALUES ";

            sb_tmp.Append(r);

            int k = 0;
            foreach (var o in listCPT)
            {
                if (k > 0)
                {
                    sb_tmp.Append(",");
                }

                if (k > 999)
                {
                    sb_tmp.Remove(sb_tmp.Length - 1, 1);
                    sb_tmp.Append(";");
                    sb_tmp.Append(r);
                    k = 0;
                }

                string name = o.Name;
                if (name.Contains("'"))
                {
                    name = o.Name.Replace("'", "''");
                }

                string desc = o.Description;
                if (desc.Contains("'"))
                {
                    desc = o.Description.Replace("'", "''");
                }

                sb_tmp.AppendFormat("({0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}') ",
               o.Id, o.NewId, o.ClassId, name, desc, o.UpdateDate, o.CreateDate);

                ++k;
            }
            

            sb_tmp.Append("\r\n");
            sb_tmp.Append("BEGIN TRANSACTION tr1 ALTER TABLE [CollectionSystem-Imports].[dbo].[Charges] NOCHECK CONSTRAINT ALL ");
            sb_tmp.Append("UPDATE [CollectionSystem-Imports].[dbo].[Charges] SET [CollectionSystem-Imports].[dbo].[Charges].CPTId = pTbl.NewId FROM [CollectionSystem-Imports].[dbo].[Charges] INNER JOIN @PreTbl AS pTbl On [CollectionSystem-Imports].[dbo].[Charges].CPTId = pTbl.Id WHERE pTbl.Id > 0; ");

            sb_tmp.Append("ALTER TABLE [CollectionSystem-Imports].[dbo].[CPTs] NOCHECK CONSTRAINT ALL DELETE FROM [CollectionSystem-Imports].[dbo].[CPTs] WHERE Id in (select Id from @PreTbl);");

            sb_tmp.Append(@"set identity_insert [CollectionSystem-Imports].dbo.CPTs on INSERT INTO[CollectionSystem-Imports].dbo.CPTs
                         ([Id], [ClassId], [Name], [Description], [UpdateDate], [CreateDate])
SELECT [NewId], [ClassId], [Name], [Description], [UpdateDate], [CreateDate]
FROM @PreTbl where id > 0
set identity_insert [CollectionSystem-Imports].dbo.CPTs off COMMIT TRANSACTION tr1");

            return MultipleExecutor.ExecuteTimes(5, sb_tmp.ToString(), "CPT");
        }

        void SyncChargesTransactionsSplitted()
        {
            SqlDataReader reader = null;
            DatabaseReader.GetChargesWithNullLiveId(ref reader);
            List<string> charges2Update = new List<string>();
            Dictionary<string, List<SyncEntities.SyncCharge>> requests_ch = Request.CreateChargeRequestList(ref reader, ref charges2Update);

            Console.WriteLine("Charges and Transactions table sync started...");
 
            if(reader!=null)reader.Close();
            if (requests_ch != null)
            {
                foreach (var charges in requests_ch)
                {
                    ChargeSyncronizer.AddRecords(charges);                                        
                }
            }

            else
            {
                Logger.Write("Sync Charges table - already synchronized");
                Console.WriteLine("Sync Charges table - already synchronized");
                return;
            }          
        }

        void UpdateChargesTransactions()
        {          
            SqlDataReader reader = null;

            //get charges to update to list of Charge Entities
            DatabaseReader.GetChargesToUpdate(ref reader);
            List<string> charges2Update = new List<string>();

            Dictionary<string, List<SyncEntities.SyncCharge>> requests_ch = Request.CreateChargeRequestList(ref reader, ref charges2Update);

            if (reader!=null) reader.Close();

            if (requests_ch != null)
            {
                foreach (var charges in requests_ch)
                {

                    ChargeSyncronizer.UpdateCharges(charges2Update);
                    ChargeSyncronizer.KillTransactions(charges);
                    ChargeSyncronizer.AddTransactions(charges);
                }
            }                
        }

        void DeleteChargesTransactions()
        {
            SqlDataReader reader = null;

            DatabaseReader.GetChargesToDelete(ref reader);
            List<string> charges2Update = new List<string>();
            Dictionary<string, List<SyncEntities.SyncCharge>> requests_ch = Request.CreateChargeRequestList(ref reader, ref charges2Update );
            if (reader!=null) reader.Close();
            
            if (requests_ch != null)
            {
                foreach (var charges in requests_ch)
                {
                    ChargeSyncronizer.DeleteRecords(charges);
                }
                
            }
        }
    }
}
