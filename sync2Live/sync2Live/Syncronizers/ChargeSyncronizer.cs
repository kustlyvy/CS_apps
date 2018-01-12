using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sync2Live.Utils;
using System.Data.SqlClient;

namespace sync2Live.Syncronizers
{
    class ChargeSyncronizer
    {
        const string srvAPIURL = "http://collectionsystem.grebinnyk.com/exports";

        



        public static bool KillTransactions(KeyValuePair<string, List<SyncEntities.SyncCharge>> charges)
        {
            int killed_success = 0;
            int killed_error = 0;

            //Logger.Write("KillRecords:");
            // Console.WriteLine("KillRecords:");
            try
            {
                foreach (var o in charges.Value)
                {
                    if (o != null)
                    {

                        // kill transactions
                        string json_tr = String.Format("{{ \"Id\": \"{0}\" }}", o.LiveId);
                        string resp_tr = HttpHandler.PostJson(srvAPIURL + "/Transaction/KillByCharge", json_tr);

                        if (resp_tr.Contains(": false"))
                        {
                            //Logger.Write("Kill transactions by charge: error - " + resp_tr.ToString());
                            Console.WriteLine("Kill transactions by charge: error ...");
                        }

                        // Logger.Write("Kill transactions by charge: success");
                        Console.WriteLine("Kill transactions by charge: success ...");
                    }
                }
            }
            catch (Exception e)
            {
                //Logger.Write("Kill charge: error - " + e.ToString());
                Console.WriteLine("Kill charge: error ..." + e.ToString());
                return false;
            }

            Logger.Write("killed: " + killed_success.ToString() + " charges");
            Logger.Write("kill charge errors: " + killed_error.ToString());
            return true;

        }

        public static bool AddTransactions(KeyValuePair<string, List<SyncEntities.SyncCharge>> charges)
        {
            List<string> oldIds = new List<string>();
            List<string> newIds = new List<string>();

            int added = 0;


            try
            {
                foreach (var o in charges.Value)
                {
                    if (o != null)
                    {
                        oldIds.Add(o.Id.ToString());
                        newIds.Add(o.LiveId.ToString());
                    }
                }
            }
            catch (Exception e){
                Console.WriteLine("Kill charge: error ..." + e.ToString());
                return false;

            }
                        SqlDataReader tranReader = null;

            List<SyncEntities.SyncTransaction> listTr = new List<SyncEntities.SyncTransaction>();
            // select all from transactions
            //DatabaseReader.GetTransactions(ref tranReader, ref newIds);
            

            DatabaseReader.GetTransactionsLive(ref tranReader, ref oldIds, ref newIds);
            // DatabaseReader.GetTransactionsWhereChargeNotUpdated(ref tranReader, ref newIds); 
            //oldIds.Clear();
            // create json for all with new live id
            string tr_req = Request.CreateTransactionRequest(ref tranReader, ref listTr);
            if (tranReader != null) tranReader.Close();

            if (String.IsNullOrEmpty(tr_req))
                return false;
            // send to http
            string tr_resp = HttpHandler.PostJson(srvAPIURL + "/Transaction/SaveList", tr_req);
            if (tr_resp.Contains(": false"))
            {
                Logger.Write("send transactions to http: error");
                Logger.Write("  request  - /// " + tr_req + " ///");
                Console.WriteLine("send transactions to http: error ...");
                Console.WriteLine("  request  - /// " + tr_req + " ///");
                return false; 
            }
            else
            {
                //Logger.Write("send transactions to http: success");
                //Console.WriteLine("send transactions to http: success ...");
            }

            StringBuilder sb_tmp = new StringBuilder(
                @"DECLARE @PreTbl TABLE ([Id] BIGINT); ");

            string r = @"INSERT INTO @PreTbl ([Id]) VALUES ";

            sb_tmp.Append(r);

            int k = 0;
            foreach (var o in charges.Value)
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


                sb_tmp.AppendFormat("({0}) ", 
                    o.Id);

                ++k;
            }

