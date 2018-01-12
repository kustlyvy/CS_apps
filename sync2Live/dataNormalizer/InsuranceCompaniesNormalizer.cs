using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

using System.Configuration;


namespace dataNormalizer
{
    class InsuranceCompaniesNormalizer
    {
        Dictionary<string, string> unique_names = new Dictionary<string, string>();
        Dictionary<string, string> to_change = new Dictionary<string, string>();

        private bool ExecuteTimes(int times, string cmd, string table)
        {
            int execTimes = 5;
            bool result = true;
            while (execTimes > 0)
            {
                Logger.Write("trying to Normalize local " + table + " table");
                int res = DatabaseReader.ExecuteCommand(cmd);
                if (res == -1)
                {
                    int num = (5 - execTimes) + 1;
                    Logger.Write("Normalizing local " + table + " table failed: on try " + num.ToString());
                    if (execTimes == 1)
                    {
                        Logger.Write(cmd);
                        result = false;
                    }
                }
                else {
                    Logger.Write("local " + table + " ids update success.");
                    result = true;
                }
                --execTimes;
                System.Threading.Thread.Sleep(1000);
                break;
            }
            return result;
        }

        void UpdateInsuranceData(KeyValuePair<string,string> record)
        {
            StringBuilder sb_tmp = new StringBuilder();
            sb_tmp.AppendFormat(
                "UPDATE [dbo].[Charges] SET [dbo].[Charges].InsuranceCompanyId = {0} FROM [dbo].[Charges] INNER JOIN [dbo].[InsuranceCompanies] On [dbo].[Charges].InsuranceCompanyId = [dbo].[InsuranceCompanies].Id WHERE [dbo].[InsuranceCompanies].Name = '{1}';", 
                record.Value, record.Key);

            ExecuteTimes(5, sb_tmp.ToString(), "Insurance Companies");
        }

        void RemoveDupInsuranceCompanies(KeyValuePair<string, string> record)
        {
            StringBuilder sb_tmp = new StringBuilder();
            sb_tmp.AppendFormat(
                "DELETE FROM [dbo].[InsuranceCompanies] WHERE [dbo].[InsuranceCompanies].Name = '{0}' AND [dbo].[InsuranceCompanies].Id <> {1};",
                record.Key, record.Value);

            ExecuteTimes(5, sb_tmp.ToString(), "Insurance Companies");
        }

        public bool Normalize()
        {
            SqlDataReader reader = null;

            string server = ConfigurationManager.AppSettings["server"];
            string database = ConfigurationManager.AppSettings["database"];
            string login = ConfigurationManager.AppSettings["login"];
            string password = ConfigurationManager.AppSettings["password"];
            string localDBName = ConfigurationManager.AppSettings["localDbName"];

            DatabaseReader.OpenConnection(new Parameters(
                        server,
                        database,
                        login,
                        password
                    ));

            // query the ins comps in db
            DatabaseReader.ReadInsuranceCompanies(ref reader);

            if (reader == null)
                return true;

            while (reader.Read())
            {
                string name = reader["Name"].ToString().Trim();
                string id = reader["Id"].ToString().Trim();
                try {
                    // put the names and ids to dictionary to find duplicates
                    unique_names.Add(name, id);
                }
                catch(ArgumentException e)
                {
                    try
                    {
                        // put ids to for deletion list
                        to_change.Add(name, id);
                    }
                    catch(Exception ex)
                    {

                    }

                }

            }

            reader.Close();


            foreach (var record in to_change)
            {
                UpdateInsuranceData(record);
                RemoveDupInsuranceCompanies(record);
            }

            DatabaseReader.CloseConnection();
            // query duplicated names one by one and assign lesser id
            // remove duplicates from insurance table

            return true;
        }
        

        
    }
}
