using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

using System.Configuration;


namespace sync2Live
{
    class Program
    {
         // TODO : use parameters for connection string

        // parameters: [0] - mode (real-time sync for Charges and Transactions) 

        static int Main(string[] args)
        {
            Logger.Write("Started;");


                string server = ConfigurationManager.AppSettings["server"];
                string database = ConfigurationManager.AppSettings["database"];
                string login = ConfigurationManager.AppSettings["login"];
                string password = ConfigurationManager.AppSettings["password"];
                string localDBName = ConfigurationManager.AppSettings["localDbName"];

            ApiSynchronizer syncr = new ApiSynchronizer(
                    new Parameters(
                        server,
                        database,
                        login,
                        password
                    )
                );

            if (args.Length == 0)
            {
                Logger.Write("app running in regular synchronizing mode");
                syncr.Synchronize();
            }
            //else if (args.Length > 0)
            //{
            //    Logger.Write("app running in real time synchronizing mode");
            //    syncr.RealTimeSync();
            //}
            else
                return -1;
               
            return 0;            
        }
    }
}