            sb_tmp.Append("\r\n");
            //sb_tmp.Append("ALTER TABLE [CollectionSystem-Imports].[dbo].[Charges] NOCHECK CONSTRAINT ALL ");
            sb_tmp.Append("UPDATE [CollectionSystem-Imports].[dbo].[Charges] SET [CollectionSystem-Imports].[dbo].[Charges].updated = 0 FROM [CollectionSystem-Imports].[dbo].[Charges] INNER JOIN @PreTbl AS pTbl On [CollectionSystem-Imports].[dbo].[Charges].Id = pTbl.Id WHERE pTbl.Id > 0; ");

            MultipleExecutor.ExecuteTimes(5, sb_tmp.ToString(), "Charges");

            // end of add charges bunch 

            return true;
        }



        public static bool KillRecords(KeyValuePair<string, List<SyncEntities.SyncCharge>> charges)
        {
            int killed_success = 0;
            int killed_error = 0;

            //Logger.Write("KillRecords:");
           // Console.WriteLine("KillRecords:");
            try
            {
                foreach (var o in charges.Value)
                {
                    if (o != null)
                    {
                        string json_r = String.Format("{{ \"Id\": \"{0}\" }}", o.LiveId);
                        string resp = HttpHandler.PostJson(srvAPIURL + "/Charge/Kill", json_r);
                        if (resp.Contains(": false"))
                        {
                           /* string msg = String.Format(
    "{{ LiveId: {0}, PatientId: {1], CreateDate: {2}, DateOfPosting: {3},  DateOfService: {4}, Payment: {5}, Adjustment: {6}, Billed: {7}, Balance: {8} }}  == kill error\n",
    o.LiveId,
    o.PatientId,
    o.CreateDate,
    o.DateOfPosting,
    o.DateOfService,
    o.Payment,
    o.Adjustment,
    o.Billed,
    o.Balance);

                            Logger.Write(msg);*/

                            ++killed_error;
                        }
                        else {
                           /* string msg = String.Format(
                                "{{ LiveId: {0}, PatientId: {1}, CreateDate: {2}, DateOfPosting: {3},  DateOfService: {4}, Payment: {5}, Adjustment: {6}, Billed: {7}, Balance: {8} }}  == killed successfully\n",
                                o.LiveId,
                                o.PatientId,
                                o.CreateDate,
                                o.DateOfPosting,
                                o.DateOfService,
                                o.Payment,
                                o.Adjustment,
                                o.Billed,
                                o.Balance);

                             Logger.Write(msg);
                            Console.WriteLine(msg);*/
                            ++killed_success;
                        }

                        // kill transactions
                        string json_tr = String.Format("{{ \"Id\": \"{0}\" }}", o.LiveId);
                        string resp_tr = HttpHandler.PostJson(srvAPIURL + "/Transaction/KillByCharge", json_tr);

                        if (resp_tr.Contains(": false"))
                        {
                            //Logger.Write("Kill transactions by charge: error - " + resp_tr.ToString());
                            Console.WriteLine("Kill transactions by charge: error ...");
                        }

                       // Logger.Write("Kill transactions by charge: success");
                        Console.WriteLine("Kill transactions by charge: success ...");


                    }                        
                }
            }
            catch (Exception e)
            {
                //Logger.Write("Kill charge: error - " + e.ToString());
                Console.WriteLine("Kill charge: error ..." + e.ToString());
                return false;
            }

            Logger.Write("killed: " + killed_success.ToString() + " charges");
            Logger.Write("kill charge errors: " + killed_error.ToString());


            //Logger.Write("===========================");
            //Console.WriteLine("===========================");
            return true;
        }


