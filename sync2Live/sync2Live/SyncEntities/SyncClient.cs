using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live.SyncEntities
{
    public class SyncClient
    {
        public long Id { get; set; }
        public string NewId { get; set; }
        public string UpdateDate { get; set; }
        public string CreateDate { get; set; }
        public string Name { get; set; }
        public string Active { get; set; }



        public SyncClient(long _id, string _updateDate, string _createDate, string _name, string _active)
        {
            Id = _id;
          /*  if (_updateDate.Contains('/'))
            {
                UpdateDate = _updateDate.Replace('/', '-');
            }
            else*/
                UpdateDate = _updateDate;

           /* if (_createDate.Contains('/'))
            {
                CreateDate = _createDate.Replace('/', '-');
            }
            else*/
                CreateDate = _createDate;
            Name = _name;
            Active = _active.ToLower();
        }

        public string GetNewHttpJson()
        {
            const string clientRequestFmt = "{{\"Name\": \"{0}\", \"Active\": {1}, \"Id\": {2} }}";

            string req = String.Format(clientRequestFmt,
                    Name,
                    Active,
                    "null");

            

            return req;
        }
    }
}
