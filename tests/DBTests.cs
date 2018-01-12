using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.Collections.Generic;
using sync2Live;

namespace tests
{
    [TestClass]
    public class DbTests
    {
        [TestMethod]
        public void TestGetTransactions()
        {
            string server = "176.9.113.46";
            string database = "CollectionSystem-Imports";
            string login = "cs_user";
            string password = "Collect12";

            DatabaseReader.OpenConnection(new Parameters(
                        server,
                        database,
                        login,
                        password
                    ));

            SqlDataReader reader = null;
            List<string> testList = new List<string>();
            testList.Add("399527");
            testList.Add("399528");
            testList.Add("399529");
            testList.Add("399530");
            testList.Add("399531");
            DatabaseReader.GetTransactions(ref reader, ref testList);

            List<sync2Live.SyncEntities.SyncTransaction> listTr = new List<sync2Live.SyncEntities.SyncTransaction>();

            string tr_req = Request.CreateTransactionRequest(ref reader, ref listTr);
            reader.Close();

            DatabaseReader.CloseConnection();

            Assert.IsTrue(listTr.Count == 9);
        }

    }
}
