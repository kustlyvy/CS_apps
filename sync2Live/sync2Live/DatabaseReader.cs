﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace sync2Live
{
    public class DatabaseReader
    {
        static SqlConnection connection = null;

        public static void OpenConnection(Parameters prm)
        {
            string conn = String.Format(
                @"user id={0};password={1};server={2};Trusted_Connection=no;database={3};connection timeout=30000",
                prm.login, 
                prm.password, 
                prm.server, 
                prm.database
            );
            connection = new SqlConnection(conn.Trim());

            try {
                connection.Open();
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Logger.Write("DatabaseReader - " + e.ToString());
            }
        }

        public static void CloseConnection()
        {
            try {
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Logger.Write("DatabaseReader - " + e.ToString());
            }
        }

        public static void ReadDataByCommand(string query, ref SqlDataReader reader)
        {            
            try
            {
                SqlCommand cmd = new SqlCommand(query, connection);
            
                reader = cmd.ExecuteReader();                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Logger.Write("DatabaseReader - " + e.ToString());
            }
        }

        public static DataTable ReadSQLDataByCommand(string query)
        {

            DataTable dt = new DataTable();
            try
            {
                SqlCommand cmd = new SqlCommand(query, connection);


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dt);
                da.Dispose();
                return dt;
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Logger.Write("DatabaseReader - " + e.ToString());
                return null;
            }
        }

        public static int ExecuteCommand(string query)
        {
            int res = -1;
            try
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                res = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Logger.Write("DatabaseReader - " + e.ToString());
            }
            return res;
        }

        public static void ReadPatients(ref SqlDataReader reader)
        {
            // simplified version should be replaced with version below, when ClientPatient table will work
            string cmd = @"SELECT Id,
                                       PolicyNumber,
                                       AccountId,
                                       FirstName,
                                       MiddleName,
                                       LastName,
                                       DOB,
                                       UpdateDate,
                                       CreateDate,
                                       SSN,
                                       Policy,
                                       Access
                          FROM [CollectionSystem-Imports].dbo.Patients
                          where id > 0";
            ReadDataByCommand(cmd, ref reader);

            // TODO: replace the above call with this one when ClientPatient table will work
            /*ReadDataByCommand(@"SELECT [CollectionSystem_Test].dbo.ClientPatients.ClientId,
                                       [CollectionSystem-Imports].dbo.Patients.Id, 
                                       [CollectionSystem-Imports].dbo.Patients.PolicyNumber, 
                                       [CollectionSystem-Imports].dbo.Patients.AccountId, 
                                       [CollectionSystem-Imports].dbo.Patients.FirstName, 
                                       [CollectionSystem-Imports].dbo.Patients.MiddleName, 
                                       [CollectionSystem-Imports].dbo.Patients.LastName, 
                                       [CollectionSystem-Imports].dbo.Patients.DOB, 
                                       [CollectionSystem-Imports].dbo.Patients.UpdateDate, 
                                       [CollectionSystem-Imports].dbo.Patients.CreateDate, 
                                       [CollectionSystem-Imports].dbo.Patients.SSN, 
                                       [CollectionSystem-Imports].dbo.Patients.Policy, 
                                       [CollectionSystem-Imports].dbo.Patients.Access
                          FROM[CollectionSystem-Imports].dbo.Patients INNER JOIN [CollectionSystem_Test].dbo.ClientPatients on [CollectionSystem-Imports].dbo.Patients.Id = [CollectionSystem_Test].dbo.ClientPatients.PatientId",
                 ref reader
    );*/
        }

        public static void ReadClients(ref SqlDataReader reader)
        {
            ReadDataByCommand(@"SELECT Id, 
                                       UpdateDate, 
                                       CreateDate, 
                                       Name, 
                                       [Active]
                             FROM[CollectionSystem-Imports].dbo.Clients
                             where id > 0", // not in (select id from[CollectionSystem-PreRelease].dbo.Clients)",
                             ref reader
             );
        }

        public static void ReadInsuranceCompanies(ref SqlDataReader reader)
        {
            string cmd = @"SELECT Id, InsuranceId, Name, UpdateDate, CreateDate, ClientId, DisplayName
                        FROM [CollectionSystem-Imports].dbo.InsuranceCompanies where id > 0";
            ReadDataByCommand(cmd,// not in (select id from[CollectionSystem-PreRelease].dbo.InsuranceCompanies)",
                        ref reader
            );
        }

        public static void ReadPractices(ref SqlDataReader reader)
        {
            ReadDataByCommand(@"SELECT Id, 
                                      Name, 
                                      UpdateDate, 
                                      CreateDate
                              FROM[CollectionSystem-Imports].dbo.Practices
                              where id > 0", //not in (select id from[CollectionSystem-PreRelease].dbo.Practices)",
                              ref reader
            );
        }

       /* public static void ReadClientPractices(ref SqlDataReader reader)
        {
            ReadDataByCommand(@"SELECT ClientId, 
                                       PracticeId, 
                                       UpdateDate, 
                                       CreateDate
                             FROM [CollectionSystem-Imports].dbo.ClientPractices 
                             where practiceID not in (select practiceID from [CollectionSystem-PreRelease].dbo.ClientPractices)",
                             ref reader
            );
        }*/

        public static void ReadDoctors(ref SqlDataReader reader)
        {
            ReadDataByCommand(@"SELECT FirstName, 
                                       Id, 
                                       LastName, 
                                       ProviderId, 
                                       UpdateDate, 
                                       CreateDate, 
                                       MiddleName
                            FROM  [CollectionSystem-Imports].dbo.Doctors 
                            where id > 0", // not in (select id from [CollectionSystem-PreRelease].dbo.Physicians)",
                            ref reader
            );
        }

        public static void ReadTransactionTypes(ref SqlDataReader reader)
        {
            ReadDataByCommand(@"SELECT Id, Name, 
                                      SourceFileId, 
                                      UpdateDate, 
                                      CreateDate
                            FROM [CollectionSystem-Imports].dbo.TransactionTypes
                            where id > 0", //not in (select id from [CollectionSystem-PreRelease].dbo.TransactionTypes)",
                            ref reader
            );
        }

        public static void ReadCPT(ref SqlDataReader reader)
        {
            ReadDataByCommand(@"SELECT Id, 
                                       ClassId, 
                                       Name, 
                                       Description, 
                                       UpdateDate, 
                                       CreateDate
                           FROM [CollectionSystem-Imports].dbo.CPTs 
                           where id > 0", //not in (select id from [CollectionSystem-PreRelease].dbo.CPTs)",
                           ref reader
            );
        }


        public static void GetTransactionsLive(ref SqlDataReader reader, ref List<string> newIds, ref List<string> oldIds)
        {
            StringBuilder listIds = new StringBuilder();
            int k = 0;
            foreach (string id in newIds)
            {
                if (k > 0)
                {
                    listIds.Append(',');
                }
                listIds.Append(id.ToString().Trim());
                ++k;
            }

            string cmd = @"SELECT transactions.Id, charges.LiveId as ChargeID, transactions.TransactionId, transactions.DateOfService, transactions.DateOfPosting, transactions.Billed, transactions.Payment, transactions.Adjustment, transactions.Balance FROM 
                            [CollectionSystem-Imports].dbo.Transactions 
                            inner join [CollectionSystem-Imports].dbo.charges on charges.id=transactions.chargeid 
                            where transactions.ChargeId in  
                           (" + listIds.ToString() + ")";

            ReadDataByCommand(cmd, ref reader);


        }


        public static void GetTransactions(ref SqlDataReader reader, ref List<string> newIds)
        {
            StringBuilder listIds = new StringBuilder();
            int k = 0;
            foreach (string id in newIds)
            {
                if(k > 0)
                {
                    listIds.Append(',');
                }
                listIds.Append(id.ToString().Trim());
                ++k;               
            }

            string cmd = @"SELECT Id, 
                                       ChargeId, 
                                       TransactionId, 
                                       DateOfService, 
                                       DateOfPosting, 
                                       Billed,
                                       Payment, 
                                       Adjustment,
                                       Balance
                                       FROM [CollectionSystem-Imports].dbo.Transactions 
                           where ChargeId in (" + listIds.ToString() + ")";

            ReadDataByCommand(cmd, ref reader);



        }

        public static void GetChargesToUpdate(ref SqlDataReader reader)
        {
            var cmd = @"SELECT Id,DateOfService,DateOfPosting,PracticeId,PatientId,CPTId,InsuranceCompanyId,Billed,Payment,Adjustment,Balance,UpdateDate,CreateDate, DoctorId,CaseNumber,                                       
                                       StatusId,DueDate, LiveID,updated,toDelete FROM [CollectionSystem-Imports].dbo.Charges where updated = 1";
            ReadDataByCommand(cmd,
                    ref reader
           );
        }

        public static void GetChargesToDelete(ref SqlDataReader reader)
        {
            ReadDataByCommand(@"SELECT Id,      
                                       DateOfService,
                                       DateOfPosting, 
                                       PracticeId,
                                       PatientId,
                                       CPTId,
                                       InsuranceCompanyId,
                                       Billed,
                                       Payment,
                                       Adjustment,
                                       Balance,
                                       UpdateDate, 
                                       CreateDate, 
                                       DoctorId,
                                       CaseNumber,                                       
                                       StatusId,
                                       DueDate, 
                                       LiveID, 
                                       updated,
                                       toDelete                                         
                          FROM[CollectionSystem-Imports].dbo.Charges where toDelete = 1",
                    ref reader
           );
        }

        /*where chargeID in (select liveID from @preTbl)
                           where [dbo].[Charges].LiveId > 0 AND [dbo].[Charges].updated = 1 INNER JOIN [dbo].[Charges] ON [dbo][Transactions].ChargeId = [dbo].[Charges].Id;",
      */

        public static void GetChargesWithNullLiveId(ref SqlDataReader reader)
        {
            // simplified version should be replaced with version below, when ClientPatient table will work

            var commandText = @"SELECT Id,DateOfService,DateOfPosting,PracticeId,PatientId,CPTId,InsuranceCompanyId,Billed,Payment,Adjustment,Balance,UpdateDate,CreateDate,DoctorId,CaseNumber,StatusId,DueDate,LiveID,updated,toDelete
                          FROM [CollectionSystem-Imports].dbo.Charges
                          where LiveID is NULL";
                commandText = @"SELECT * FROM [CollectionSystem-Imports].dbo.Charges where LiveID is NULL";
            ReadDataByCommand(commandText,
                          ref reader
             );

        }

        public static void GetAllCharges(ref SqlDataReader reader)
        {
            // simplified version should be replaced with version below, when ClientPatient table will work
            ReadDataByCommand(@"SELECT Id,      
                                       DateOfService,
                                       DateOfPosting, 
                                       PracticeId,
                                       PatientId,
                                       CPTId,
                                       InsuranceCompanyId,
                                       Billed,
                                       Payment,
                                       Adjustment,
                                       Balance,
                                       UpdateDate, 
                                       CreateDate, 
                                       DoctorId,
                                       CaseNumber,                                       
                                       StatusId,
                                       DueDate, 
                                       LiveID, 
                                       updated 
                                        
                          FROM[CollectionSystem-Imports].dbo.Charges
                          where Id > 0",
                          ref reader
             );

        }

    }
}
