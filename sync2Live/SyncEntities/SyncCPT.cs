using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live.SyncEntities
{
    public class SyncCPT
    {
        public long Id { get; set; }
        public string NewId { get; set; }
        public string ClassId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UpdateDate { get; set; }
        public string CreateDate { get; set; }

        public SyncCPT(long _id, string _classId, string _name, string _desc, string _updDate, string _crDate)
        {
            Id = _id;
            ClassId = _classId;
            Name = _name;
            Description = _desc;
            UpdateDate = _updDate;
            CreateDate = _crDate;
        }

        public string GetNewHttpJson()
        {
            const string cptRequestFmt = "{{\"ClassId\": {0}, \"Name\" : \"{1}\", \"Description\": \"{2}\", \"Id\": {3} }}";

            string req = String.Format(
                cptRequestFmt,
                ClassId,
                Name,
                Description,
                "null");

            return req;
        }
    }

}