        public static bool UpdateCharges(List<string> charges2Update)
        {



            

            string response = null;
            string par="";
            List<string> req = new List<string>();
            int i = 0;
            while (charges2Update.Count > 0)

            {
                i = 0;
                par = "";
                req.Clear();
                while (i < 100 && charges2Update.Count > 0)
                {
                    req.Add(charges2Update.ElementAt(charges2Update.Count-1));
                    charges2Update.RemoveAt(charges2Update.Count-1);
                    i++;
                }
                par = CreateListParams(req);


                response = HttpHandler.PostJson(srvAPIURL + "/Charge/SaveList", par);
                if (response.Contains(":false"))
                {
                    Logger.Write("Error updating charges - ///////////////   ");
                    Logger.Write(charges2Update.ToString());
                    Logger.Write("   ////////////////");
                    Console.WriteLine("Error sending charges to http... trying next bunch...");
                    
                }
                else
                {

                    Console.WriteLine("Charge update sent to http successfully..");
                    Logger.Write("Charge update sent to http successfully..");
                    
                }

            }



            return true;



        }


        public static string CreateListParams(List<string> prms)
        {


            string result = "";
            result = result.Insert(0, "{ \"List\": [");



            int i = 0;
            foreach (var row in prms)
            {
                if (i > 0)
                {
                    result = result.Insert(result.Length, ",");
                }
                result = result.Insert(result.Length, row);
                ++i;
            }
            return result.Insert(result.Length, "]}");
        }

            public static bool AddRecords(KeyValuePair<string, List<SyncEntities.SyncCharge>> charge)
        {
            List<string> oldIds = new List<string>();
            List<string> newIds = null;

            int added = 0;

            string response = null;

            response = HttpHandler.PostJson(srvAPIURL + "/Charge/SaveList", charge.Key);
            if (response.Contains(":false"))
            {
                Logger.Write("Error adding charges - ///////////////   ");
                Logger.Write(charge.Key);
                Logger.Write("   ////////////////");
                Console.WriteLine("Error sending charges to http... trying next bunch...");
                return false;
            }
            else {

                //Console.WriteLine("sent to http successfully..");
                //Logger.Write("sent to http successfully..");
            }

            newIds = Utils.JsonUtil.GetListOfReturnedIds(response);

            Console.WriteLine(newIds.Count.ToString() + " values added...");
            //Logger.Write(newIds.Count.ToString() + " values added...");
            added += newIds.Count;

            if (newIds.Count == 0)
                return false;

            if (newIds.Count == charge.Value.Count)
            {
                Logger.Write("Add Records");
                for (int j = 0; j < newIds.Count; ++j)
                {
                    oldIds.Add(charge.Value[j].Id.ToString());

                    charge.Value[j].NewId = newIds[j];
                    charge.Value[j].LiveId = newIds[j];
                    /*string msg = String.Format(
    "{{ LiveId: {0}, PatientId: {1}, CreateDate: {2}, DateOfPosting: {3},  DateOfService: {4}, Payment: {5}, Adjustment: {6}, Billed: {7}, Balance: {8} }}  == added successfully\n",
    charge.Value[j].LiveId,
    charge.Value[j].PatientId,
    charge.Value[j].CreateDate,
    charge.Value[j].DateOfPosting,
    charge.Value[j].DateOfService,
    charge.Value[j].Payment,
    charge.Value[j].Adjustment,
    charge.Value[j].Billed,
    charge.Value[j].Balance);

                    Logger.Write(msg);
                    Console.WriteLine(msg);*/
                    
                }
            }

            StringBuilder sb_tmp = new StringBuilder(
@"DECLARE @PreTbl TABLE ([Id] BIGINT, [LiveId] BIGINT); ");

            string r = @"INSERT INTO @PreTbl ([Id],[LiveId]) VALUES ";

            sb_tmp.Append(r);

            int k = 0;
            foreach (var o in charge.Value)
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


                sb_tmp.AppendFormat("({0}, {1}) ",
               o.Id, o.LiveId);

                ++k;
            }

