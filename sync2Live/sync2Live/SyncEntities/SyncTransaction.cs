using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live.SyncEntities
{
    public class SyncTransaction
    {
        public long Id { get; set; }
        public string NewId { get; set; }
        public string ChargeId { get; set; }
        public string DateOfService { get; set; }
        public string DateOfPosting { get; set; }
        public string TransactionId { get; set; }
        public string Billed { get; set; }
        public string Payment { get; set; }
        public string Adjustment { get; set; }
        public string Balance { get; set; }
        //public string UpdateDate { get; set; }

        public SyncTransaction(long _id, string _chargeId, string _dateOfService, string _dateOfPosting, 
            string _tranId, string _billed, string _payment, string _adj, string _bal/*, string _updateDate*/)
        {
            Id = _id;
            ChargeId = _chargeId;
            DateOfService = _dateOfService;
            DateOfPosting = _dateOfPosting;
            TransactionId = _tranId;
            Billed = _billed;
            Payment = _payment;
            Adjustment = _adj;
            Balance = _bal;
           // UpdateDate = _updateDate;
        }

        public string GetNewHttpJson()
        {
            const string tranRequestFormat = "{{ \"ChargeId\": {0}, \"TransactionId\": \"{1}\", \"DateOfService\": \"{2}\", \"DateOfPosting\": \"{3}\", \"Billed\": {4}, \"Payment\": {5}, \"Adjustment\": {6}, \"Balance\": {7}, \"Id\": {8} }}";

            string json = String.Format(
                 tranRequestFormat,
                 ChargeId,
                 TransactionId,
                 DateOfService,
                 DateOfPosting,
                 Billed,
                 Payment,
                 Adjustment,
                 Balance,
                 "null"
            );

            return json;
        }
    }
}
