using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live.SyncEntities
{
    public class SyncPractice
    {
        public long Id { get; set; }
        public string NewId { get; set; }
        public string Name { get; set; }
        public string UpdateDate { get; set; }
        public string CreateDate { get; set; }


        public SyncPractice(long _id, string _name, string _updDate, string _crDate)
        {
            Id = _id;
            Name = _name;
            UpdateDate = _updDate;
            CreateDate = _crDate;
        }

        public string GetNewHttpJson()
        {
            const string practiceRequestFmt = "{{\"Name\": \"{0}\", \"ClientId\": {1}, \"Id\": {2} }}";

            string req = String.Format(
                        practiceRequestFmt,
                        Name,
                        805,
                        "null");
            return req;
        }
    }
}
