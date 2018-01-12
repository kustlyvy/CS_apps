using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live.SyncEntities
{
    public class SyncDoc
    {
        public long Id { get; set; }
        public string NewId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProviderId { get; set; }
        public string UpdateDate { get; set; }
        public string CreateDate { get; set; }
        public string MiddleName { get; set; }

        public SyncDoc(long _id, string _firstName, string _lastName, string _providerId, 
            string _updDate, string _crDate, string _middleName)
        {
            Id = _id;
            FirstName = _firstName;
            LastName = _lastName;
            ProviderId = _providerId;
            UpdateDate = _updDate;
            CreateDate = _crDate;
            MiddleName = _middleName;
        }

        public string GetNewHttpJson()
        {
            const string doctorsRequestFmt = "{{\"FirstName\": \"{0}\", \"LastName\": \"{1}\", \"ProviderId\" : \"{2}\", \"MiddleName\": \"{3}\", \"Id\": {4} }}";
            //const string doctorsRequestFmt = "{{\"FirstName\": \"{0}\", \"LastName\": \"{1}\", \"ProviderId\" : \"{2}\", \"MiddleName\": \"{3}\", \"Id\": }}";
            string req = String.Format(
                doctorsRequestFmt,
                FirstName,
                LastName,
                ProviderId,
                MiddleName,
                "null"
            );

            /*string req = String.Format(
                doctorsRequestFmt,
                FirstName,
                LastName,
                ProviderId,
                MiddleName
            );*/

            return req;
        }
    }
}
