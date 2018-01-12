using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dataNormalizer
{
    static class Logger
    {
        static string fileName = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "sync2Live.log");
        static System.IO.StreamWriter log_file = new System.IO.StreamWriter(fileName, true);

        public static void Write(string line)
        {
            DateTime localDate = DateTime.Now;
            string ln = String.Format(@"{0} - {1}", localDate.ToString(), line);
            log_file.WriteLine(ln);
            log_file.Flush();
        } 
    }
}
