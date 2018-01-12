// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Data.OleDb;
using System.Data;
using Excel;

using System.Windows.Forms;



namespace advancedMDsync
{
    class Program
    {

        static void Main(string[] args)
        {
            string id = "All";
            string mode = "Daily";
            int time = 365;
            if (args.Length > 1)
            {
                Int32.TryParse(args[0], out time);
                id = args[1];
                if (args.Length == 3)
                {
                    if ((args[2].CompareTo("Daily") == 0) || (args[2].CompareTo("Sync") == 0))
                    {
                        mode = args[2];
                    }
                }
            }
             
            try {

                DateTime dtStart;
                if (mode.CompareTo("Sync") == 0)
                {

                   dtStart = new DateTime(2016, 1, 1);
                }
                else
                {
                    dtStart = DateTime.Now.AddDays(-time);
                }
                DateTime dtEnd;
                bool bFinish = false;
                int suffix = 0;

                while (!bFinish)
                {
                    ++suffix;

                    string begDate = dtStart.ToString("MM/dd/yyyy"); 

                    string curDate;
                    if (DateTime.Now.Subtract(dtStart).TotalDays > 365)
                    {
                        dtEnd = dtStart.AddDays(365);
                        dtEnd = new DateTime(dtStart.Year, 12, 31);
                        curDate = dtEnd.ToString("MM/dd/yyyy");
                    }
                    else {
                        bFinish = true;
                        dtEnd = DateTime.Now;
                        curDate = dtEnd.ToString("MM/dd/yyyy");
                    }                    
                    bool passed;
                    passed = false;
                    int cnt = 0;
                    
                    while (!passed && cnt < 5)
                    {
                        Scraper scr = new Scraper();
                        scr.readCreds();
                        scr.Login();
                        System.Threading.Thread.Sleep(1000);
                        scr.SnoozeAll();
                        System.Threading.Thread.Sleep(1000);
                        scr.Understand();
                        System.Threading.Thread.Sleep(1000);
                        scr.SkipNewAppointmentPopup();
                        System.Threading.Thread.Sleep(1000);
                        passed = scr.ClickReportsMenu();
                        if (passed) passed = scr.ClickFinancialTotalsMenu();
                        if (passed) passed = scr.ClickTransactionDetailMenu();

                        if (passed) passed = scr.SwitchToReportWIndow();
                        if (passed) passed = scr.CheckExportOnRun();
                        if (passed) passed = scr.SaveReports(time, id, suffix, begDate, curDate);
                        if (!passed) scr.close();
                        cnt++;


                    }
                    dtStart = dtEnd.AddDays(1);

                    System.Threading.Thread.Sleep(10000);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                Console.WriteLine(e.ToString());
            }
        }
    }
}
