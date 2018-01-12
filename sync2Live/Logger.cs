using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live
{
    static class Logger
    {
        static string fileName = Path.Combine((Environment.CurrentDirectory.ToString()),
    "sync2Live.log");
        static System.IO.StreamWriter log_file = new System.IO.StreamWriter(fileName, true);

       /* static string fileNameData = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
"sync2Live_data.log");
        static System.IO.StreamWriter log_file_data = new System.IO.StreamWriter(fileName, false);*/


        public static void Write(string line)
        {
            DateTime localDate = DateTime.Now;
            string ln = String.Format(@"{0} - {1}", localDate.ToString(), line);
            log_file.WriteLine(ln);
            log_file.Flush();
        }

     /*   public static void WriteCharges(string line)
        {
            DateTime localDate = DateTime.Now;
            string ln = String.Format(@"{0} - {1}", localDate.ToString(), line);
            log_file_data.WriteLine(ln);
            log_file_data.Flush();
        }*/
    }
}
