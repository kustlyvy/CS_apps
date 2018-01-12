using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live.SyncEntities
{
    public class SyncPatient
    {
        public long Id { get; set; }
        public long ClientId { get; set; }
        public string NewId { get; set; }
        public string PolicyNumber { get; set; }
        public string AccountId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; } 
        public string LastName { get; set; }
        public string DOB { get; set; }
        public string UpdateDate { get; set; } 
        public string CreateDate { get; set; }
        public string SSN { get; set; } 
        public string Policy { get; set; }
        public string Access { get; set; }

        public SyncPatient(long _id,
            long _ClientId,
            string _PolicyNumber,
            string _AccountId, 
            string _FirstName, 
            string _MiddleName, 
            string _LastName,
            string _DOB, 
            string _UpdateDate, 
            string _CreateDate, 
            string _SSN, 
            string _Policy, 
            string _Access)
        {
            Id = _id;
            ClientId = _ClientId;
            AccountId = _AccountId;

            if (String.IsNullOrEmpty(_FirstName))
            {
                FirstName = "N/A";
            }
            else {
                FirstName = _FirstName;
            }

            MiddleName = _MiddleName;
            LastName = _LastName;
            if (String.IsNullOrEmpty(_DOB))
            {
                DOB = "";// DateTime.Now.ToString();
            }
            else
                DOB = _DOB;
            UpdateDate = _UpdateDate;
            CreateDate = _CreateDate;
            SSN = _SSN;
            Policy = _Policy;
            Access = _Access.ToLower();
        }

        public string GetNewHttpJson()
        {
            const string patientRequestFmt = "{{ \"PolicyNumber\": \"{0}\", \"AccountId\": \"{1}\", \"FirstName\": \"{2}\", \"MiddleName\": \"{3}\", \"LastName\": \"{4}\", \"DOB\": \"{5}\", \"SSN\": \"{6}\", \"Policy\": \"{7}\", \"Access\": {8}, \"ClientId\": {9}, \"Id\": {10} }}";

            string req = String.Format(patientRequestFmt,
                           PolicyNumber,
                           AccountId,
                           FirstName,
                           MiddleName,
                           LastName,
                           DOB,
                           SSN,
                           Policy,
                           Access,
                           805,
                           "null");

            return req;
        }
    }
}
