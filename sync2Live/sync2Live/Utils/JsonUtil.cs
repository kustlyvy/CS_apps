using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace sync2Live.Utils
{
    class JsonUtil
    {
        public static List<string> GetListOfIds(string js)
        {
            List<string> exIds = new List<string>();
            JObject o = JObject.Parse(js);

            var n = o.First;
            var f = n.First;
            foreach (var r in f)
            {
                string s = r["Id"].ToString();
                exIds.Add(s);
            }

            return exIds;
        }

        public static List<string> GetListOfReturnedIds(string js)
        {
            List<string> exIds = new List<string>();
            JObject o = null;
            try {
                js.TrimStart();
                js.TrimEnd();
                o = JObject.Parse(js);


            var n = o.First;
            var f = n.First;
            foreach (var r in f)
            {
                exIds.Add(r.ToString());
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(js);
            }

            return exIds;
        }

        public static string GetFirstModelId(string js)
        {
            JObject o = JObject.Parse(js);
            var n = o.First;
            return n.First.ToString();
        }
    }
}
