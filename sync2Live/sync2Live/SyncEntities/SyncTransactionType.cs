using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live.SyncEntities
{
    class SyncTransactionType
    {
        long Id { get; set; }
        public string NewId { get; set; }
        string Name { get; set; }
        string SourceField { get; set; }
        string UpdateDate { get; set; }
        string CreateDate { get; set; }
    }
}
