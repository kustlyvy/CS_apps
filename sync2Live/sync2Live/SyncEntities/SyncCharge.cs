using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sync2Live.SyncEntities
{
    public class SyncCharge
    {
        public long Id { get; set; }
        public string NewId { get; set; }
        public string DateOfService { get; set; }
        public string DateOfPosting { get; set; }
        public string PracticeId { get; set; }
        public string PatientId { get; set; }
        public string CPTId { get; set; }
        public string InsuranceCompanyId { get; set; }
        public string Billed { get; set; }
        public string Payment { get; set; }
        public string Adjustment { get; set; }
        public string Balance { get; set; }
        public string UpdateDate { get; set; }
        public string CreateDate { get; set; }
        public string DoctorId { get; set; }
        public string CaseNumber { get; set; }
        public string StatusId { get; set; }
        public string DueDate { get; set; }
        public string ClientId { get; set; }
        public string LiveId { get; set; }
        public string updated { get; set; }
        public string toDelete { get; set; }

        public SyncCharge(long _id, string _dateofService, string _dateofPosting, string _practiceId,
            string _patientId, string _cptId, string _insuranceCompanyId, string _billed, string _payment, string _adj,
            string _bal, string _updateDate, string _creaetDate, string _docId, string _caseNum, string _statId,
            string _dueDate, string _clientId, string _liveId, string _upd, string _del)
        {
            Id = _id;
            DateOfService = _dateofService;
            DateOfPosting = _dateofPosting;
            PracticeId = _practiceId;
            PatientId = _patientId;
            CPTId = _cptId;
            InsuranceCompanyId = _insuranceCompanyId;
            if (String.IsNullOrEmpty(_billed))
                Billed = "0.0";
            else
                Billed = _billed;

            if (String.IsNullOrEmpty(_payment))
                Payment = "0.0";
            else
                Payment = _payment;

            if (String.IsNullOrEmpty(_adj))
                Adjustment = "0.0";
            else
                Adjustment = _adj;

            if (String.IsNullOrEmpty(_bal))
                Balance = "0.0";
            else
                Balance = _bal;

            UpdateDate = _updateDate;
            CreateDate = _creaetDate;
            DoctorId = _docId;
            CaseNumber = _caseNum;

            if (String.IsNullOrEmpty(_statId))
            {
                StatusId = "null";
            }
            else
                StatusId = _statId;
            DueDate = _dueDate;
            ClientId = _clientId;
            LiveId = _liveId;
            updated = _upd;
            toDelete = _del;
        }

        public string GetNewHttpJson()
        {
            const string chargeRequestFmt = "{{\"PolicyNumber\": \"{0}\", \"PracticeId\": {1}, \"PatientId\": {2}, \"CPTId\": {3}, \"InsuranceCompanyId\": {4}, \"PhysicianId\": {5}, \"DateOfService\": \"{6}\", \"DateOfPosting\": \"{7}\", \"StatusId\": {8}, \"Billed\": {9}, \"Payment\": {10}, \"Adjustment\": \"{11}\", \"Balance\": {12}, \"CaseNumber\": \"{13}\", \"ClientId\":{14}, \"Id\": {15} }}";

            string json = String.Format(
                chargeRequestFmt,
                "null",
                PracticeId,
                PatientId,
                CPTId,
                InsuranceCompanyId,
                DoctorId,
                DateOfService,
                DateOfPosting,
                StatusId,
                Billed,
                Payment,
                Adjustment,
                Balance,
                "null",
                ClientId,
                "null" //Id
            );

            return json;
        }

        public string GetChargeUpdateHttpJson()
        {
            //const string chargeRequestFmt = "{{\"PolicyNumber\": \"{0}\", \"PracticeId\": {1}, \"PatientId\": {2}, \"CPTId\": {3}, \"InsuranceCompanyId\": {4}, \"PhysicianId\": {5}, \"DateOfService\": \"{6}\", \"DateOfPosting\": \"{7}\", \"StatusId\": {8}, \"Billed\": {9}, \"Payment\": {10}, \"Adjustment\": \"{11}\", \"Balance\": {12}, \"CaseNumber\": \"{13}\", \"ClientId\":{14}, \"Id\": {15} }}";
            const string chargeRequestFmt = "{{\"PolicyNumber\": \"{0}\", \"PracticeId\": {1}, \"PatientId\": {2}, \"CPTId\": {3}, \"InsuranceCompanyId\": {4}, \"PhysicianId\": {5}, \"DateOfService\": \"{6}\", \"DateOfPosting\": \"{7}\",  \"Billed\": {8}, \"Payment\": {9}, \"Adjustment\": \"{10}\", \"Balance\": {11}, \"CaseNumber\": \"{12}\", \"ClientId\":{13}, \"Id\": {14} }}";
            string json = String.Format(
                chargeRequestFmt,
                "null",
                PracticeId,
                PatientId,
                CPTId,
                InsuranceCompanyId,
                DoctorId,
                DateOfService,
                DateOfPosting,
                Billed,
                Payment,
                Adjustment,
                Balance,
                "null",
                ClientId,
                LiveId
            );

            return json;
        }
    }


}
