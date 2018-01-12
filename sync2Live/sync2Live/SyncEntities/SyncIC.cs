using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live.SyncEntities
{
    public class SyncIC
    {
        public long ID { get; set; }
        public string NewId { get; set; }
        public string InsuranceId { get; set; }
        public string Name { get; set;  }
        public string UpdateDate { get; set;  }
        public string CreateDate { get; set; }
        public long ClientId { get; set; }
        public string DisplayName { get; set; }

        public SyncIC(long _id, string _iid, string _name, string _updDate, string _crDate, long _clientId, string _dispName)
        {
            ID = _id;
            InsuranceId = _iid; 
            Name = _name;
            UpdateDate = _updDate;
            CreateDate = _crDate;
            ClientId = _clientId;
            DisplayName = _dispName;
        }

        public string GetNewHttpJson()
        {
            const string insuranceCompanyRequestFmt = "{{\"InsuranceId\": \"{0}\", \"Name\": \"{1}\", \"ClientId\": {2}, \"DisplayName\": \"{3}\", \"Id\": {4} }}";

            string req = String.Format(
                insuranceCompanyRequestFmt,
                   InsuranceId,
                   Name,
                   ClientId,
                   DisplayName,
                   "null"
              );

            return req;
        }
    }
}
