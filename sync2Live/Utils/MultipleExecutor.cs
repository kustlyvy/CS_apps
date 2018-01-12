using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live.Utils
{
    class MultipleExecutor
    {
        public static bool ExecuteTimes(int times, string cmd, string table)
        {
            int execTimes = 5;
            bool result = true;
            while (execTimes > 0)
            {
                Logger.Write("trying to Update local " + table + " ids... ");
                int res = DatabaseReader.ExecuteCommand(cmd);
                if (res == -1)
                {
                    int num = (5 - execTimes) + 1;
                    Logger.Write("Update local " + table + " ids failed: on try " + num.ToString());
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
    }
}
