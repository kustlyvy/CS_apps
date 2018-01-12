using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Collections;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace sync2Live
{
	public class Request
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern void OutputDebugString(string message);

		//const string patientRequestFmt = "{{ 'PolicyNumber': '{0}', 'AccountId': '{1}', 'FirstName': '{2}', 'MiddleName': '{3}', 'LastName': '{4}', 'DOB': '{5}', 'SSN': '{6}', 'Policy': '{7}', 'Access': {8}, 'ClientId': {9}, 'Id': {10} }}";
		//const string clientRequestFmt = "{{\"Name\": \"{0}\", \"Active\": {1}, \"Id\": {2} }}";
		//const string clientPracticeFmt = "{{\"Name\": \"{0}\", \"ClientId\": {1}, \"Id\": {2} }}";
		// const string insuranceCompanyRequestFmt = "{{\"InsuranceId\": \"{0}\", \"Name\": \"{1}\", \"ClientId\": {2}, \"DisplayName\": \"{3}\", \"Id\": {4} }}";
		//const string practiceRequestFmt = "{{\"Name\": \"{0}\", \"ClientId\": {1}, \"Id\": {2} }}";
		//const string doctorsRequestFmt = "{{\"FirstName\": \"{0}\", \"LastName\": \"{1}\", \"ProviderId\" : {2}, \"MiddleName\": \"{3}\", \"Id\": {4} }}";
		const string transactionTypesRequestFmt = "{{\"Name\": \"{0}\", \"ProviderId\" : {1}, \"Id\": {2} }}";
		//const string cptRequestFmt = "{{\"ClassId\": {0}, \"Name\" : \"{1}\", \"Description\": \"{2}\", \"Id\": {3} }}";
		//const string chargeRequestFmt = "{{\"PolicyNumber\": \"{0}\", \"PracticeId\": {1}, \"PatientId\": {2}, \"CPTId\": {3}, \"InsuranceCompanyId\": {4}, \"PhysicianId\": {5}, \"DateOfService\": \"{6}\", \"DateOfPosting\": \"{7}\", \"StatusId\": {8}, \"Billed\": {9}, \"Payment\": {10}, \"Adjustment\": \"{11}\", \"Balance\": {12}, \"CaseNumber\": \"{13}\", \"Id\": {14} }}";
/*
		const string testPatients = "{\"List\": [ { \"PolicyNumber\": \"sample string 1\", \"AccountId\": \"sample string 2\", \"FirstName\": \"sample string 3\", \"MiddleName\": \"sample string 4\", \"LastName\": \"sample string 5\", \"DOB\": \"2016-09-30T14:39:26.2399528+01:00\", \"SSN\": \"sample string 6\", \"Policy\": \"sample string 7\", \"Access\": true, \"ClientId\": 1, \"Id\": 1 }, { \"PolicyNumber\": \"sample string 1\", \"AccountId\": \"sample string 2\", \"FirstName\": \"sample string 3\", \"MiddleName\": \"sample string 4\", \"LastName\": \"sample string 5\", \"DOB\": \"2016-09-30T14:39:26.2399528+01:00\", \"SSN\": \"sample string 6\", \"Policy\": \"sample string 7\", \"Access\": true, \"ClientId\": 1, \"Id\": 1
	}
  ],
  "RowsCount": 1,
  "Status": true,
  "Message": "sample string 3",
  "Errors": {
	"sample string 1": "sample string 2",
	"sample string 3": "sample string 4"
  }
}";*/

		const string srvAPIURL = "http://collectionsystem.grebinnyk.com/exports";

		static List<string> GetListOfIds(string js)
		{
			List<string> exIds = new List<string>();
			JObject o = JObject.Parse(js);

			var n = o.First;
			var f = n.First;
			foreach (var r in f)
			{
				string s = r["Id"].ToString();
				exIds.Add(s);
			}

			return exIds;
		}

		

		static List<string> GetListOfAccountIds(string js)
		{
			List<string> exIds = new List<string>();
			JObject o = JObject.Parse(js);

			var n = o.First;
			var f = n.First;
			foreach (var r in f)
			{
				string s = r["AccountId"].ToString();
				exIds.Add(s);
			}

			return exIds;
		}

		static List<string> GetListOfInsuranceIds(string js)
		{
			List<string> exIds = new List<string>();
			JObject o = JObject.Parse(js);

			var n = o.First;
			var f = n.First;
			foreach (var r in f)
			{
				string s = r["InsuranceId"].ToString();
				exIds.Add(s);
			}

			return exIds;
		}

		static string CreateListParams(List<string> prms)
		{

			
			string result = "";
			result = result.Insert(0, "{ \"List\": [");


			
			int i = 0;
			foreach (var row in prms)
			{
				if (i > 0)
				{
					result = result.Insert(result.Length, ",");
				}
				result = result.Insert(result.Length, row);
				++i;
			}
			return result.Insert(result.Length, "]}");
			



		}

		public static string CreatePatientsRequest(
			ref SqlDataReader reader, 
			ref bool list, 
			ref List<SyncEntities.SyncPatient> listPat)
		{
			if (reader == null)
				return "";

			List<string> exIds = null;
			try
			{
				string srv_clients = HttpHandler.Get(srvAPIURL+"/Patient/List?clientId=805");
				exIds = GetListOfAccountIds(srv_clients);
			}
			catch (Exception e)
			{
				Logger.Write("Unable to retrieve list of Patients from server - " + e.ToString());
				return "";
			}

			List<string> patients = new List<string>();
			string result = "";
		  
			JArray jl = new JArray();

			if (exIds != null && exIds.Count > 0)
			{
				while (reader.Read())
				{
					if (!exIds.Contains(reader["AccountId"].ToString().Trim()))
					{
						SyncEntities.SyncPatient pt = new SyncEntities.SyncPatient(
							(long)reader["Id"],
							805,
							reader["PolicyNumber"].ToString().Trim(),
							reader["AccountId"].ToString().Trim(),
							reader["FirstName"].ToString().Trim(),
							reader["MiddleName"].ToString().Trim(),
							reader["LastName"].ToString().Trim(),
							reader["DOB"].ToString().Trim(),
							reader["UpdateDate"].ToString().Trim(),
							reader["CreateDate"].ToString().Trim(),
							reader["SSN"].ToString().Trim(),
							reader["Policy"].ToString().Trim(),
							reader["Access"].ToString().Trim());

						patients.Add(pt.GetNewHttpJson());
		  
						listPat.Add(pt);
		
					}
				}
			}
			else
			{
				while (reader.Read())
				{
					SyncEntities.SyncPatient pt = new SyncEntities.SyncPatient(
						  (long)reader["Id"],
						  805,
						  reader["PolicyNumber"].ToString().Trim(),
						  reader["AccountId"].ToString().Trim(),
						  reader["FirstName"].ToString().Trim(),
						  reader["MiddleName"].ToString().Trim(),
						  reader["LastName"].ToString().Trim(),
						  reader["DOB"].ToString().Trim(),
						  reader["UpdateDate"].ToString().Trim(),
						  reader["CreateDate"].ToString().Trim(),
						  reader["SSN"].ToString().Trim(),
						  reader["Policy"].ToString().Trim(),
						  reader["Access"].ToString().Trim());

						  patients.Add(pt.GetNewHttpJson());

						  listPat.Add(pt);
				}
			}

			if (patients.Count <= 0)
				return "";

			if (patients.Count == 1)
			{
				return patients.ElementAt(0).Trim();
			}
			else
			{
				list = true;
				result = CreateListParams(patients);
				return result;
			}
		}

		public static Dictionary<string, List<SyncEntities.SyncPatient>> CreatePatientsRequestList(ref SqlDataReader reader)
		{
			if (reader == null)
				return null;

			Dictionary<string, List<SyncEntities.SyncPatient>> requests_pat = new Dictionary<string, List<SyncEntities.SyncPatient>>();
			List<string> cpts = new List<string>();
			List<SyncEntities.SyncPatient> listPat = new List<SyncEntities.SyncPatient>();
			int count = 0;


			List<string> exIds = null;
			try
			{
				string srv_clients = HttpHandler.Get(srvAPIURL + "/Patient/List?clientId=805");
				exIds = GetListOfIds(srv_clients);
			}
			catch (Exception e)
			{
				Logger.Write("Unable to retrieve list of Patients from server - " + e.ToString());
				return null;
			}

			if (exIds != null && exIds.Count > 0)
			{
				while (reader.Read())
				{
					if (!exIds.Contains(reader["Id"].ToString().Trim()))
					{
						SyncEntities.SyncPatient pt = new SyncEntities.SyncPatient(
						(long)reader["Id"],
						805,
						reader["PolicyNumber"].ToString().Trim(),
						reader["AccountId"].ToString().Trim(),
						reader["FirstName"].ToString().Trim(),
						reader["MiddleName"].ToString().Trim(),
						reader["LastName"].ToString().Trim(),
						reader["DOB"].ToString().Trim(),
						reader["UpdateDate"].ToString().Trim(),
						reader["CreateDate"].ToString().Trim(),
						reader["SSN"].ToString().Trim(),
						reader["Policy"].ToString().Trim(),
						reader["Access"].ToString().Trim());

						cpts.Add(pt.GetNewHttpJson());
						listPat.Add(pt);
						++count;

						if (count >= 100)
						{
							string json = CreateListParams(cpts);
							List<SyncEntities.SyncPatient> lc = new List<SyncEntities.SyncPatient>();
							lc.AddRange(listPat);
							requests_pat.Add(json, lc);
							cpts.Clear();
							listPat.Clear();
							json = "";
							count = 0;
						}
					}
				}
			}
			else {
				while (reader.Read())
				{
					SyncEntities.SyncPatient pt = new SyncEntities.SyncPatient(
						  (long)reader["Id"],
						  805,
						  reader["PolicyNumber"].ToString().Trim(),
						  reader["AccountId"].ToString().Trim(),
						  reader["FirstName"].ToString().Trim(),
						  reader["MiddleName"].ToString().Trim(),
						  reader["LastName"].ToString().Trim(),
						  reader["DOB"].ToString().Trim(),
						  reader["UpdateDate"].ToString().Trim(),
						  reader["CreateDate"].ToString().Trim(),
						  reader["SSN"].ToString().Trim(),
						  reader["Policy"].ToString().Trim(),
						  reader["Access"].ToString().Trim());

					cpts.Add(pt.GetNewHttpJson());
					listPat.Add(pt);
					++count;

					if (count >= 100)
					{
						string json = CreateListParams(cpts);
						List<SyncEntities.SyncPatient> lc = new List<SyncEntities.SyncPatient>();
						lc.AddRange(listPat);
						requests_pat.Add(json, lc);
						cpts.Clear();
						listPat.Clear();
						json = "";
						count = 0;
					}
				}

			if (cpts.Count > 0)
			{
				string js = CreateListParams(cpts);
				List<SyncEntities.SyncPatient> ll = new List<SyncEntities.SyncPatient>();
				ll.AddRange(listPat);
				requests_pat.Add(js, ll);
			}
		}

			if (requests_pat.Count <= 0)
				return null;

			return requests_pat;

		}


		public static void CreateClientsRequest(
			ref SqlDataReader reader, 
			ref List<string> req_list, 
			ref List<SyncEntities.SyncClient> listCli)
		{
			if (reader == null)
				return;
			
			List<string> exIds = null;
			 try
			{
				string srv_clients = HttpHandler.Get(srvAPIURL+"/Client/List");
				exIds = GetListOfIds(srv_clients);
			}
			catch (Exception e)
			{
				Logger.Write("Unable to retrieve list of Practices from server - " + e.ToString());
				return;
			}

			if(exIds.Count == 0){
				while (reader.Read())
				{
					SyncEntities.SyncClient cl = new SyncEntities.SyncClient(
						(long)reader["Id"], 
						reader["UpdateDate"].ToString().Trim(),
						reader["CreateDate"].ToString().Trim(),
						reader["Name"].ToString().Trim(),
						reader["Active"].ToString().Trim()
						);

					req_list.Add(cl.GetNewHttpJson());
					listCli.Add(cl);
					
				}
			}
			else {
				while (reader.Read())
				{
					if(!exIds.Contains(reader["Id"].ToString().Trim())){
						SyncEntities.SyncClient cl = new SyncEntities.SyncClient(
						   (long)reader["Id"],
						   reader["UpdateDate"].ToString(),
						   reader["CreateDate"].ToString(),
						   reader["Name"].ToString().Trim(),
						   reader["Active"].ToString().Trim()
						);

						req_list.Add(cl.GetNewHttpJson());
						listCli.Add(cl);
					}
				}
			}
		}

		public static string CreateInsuranceCompaniesRequest(ref SqlDataReader reader, ref bool list, ref List<SyncEntities.SyncIC> listIC)
		{
			if (reader == null)
				return "";

			ArrayList objs = new ArrayList();
			List<string> exIds = null;
			try
			{
				string srv_ics = HttpHandler.Get(srvAPIURL+"/InsuranceCompany/List?clientId=805");
				exIds = GetListOfInsuranceIds(srv_ics);
			}
			catch (Exception e)
			{
				Logger.Write("Unable to retrieve list of Insurance Companies from server - " + e.ToString());
				return "";
			}

			List<string> ics = new List<string>();
			string result = "";

			if (exIds.Count == 0)
			{
				while (reader.Read())
				{
					SyncEntities.SyncIC ic = new SyncEntities.SyncIC(
						(long)reader["Id"],
						reader["InsuranceId"].ToString().Trim(),
						reader["Name"].ToString().Trim(),
						reader["UpdateDate"].ToString().Trim(),
						reader["CreateDate"].ToString().Trim(),
						805,
						reader["DisplayName"].ToString().Trim()
					);

					objs.Add(new
					{
						Id = (string)null,
						InsuranceID = reader["InsuranceId"].ToString().Trim(),
						Name = reader["Name"].ToString().Trim(),
						ClientID=805,
						DisplayNAme = reader["DisplayName"].ToString().Trim()
					});

					ics.Add(ic.GetNewHttpJson());
					listIC.Add(ic);
				}
			}
			else
			{
				while (reader.Read())
				{
					if (!exIds.Contains(reader["InsuranceId"].ToString().Trim())) {

						SyncEntities.SyncIC ic = new SyncEntities.SyncIC(
						(long)reader["Id"],
						reader["InsuranceId"].ToString().Trim(),
						reader["Name"].ToString().Trim(),
						reader["UpdateDate"].ToString().Trim(),
						reader["CreateDate"].ToString().Trim(),
						805,
						reader["DisplayName"].ToString().Trim()
						);

						objs.Add(new
						{
							Id = (string)null,
							InsuranceID = reader["InsuranceId"].ToString().Trim(),
							Name = reader["Name"].ToString().Trim(),
							ClientID = 805,
							DisplayNAme = reader["DisplayName"].ToString().Trim()
						});

						ics.Add(ic.GetNewHttpJson());
						listIC.Add(ic);
					}
				}
			}

			if (ics.Count <= 0)
				return "";

			if (ics.Count == 1)
			{
				return ics.ElementAt(0).Trim();
			}
			else
			{
				list = true;
				result = CreateListParams(ics);
				return result;
			}

		}

		public static string CreatePracticesRequest(ref SqlDataReader reader, ref bool list, ref List<SyncEntities.SyncPractice> listPrc)
		{
			if (reader == null)
				return "";			
			
			List<string> exIds = null;
			try
			{
				string srv_practices = HttpHandler.Get(srvAPIURL+"/Practice/List?clientId=805");
				exIds = GetListOfIds(srv_practices);
			}
			catch (Exception e)
			{
				Logger.Write("Unable to retrieve list of Practices from server - " + e.ToString());
				return "";
			}

			List<string> prcts = new List<string>();
			string result = "";

			
			if(exIds.Count == 0){
				while (reader.Read())
				{
					SyncEntities.SyncPractice prc = new SyncEntities.SyncPractice(
						(long)reader["Id"],
						reader["Name"].ToString().Trim(),
						reader["UpdateDate"].ToString().Trim(),
						reader["CreateDate"].ToString().Trim()
						);               

					prcts.Add(prc.GetNewHttpJson());
					listPrc.Add(prc);
				}
			}
			else {
				 while (reader.Read())
				{
					if(!exIds.Contains(reader["Id"].ToString().Trim())){
						SyncEntities.SyncPractice prc = new SyncEntities.SyncPractice(
							(long)reader["Id"],
							reader["Name"].ToString().Trim(),
							reader["UpdateDate"].ToString().Trim(),
							reader["CreateDate"].ToString().Trim()
							);

						prcts.Add(prc.GetNewHttpJson());
						listPrc.Add(prc);
					}
				}				
			}

			if (prcts.Count <= 0)
				return "";

			if (prcts.Count == 1)
			{
				return prcts.ElementAt(0).Trim();
			}
			else
			{
				list = true;
				result = CreateListParams(prcts);
				return result;
			}
		}

		
	  /*  public static string CreateClientPracticesRequest(ref SqlDataReader reader, ref bool list)
		{
			List<string> clprcts = new List<string>();
			string result = "";

			while (reader.Read())
			{
				string req = String.Format(
					clientPracticeFmt,
					reader["Name"].ToString().Trim(),
					805,
					reader["Id"].ToString().Trim());

				clprcts.Add(req);
			}

			if (clprcts.Count <= 0)
				return "";

			if (clprcts.Count == 1)
			{
				return clprcts.ElementAt(0).Trim();
			}
			else
			{
				list = true;
				result = CreateListParams(clprcts);
				return result;
			}
		} */

		public static string CreateDoctorsRequest(ref SqlDataReader reader, ref bool list, ref List<SyncEntities.SyncDoc> listDoc)
		{
			if (reader == null)
				return "";


			ArrayList objs = new ArrayList();
			List<string> exIds = null;
			try
			{
				string srv_doctors = HttpHandler.Get(srvAPIURL+"/Physician/List");
				exIds = GetListOfIds(srv_doctors);
				
			}
			catch (Exception e)
			{
				Logger.Write("Unable to retrieve list of Physicians from server - " + e.ToString());
				return "";
			}

			List<string> doctors = new List<string>();
			string result = "";


	 


			if (exIds.Count == 0)
			{
				while (reader.Read())
				{

					SyncEntities.SyncDoc doc = new SyncEntities.SyncDoc(
						(long)reader["Id"],
						reader["FirstName"].ToString().Trim().Replace("\"", "'"),
						reader["LastName"].ToString().Trim().Replace("\"", "'"),
						reader["ProviderId"].ToString().Trim().Replace("\"", "'"),
						reader["UpdateDate"].ToString().Trim().Replace("\"", "'"),
						reader["CreateDate"].ToString().Trim().Replace("\"", "'"),
						reader["MiddleName"].ToString().Trim().Replace("\"", "'")
						);



					objs.Add(new {
						Id = (string) null,
						FirstName = reader["FirstName"].ToString().Trim(),
						LastName=reader["LastName"].ToString().Trim(),
						ProviderId=reader["ProviderId"].ToString().Trim(),
						MiddleName = reader["MiddleName"].ToString().Trim()
					});

					doctors.Add(doc.GetNewHttpJson());
					listDoc.Add(doc);
				}
			}
			else
			{
				while (reader.Read())
				{
					if (!exIds.Contains(reader["Id"].ToString().Trim()))
					{
						SyncEntities.SyncDoc doc = new SyncEntities.SyncDoc(
							(long)reader["Id"],
							reader["FirstName"].ToString().Trim().Replace("\"", "'"),
							reader["LastName"].ToString().Trim().Replace("\"", "'"),
							reader["ProviderId"].ToString().Trim().Replace("\"", "'"),
							reader["UpdateDate"].ToString().Trim().Replace("\"", "'"),
							reader["CreateDate"].ToString().Trim().Replace("\"", "'"),
							reader["MiddleName"].ToString().Trim().Replace("\"", "'")
							);


							objs.Add(new {
											Id = (string) null,
											FirstName = reader["FirstName"].ToString().Trim(),
											LastName=reader["LastName"].ToString().Trim(),
											ProviderId=reader["ProviderId"].ToString().Trim(),
											MiddleName = reader["MiddleName"].ToString().Trim()
							});



						
						
						doctors.Add(doc.GetNewHttpJson());
						listDoc.Add(doc);
					}
				}

			}

			if (doctors.Count <= 0)
				return "";

			if (doctors.Count == 1)
			{
				return doctors.ElementAt(0).Trim();
			}
			else
			{
				list = true;


				result = CreateListParams (doctors);
				result = "";
				result = result.Insert(0, "{ \"List\":");
				string req1 = JsonConvert.SerializeObject(objs);
				

				result = result.Insert(result.Length, req1);
				result = result.Insert(result.Length, "}");
				return result;
			}
		}


		public static string CreateTransactionTypesRequest(ref SqlDataReader reader, ref bool list, ref Dictionary<string, string> idsToUpdate)
		{
			if (reader == null)
				return "";
			
			List<string> exIds = null;
			try
			{
				string srv_trtypes = HttpHandler.Get(srvAPIURL+"/TransactionType/List");
				exIds = GetListOfIds(srv_trtypes);
			}
			catch (Exception e)
			{
				Logger.Write("Unable to retrieve list of Transaction Types from server - " + e.ToString());
				return "";
			}

			List<string> trtps = new List<string>();
			string result = "";

			if (exIds.Count == 0)
			{
				while (reader.Read())
				{
					string req = String.Format(
						transactionTypesRequestFmt,
						reader["Name"].ToString().Trim(),
						reader["SourceFileId"].ToString().Trim(),
						reader["Id"].ToString().Trim());

					trtps.Add(req);
					idsToUpdate.Add(reader["Id"].ToString().Trim(), "");
				}
			}
			else
			{
				while (reader.Read())
				{
					if (!exIds.Contains(reader["Id"].ToString().Trim()))
					{
						string req = String.Format(
							transactionTypesRequestFmt,
							reader["Name"].ToString().Trim(),
							reader["SourceFileId"].ToString().Trim(),
							reader["Id"].ToString().Trim());

						trtps.Add(req);
						idsToUpdate.Add(reader["Id"].ToString().Trim(), "");
					}
				}

			}

			if (trtps.Count <= 0)
				return "";

			if (trtps.Count == 1)
			{
				return trtps.ElementAt(0).Trim();
			}
			else
			{
				list = true;
				result = CreateListParams(trtps);
				return result;
			}
		}

		public static string CreateCPTRequest(ref SqlDataReader reader, ref bool list, ref List<SyncEntities.SyncCPT> listCPT)
		{
			if (reader == null)
				return "";
			
			List<string> exIds = null;
			try
			{
				string srv_trtypes = HttpHandler.Get(srvAPIURL+"/CPT/List");
				exIds = GetListOfIds(srv_trtypes);
			}
			catch (Exception e)
			{
				Logger.Write("Unable to retrieve list of CPTs from server - " + e.ToString());
				return "";
			}

			List<string> cpts = new List<string>();
			string result = "";

			if (exIds.Count == 0)
			{
				while (reader.Read())
				{
					SyncEntities.SyncCPT cpt = new SyncEntities.SyncCPT(
						(long)reader["Id"],
						reader["ClassId"].ToString().Trim(),
						reader["Name"].ToString().Trim(),
						reader["Description"].ToString().Trim(),
						reader["UpdateDate"].ToString().Trim(),
						reader["CreateDate"].ToString().Trim()
					);

					cpts.Add(cpt.GetNewHttpJson());
					listCPT.Add(cpt);
				}
			}
			else
			{
				while (reader.Read())
				{
					if (!exIds.Contains(reader["Id"].ToString().Trim()))
					{
						SyncEntities.SyncCPT cpt = new SyncEntities.SyncCPT(
							(long)reader["Id"],
							reader["ClassId"].ToString().Trim(),
							reader["Name"].ToString().Trim(),
							reader["Description"].ToString().Trim(),
							reader["UpdateDate"].ToString().Trim(),
							reader["CreateDate"].ToString().Trim()
						);

						cpts.Add(cpt.GetNewHttpJson());
						listCPT.Add(cpt);
					}
				}
			}

			// code for testing
			/*  string req = String.Format(
	  "{{\"ClassId\": {0}, \"Name\" : \"{1}\", \"Description\": \"{2}\", \"Id\": {3} }}",
	  11,
	  "Name",
	  "Description",
	  6502);

			  cpts.Add(req);

			  string req2 = String.Format(
  "{{\"ClassId\": {0}, \"Name\" : \"{1}\", \"Description\": \"{2}\", \"Id\": {3} }}",
  11,
  "Name",
  "Description",
  6503);

			  cpts.Add(req2);*/

			if (cpts.Count <= 0)
				return "";

			if (cpts.Count == 1)
			{
				return cpts.ElementAt(0).Trim();
			}
			else
			{
				list = true;
				result = CreateListParams(cpts);
				return result;
			}
		}

		public static string CreateChargeRequest(ref SqlDataReader reader, ref bool list, ref List<SyncEntities.SyncCharge> listCh)
		{
			if (reader == null)
				return "";

			string json = "";
			List<string> cpts = new List<string>();

			while (reader.Read())
			{
				SyncEntities.SyncCharge ch = new SyncEntities.SyncCharge(
					(long)reader["Id"],
					reader["DateOfService"].ToString().Trim(),
					reader["DateOfPosting"].ToString().Trim(),
					reader["PracticeId"].ToString().Trim(),
					reader["PatientId"].ToString().Trim(),
					reader["CPTId"].ToString().Trim(),
					reader["InsuranceCompanyId"].ToString().Trim(),
					reader["Billed"].ToString().Trim(),
					reader["Payment"].ToString().Trim(),
					reader["Adjustment"].ToString().Trim(),
					reader["Balance"].ToString().Trim(),
					reader["UpdateDate"].ToString().Trim(),
					reader["CreateDate"].ToString().Trim(),
					reader["DoctorId"].ToString().Trim(),
					reader["CaseNumber"].ToString().Trim(),
					reader["StatusId"].ToString().Trim(),
					reader["DueDate"].ToString().Trim(),
					"805",
					reader["LiveID"].ToString().Trim(),
					reader["updated"].ToString().Trim(),
					reader["toDelete"].ToString().Trim()
				);
			

				cpts.Add(ch.GetNewHttpJson());
				listCh.Add(ch);
			}

			if (cpts.Count <= 0)
				return "";

			return CreateListParams(cpts);
		}


		public static Dictionary<string, List<SyncEntities.SyncCharge>> CreateChargeRequestList(ref SqlDataReader reader, ref List<string> charges2Update)
		{
			if (reader == null)
				return null;

			Dictionary<string, List<SyncEntities.SyncCharge>> requests_ch = new Dictionary<string, List<SyncEntities.SyncCharge>>(StringComparer.OrdinalIgnoreCase);
			List<string> cpts = new List<string>();
		   
			List<SyncEntities.SyncCharge> listCh = new List<SyncEntities.SyncCharge>();
			int count = 0;

			while (reader.Read())
			{
					SyncEntities.SyncCharge ch = new SyncEntities.SyncCharge(
						(long)reader["Id"],
						reader["DateOfService"].ToString().Trim(),
						reader["DateOfPosting"].ToString().Trim(),
						reader["PracticeId"].ToString().Trim(),
						reader["PatientId"].ToString().Trim(),
						reader["CPTId"].ToString().Trim(),
						reader["InsuranceCompanyId"].ToString().Trim(),
						reader["Billed"].ToString().Trim(),
						reader["Payment"].ToString().Trim(),
						reader["Adjustment"].ToString().Trim(),
						reader["Balance"].ToString().Trim(),
						reader["UpdateDate"].ToString().Trim(),
						reader["CreateDate"].ToString().Trim(),
						reader["DoctorId"].ToString().Trim(),
						reader["CaseNumber"].ToString().Trim(),
						reader["StatusId"].ToString().Trim(),
						reader["DueDate"].ToString().Trim(),
						"805",
						reader["LiveID"].ToString().Trim(),
						reader["updated"].ToString().Trim(),
						reader["toDelete"].ToString().Trim()
					);

			   /* string ss = ch.GetNewHttpJson();
				if (!ss.Contains("PolicyNumber"))
				{
					int jj = 0;
				}*/
					cpts.Add(ch.GetNewHttpJson());
					charges2Update.Add(ch.GetChargeUpdateHttpJson());

					listCh.Add(ch);
					++count;

				if(count >= 100)
				{
					string json = CreateListParams(cpts);
					List<SyncEntities.SyncCharge> lc = new List<SyncEntities.SyncCharge>();
					 
					lc.AddRange(listCh);
					try
					{
						requests_ch.Add(json, lc);
					}
					catch { }
					cpts.Clear();
					listCh.Clear();
					json = "";
					count = 0;
				}
			}

			if(cpts.Count > 0)
			{
				string js = CreateListParams(cpts);
				List<SyncEntities.SyncCharge> ll = new List<SyncEntities.SyncCharge>();
				ll.AddRange(listCh);
				requests_ch.Add(js, ll);
			}

			if (requests_ch.Count <= 0)
				return null;

			return requests_ch;
		}


		public static string CreateTransactionRequestLiveID(ref SqlDataReader reader, ref List<SyncEntities.SyncTransaction> listTr)
		{
			if (reader == null)
				return "";

			string json = "";
			List<string> trReq = new List<string>();

			while (reader.Read())
			{
				SyncEntities.SyncTransaction tr = new SyncEntities.SyncTransaction(
					(long)reader["Id"],
					reader["ChargeId"].ToString().Trim(),
					reader["DateOfService"].ToString().Trim(),
					reader["DateOfPosting"].ToString().Trim(),
					reader["TransactionId"].ToString().Trim(),
					reader["Billed"].ToString().Trim(),
					reader["Payment"].ToString().Trim(),
					reader["Adjustment"].ToString().Trim(),
					reader["Balance"].ToString().Trim()
				);

				trReq.Add(tr.GetNewHttpJson());
				listTr.Add(tr);
			}

			if (trReq.Count <= 0)
				return "";

			json = CreateListParams(trReq);
			return json;
		}




		public static string CreateTransactionRequest(ref SqlDataReader reader, ref List<SyncEntities.SyncTransaction> listTr)
		{
			if (reader == null)
				return "";

			string json = "";
			List<string> trReq = new List<string>();

			while (reader.Read())
			{
					SyncEntities.SyncTransaction tr = new SyncEntities.SyncTransaction(
						(long)reader["Id"],
						reader["ChargeId"].ToString().Trim(),
						reader["DateOfService"].ToString().Trim(),
						reader["DateOfPosting"].ToString().Trim(),
						reader["TransactionId"].ToString().Trim(),
						reader["Billed"].ToString().Trim(),
						reader["Payment"].ToString().Trim(),
						reader["Adjustment"].ToString().Trim(),
						reader["Balance"].ToString().Trim()
					);

					trReq.Add(tr.GetNewHttpJson());
					listTr.Add(tr);
			}

				if (trReq.Count <= 0)
				return "";

				json = CreateListParams(trReq);
				return json;
		}


		public static string CreateTransactionRequestZeroIds(ref SqlDataReader reader, ref List<SyncEntities.SyncTransaction> listTr)
		{
			if (reader == null)
				return "";

			string json = "";
			List<string> trReq = new List<string>();

			while (reader.Read())
			{
					SyncEntities.SyncTransaction tr = new SyncEntities.SyncTransaction(
						(long)reader["Id"],
						reader["ChargeId"].ToString().Trim(),
						reader["DateOfService"].ToString().Trim(),
						reader["DateOfPosting"].ToString().Trim(),
						reader["TransactionId"].ToString().Trim(),
						reader["Billed"].ToString().Trim(),
						reader["Payment"].ToString().Trim(),
						reader["Adjustment"].ToString().Trim(),
						reader["Balance"].ToString().Trim()
					);

					trReq.Add(tr.GetNewHttpJson());
					listTr.Add(tr);
			}

			if (trReq.Count <= 0)
				return "";

			json = CreateListParams(trReq);
			return json;
		}     
	}
}
