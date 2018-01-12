using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace intgchk
{
    class IntegrityCheck
    {
        string[] _col_names;
        Dictionary<string, List<List<string>>> _records = new Dictionary<string, List<List<string>>>();
        int fail_count = 0;

       /* struct Data_Money
        {
            public double amount;
            public double payment;
            public double adj;
            public double balance;

           public Data_Money(double a, double p, double ad, double b)
            {
                amount = a;
                payment = p;
                adj = ad;
                balance = b;
            }
        }

        Dictionary<string, Data_Money> amount_check = new Dictionary<string, Data_Money>();*/

        DateTime fileCreatedDate;

        public int GetFailCount()
        {
            return fail_count;
        }

        public void parseCsv(string sFile)
        {
            fileCreatedDate = System.IO.File.GetCreationTime(sFile);
             using (TextFieldParser csvParser = new TextFieldParser(sFile))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                _col_names = csvParser.ReadFields();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    string key = fields[2];

                    if (!String.IsNullOrEmpty(key))
                    {
                        if (_records.ContainsKey(key))
                        {
                            List<List<string>> lss;
                            _records.TryGetValue(key, out lss);

                            if (lss != null)
                            {
                                List<string> ls = new List<string>();

                               /* double _amount, _prp, _inp, _adj, _bal;

                                Double.TryParse(fields[63], out _amount);
                                Double.TryParse(fields[64], out _prp);
                                Double.TryParse(fields[65], out _inp);
                                Double.TryParse(fields[66], out _adj);
                                Double.TryParse(fields[67], out _bal);

                                if (_amount == _bal && _prp == 0 && _inp == 0 && _adj == 0)
                                {
                                    continue;
                                }
                                else {*/

                                    ls.Add(fields[0]);  // Last Name

                                    ls.Add(fields[1]);  // First Name
                                    ls.Add(fields[2]);  // Account Id

                                    ls.Add(fields[39]); //Post Date
                                    ls.Add(fields[40]); // Service Date

                                    ls.Add(fields[63]); // Amount
                                    ls.Add(fields[64]); // PRP
                                    ls.Add(fields[65]); // INP
                                    ls.Add(fields[66]); // Adj
                                    ls.Add(fields[67]); // Bal

                                //string k = fields[0] + fields[1];
                                //Data_Money dm;

                            /*    double _amount, _prp, _inp, _adj, _bal;

                                Double.TryParse(fields[63], out _amount);
                                Double.TryParse(fields[64], out _prp);
                                Double.TryParse(fields[65], out _inp);
                                Double.TryParse(fields[66], out _adj);
                                Double.TryParse(fields[67], out _bal);

                                if (amount_check.TryGetValue(k, out dm))
                                {
                                    dm.amount += _amount;
                                    dm.payment += (_prp + _inp);
                                    dm.adj += _adj;
                                    dm.balance += _bal;

                                    amount_check[k] = dm;

                                }
                                else {
                                    amount_check.Add(fields[0] + fields[1], new Data_Money(_amount, _prp + _inp, _adj, _bal));
                                }*/

                                    lss.Add(ls);
                                //}
                            }

                        }
                        else {

                            List<List<string>> lss = new List<List<string>>();

                            List<string> ls = new List<string>();

                          /*  double _amount, _prp, _inp, _adj, _bal;

                            Double.TryParse(fields[63], out _amount);
                            Double.TryParse(fields[64], out _prp);
                            Double.TryParse(fields[65], out _inp);
                            Double.TryParse(fields[66], out _adj);
                            Double.TryParse(fields[67], out _bal);

                            if (_amount == _bal && _prp == 0 && _inp == 0 && _adj == 0)
                            {
                                continue;
                            }
                            else {*/

                                ls.Add(fields[0]);  // Last Name

                                ls.Add(fields[1]);  // First Name
                                ls.Add(fields[2]);  // Account Id

                                ls.Add(fields[39]); //Post Date
                                ls.Add(fields[40]); // Service Date

                                ls.Add(fields[63]); // Amount
                                ls.Add(fields[64]); // PRP
                                ls.Add(fields[65]); // INP
                                ls.Add(fields[66]); // Adj
                                ls.Add(fields[67]); // Bal

                                lss.Add(ls);

                                _records.Add(key, lss);
                            //}
                        }
                    }
                }
            }
        }

        bool CheckChargessOneByOne(ref List<List<string>> rec)
        {
            bool result = true;
            foreach(var charge in rec)
            {
                SqlDataReader rdr = null;
                DatabaseReader.ReadChargesByADSDetailed(charge, ref rdr);

                if (rdr == null || rdr.HasRows == false)
                {
                    double payment = 0.0;
                    double prp, inp;

                    if (Double.TryParse(charge[6].ToString(), out prp) && Double.TryParse(charge[7].ToString(), out inp))
                    {
                        payment = prp + inp;
                    }

                    string msg = String.Format("charge is missing for account '{0}'- LastName: {1}, FirstName: {2}, DateOfService :{3}, DateOfPosting:{4}, Amount: {5}, Payment: {6}, Adjustment: {7}, Balance: {8}",
                         charge[2], charge[0], charge[1], charge[4], charge[3], charge[5], payment, charge[8], charge[9]);
                    Logger.Write(msg);
                    Console.WriteLine(msg);
                    result = false;
                    ++fail_count;
                }
                if(rdr != null)
                    rdr.Close();
            }
            return result;
        }

        bool CheckSum(List<List<string>> rec, DataRowCollection rows)
        {
            double sum_rec_amount = 0.0, sum_rows_amount = 0.0;

            double sum_rec_payment = 0.0, sum_rows_payment = 0.0;
            double sum_rec_adj = 0.0, sum_rows_adj = 0.0;
            double sum_rec_balance = 0.0, sum_rows_balance = 0.0;

            foreach (var record in rec)
            {
                double csv_payment = 0.0;
                double csv_billed = 0.0, csv_adj = 0.0, csv_bal = 0.0;
                double prp, inp;

                if (Double.TryParse(record[6].ToString(), out prp) && Double.TryParse(record[7].ToString(), out inp))
                {
                    csv_payment = prp + inp;
                }

                Double.TryParse(record[5], out csv_billed);
                Double.TryParse(record[8], out csv_adj);
                Double.TryParse(record[9], out csv_bal);

                sum_rec_amount += csv_billed;
                sum_rec_payment += csv_payment;
                sum_rec_adj += csv_adj;
                sum_rec_balance += csv_bal;
            }

            foreach (DataRow row in rows)
                {
                    if ((bool)row[9] == true)
                        continue;
                    double sql_billed, sql_payment, sql_adj, sql_bal;

                    Double.TryParse(row[5].ToString(), out sql_billed);
                    Double.TryParse(row[6].ToString(), out sql_payment);
                    Double.TryParse(row[7].ToString(), out sql_adj);
                    Double.TryParse(row[8].ToString(), out sql_bal);

                sum_rows_amount += sql_billed;
                sum_rows_payment += sql_payment;
                sum_rows_adj += sql_adj;
                sum_rows_balance += sql_bal;

            }
            return ((sum_rows_amount == sum_rows_amount) &&
                (sum_rec_payment == sum_rows_payment) &&
                (sum_rec_adj == sum_rows_adj) &&
                (sum_rec_balance == sum_rows_balance));
            }

        void CheckRecord(List<List<string>> rec)
        {
            SqlDataReader reader = null;

            Dictionary<DateTime, string> postDates = new Dictionary<DateTime, string>();
            foreach (var v in rec)
            {
                try {
                    postDates.Add(Convert.ToDateTime(v[4]), v[2]);
                }
                catch(Exception e)
                {

                }
            }

            DatabaseReader.ReadChargesByADSRecord(postDates, ref reader);

            if (reader == null || reader.HasRows == false)
            {
                if(reader != null)
                    reader.Close();
                if (!CheckChargessOneByOne(ref rec))
                {
                   // string msg = String.Format("number of records in Local SQL is 0 for account '{0}'- ADS:{1} records;",
                         // rec[0][2], rec.Count);
                    //Logger.Write(msg);
                    //Console.WriteLine(msg);                
                }
                else
                {
                    string msg = String.Format("check is passed successfully for account '{0}'", rec[0][2]);
                    Logger.Write(msg);
                    Console.WriteLine(msg);
                }
                return;
            }

            DataTable tb = new DataTable();
            tb.Columns.Add("LastName", typeof(string));  // 0
            tb.Columns.Add("FirstName", typeof(string)); // 1
            tb.Columns.Add("AccountId", typeof(string)); // 2

            tb.Columns.Add("PostDate", typeof(DateTime));  // 3
            tb.Columns.Add("ServiceDate", typeof(DateTime)); //4

            tb.Columns.Add("Amount", typeof(double));   // 5
            tb.Columns.Add("Payment", typeof(double));   // 6
            tb.Columns.Add("Adjustment", typeof(double));   // 7
            tb.Columns.Add("Balance", typeof(double));   // 8
            tb.Columns.Add("Visited", typeof(bool));   // 9

            while (reader.Read())
            {

                tb.Rows.Add(reader["LastName"],  // 0
                    reader["FirstName"],        // 1
                    reader["AccountId"],        // 2 
                    reader["DateOfService"],    // 3
                    reader["DateOfPosting"],    // 4
                    reader["Billed"],           // 5
                    reader["Payment"],          // 6
                    reader["Adjustment"],      // 7
                    reader["Balance"],         // 8
                    false                      // 9
                    );
            }

            if(rec.Count != tb.Rows.Count)
            {
                if (reader != null)
                    reader.Close();
                if (!CheckChargessOneByOne(ref rec))
                {
                    // string msg = String.Format("number of records in Local SQL is 0 for account '{0}'- ADS:{1} records;",
                    // rec[0][2], rec.Count);
                    //Logger.Write(msg);
                    //Console.WriteLine(msg);                
                }
                else
                {
                    string mm = String.Format("check is passed successfully for account '{0}'", rec[0][2]);
                    Logger.Write(mm);
                    Console.WriteLine(mm);
                }
                return;

                // report different number of records for this Account
               /* string msg = String.Format("number of records is different for account '{0}'- ADS:{1} and LocalSQL:{2} records;",
                    rec[0][2], rec.Count, tb.Rows.Count);
                Logger.Write(msg);
                Console.WriteLine(msg);*/
            }
            else
            {
                if(!CheckSum(rec, tb.Rows))
                {
                    string msg = String.Format("check sum is differ for account '{0}'", rec[0][2]);
                    Logger.Write(msg);
                    Console.WriteLine(msg);
                }
            }

            int found = 0;

            foreach(var record in rec)
            {
                double csv_payment = 0.0;
                double prp, inp;

                if (Double.TryParse(record[6].ToString(), out prp) && Double.TryParse(record[7].ToString(), out inp))
                {
                    csv_payment = prp + inp;
                }

                foreach (DataRow row in tb.Rows)
                {
                    if ((bool)row[9] == true)
                        continue;
                    double csv_billed, sql_billed, sql_payment, csv_adj, sql_adj, csv_bal, sql_bal;

                    Double.TryParse(record[5], out csv_billed);
                    Double.TryParse(row[5].ToString(), out sql_billed);
                    Double.TryParse(row[6].ToString(), out sql_payment);
                    Double.TryParse(record[8], out csv_adj);
                    Double.TryParse(row[7].ToString(), out sql_adj);
                    Double.TryParse(record[9], out csv_bal);
                    Double.TryParse(row[8].ToString(), out sql_bal);

                    /*string sss = String.Format("{0}=={1};  {2} == {3};  {4} == {5};  {6} == {7};",
                        csv_billed, sql_billed, csv_payment, sql_payment, csv_adj, sql_adj, csv_bal, sql_bal);

                    

                    if (record[2].CompareTo("22118/1") == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(sss);
                    }*/

                    if(csv_billed == sql_billed &&
                        csv_payment == sql_payment && 
                        csv_adj == sql_adj &&
                        csv_bal == sql_bal)
                    {
                        ++found;
                        //row.Delete();
                        row[9] = true;
                        break;
                    }
                }
               // tb.AcceptChanges();
            }

            if (found != rec.Count)
            {
                // Log the error
                string msg = String.Format("for account '{0}' - number of records from ADS file : {1}-,  Local SQL records that matched:{2};",
                     rec[0][2], rec.Count, found);
                Logger.Write(msg);
                Console.WriteLine(msg);
            }
            else
            {
                string msg = String.Format("check is passed successfully for account '{0}'", rec[0][2]);
                Logger.Write(msg);
                Console.WriteLine(msg);
            }
            reader.Close();
        }

        public void Check()
        {
            DatabaseReader.OpenConnection(new Parameters(
                ConfigurationManager.AppSettings["server"],
                ConfigurationManager.AppSettings["database"],
                ConfigurationManager.AppSettings["login"],
                ConfigurationManager.AppSettings["password"]));

            foreach (var rec in _records)
            {
                CheckRecord(rec.Value);
            }

            DatabaseReader.CloseConnection();
        }

    }

    class Program
    {
        static int Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("please specify start folder");
            }

            string logFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "data_integrity.log");

            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }

            string folder = args[0];

            string[] fileEntries = Directory.GetFiles(folder);
            int fails = 0;

            foreach (string file in fileEntries)
            {
                if (file.Contains("_ch"))
                {
                    Logger.Write("   ---- processing the file -- " + file);
                    Console.WriteLine("   ---- processing the file -- " + file);
                    IntegrityCheck check = new IntegrityCheck();
                    check.parseCsv(file);
                    check.Check();
                    fails += check.GetFailCount();
                }
            }

            //var path = @"F:\Projects\Upwork\MendelGratt\ADS_Data\5_ch.csv";
            //Process.Start("notepad.exe", logFile);

            //return fails;
            return 0;
        }
    }
}