            sb_tmp.Append("\r\n");
            sb_tmp.Append("ALTER TABLE [CollectionSystem-Imports].[dbo].[Charges] NOCHECK CONSTRAINT ALL ");
            sb_tmp.Append("UPDATE [CollectionSystem-Imports].[dbo].[Charges] SET [CollectionSystem-Imports].[dbo].[Charges].LiveId = pTbl.LiveId, [CollectionSystem-Imports].[dbo].[Charges].updated = 0 FROM [CollectionSystem-Imports].[dbo].[Charges] INNER JOIN @PreTbl AS pTbl On [CollectionSystem-Imports].[dbo].[Charges].Id = pTbl.Id WHERE pTbl.Id > 0; ");
            //  Denis's code  (unlinking transactions from charges, assingning LiveID as chargeID - no sence & breaks data )
            //sb_tmp.Append(" ALTER TABLE [CollectionSystem-Imports].[dbo].[Transactions] NOCHECK CONSTRAINT ALL ");
            //sb_tmp.Append("UPDATE [CollectionSystem-Imports].[dbo].[Transactions] SET [CollectionSystem-Imports].[dbo].[Transactions].ChargeId = pTbl.LiveId FROM [CollectionSystem-Imports].[dbo].[Transactions] INNER JOIN @PreTbl AS pTbl On [CollectionSystem-Imports].[dbo].[Transactions].ChargeId = pTbl.Id WHERE pTbl.Id > 0; ");

            MultipleExecutor.ExecuteTimes(5, sb_tmp.ToString(), "Charges");

            // end of add charges bunch 

            SqlDataReader tranReader = null;

            List<SyncEntities.SyncTransaction> listTr = new List<SyncEntities.SyncTransaction>();
            // select all from transactions
            //DatabaseReader.GetTransactions(ref tranReader, ref newIds);
            DatabaseReader.GetTransactionsLive(ref tranReader, ref oldIds, ref newIds );
           // DatabaseReader.GetTransactionsWhereChargeNotUpdated(ref tranReader, ref newIds); 
            //oldIds.Clear();
            // create json for all with new live id
            string tr_req = Request.CreateTransactionRequest(ref tranReader, ref listTr);
            if(tranReader != null) tranReader.Close();

            if (String.IsNullOrEmpty(tr_req))
                return false;
            // send to http
            string tr_resp = HttpHandler.PostJson(srvAPIURL + "/Transaction/SaveList", tr_req);
            if (tr_resp.Contains(": false"))
            {
                Logger.Write("send transactions to http: error");
                Logger.Write("  request  - /// " + tr_req + " ///");
                Console.WriteLine("send transactions to http: error ...");
                Console.WriteLine("  request  - /// " + tr_req + " ///");
            }
            else {
                //Logger.Write("send transactions to http: success");
                //Console.WriteLine("send transactions to http: success ...");
            }

            return true;
        }

        public static bool DeleteRecords(KeyValuePair<string, List<SyncEntities.SyncCharge>> charges)
        {
            if (!KillRecords(charges))
                return false;

            StringBuilder sb_tmp = new StringBuilder(
              @"DECLARE @PreTbl TABLE ([Id] BIGINT);");

            string r = @"INSERT INTO @PreTbl
                     ([Id])
                      VALUES ";

            sb_tmp.Append(r);


            int k = 0;
            foreach (var o in charges.Value)
            {
                if (k > 0)
                {
                    sb_tmp.Append(",");
                }

               //sb_tmp.AppendFormat("({0})", o.LiveId);
                sb_tmp.AppendFormat("({0})", o.Id);
                ++k;
            }

            // Bucket Chargeg not used in DB now sb_tmp.Append(@"DELETE FROM [dbo].[BucketCharges] WHERE ChargeId in (select Id from @PreTbl); ");
            sb_tmp.Append(@"DELETE FROM [dbo].[Transactions] WHERE chargeId in (select Id from @PreTbl);");
            sb_tmp.Append(@"DELETE FROM [dbo].[Charges] WHERE Id in (select Id from @PreTbl);");

            return MultipleExecutor.ExecuteTimes(5, sb_tmp.ToString(), "Charges");
        }


    }
}
