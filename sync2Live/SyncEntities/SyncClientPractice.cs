using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live.SyncEntities
{
    class SyncClientPractice
    {
        long Id { get; set; }
        public string NewId { get; set; }
        long ClientId { get; set; }
        long PracticeId { get; set; }
        string UpdateDate { get; set; }
        string CreateDate { get; set; }

        SyncClientPractice(long _id, long _clientId, long _pracId, string _updateDate, string _createDate)
        {
            Id = _id;
            ClientId = _clientId;
            PracticeId = _pracId;
            UpdateDate = _updateDate;
            CreateDate = _createDate;
        }
    }
}
