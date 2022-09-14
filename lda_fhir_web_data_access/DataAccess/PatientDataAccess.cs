using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lda_fhir_web_data_access
{
    public class PatientDataAccess
    {
        int _organizationId;
        List<int> _locationIds;
        public PatientDataAccess(int organizationId, List<int> locationIds)
        {
            _organizationId = organizationId;
            _locationIds = locationIds;
        }

        public List<patient> SearchPatients(string nameLast, string nameFirst, string dob, string patientId, string mrn, int skip = -1, int take = -1, string locationId = null)
        {
            List<int> locationsList = new List<int>();
            if (locationId != null)
            {
                if (_locationIds.Contains(int.Parse(locationId))) { locationsList.Add(int.Parse(locationId)); }
            }
            else { locationsList.AddRange(_locationIds); }

            var db = new DBContext();
            var pats = db.Patient.Where(p => p.organization_id == _organizationId && locationsList.Contains(p.location_id));
            if (!String.IsNullOrWhiteSpace(nameLast)) { pats = pats.Where(p => p.name_last == nameLast); }
            if (!String.IsNullOrWhiteSpace(nameFirst)) { pats = pats.Where(p => p.name_first == nameFirst); }
            if (!String.IsNullOrWhiteSpace(dob)) { var birthdate = DateTime.Parse(dob); pats = pats.Where(p => p.date_of_birth == birthdate); }
            if (!String.IsNullOrWhiteSpace(patientId)) { pats = pats.Where(p => p.pid == patientId); }
            if (!String.IsNullOrWhiteSpace(mrn)) { pats = pats.Where(p => p.medical_record_number == mrn); }

            if (skip >= 0 && take <= 0) { pats = pats.Skip(skip).Take(take); }
            return pats.ToList();
        }

        public List<patientsearchresult> SearchPatientMatches(string nameLast, string nameFirst, string dob, string patientId, string mrn, string locationId = null)
        {
            List<patient> pats = new List<patient>();
            List<patientsearchresult> patResults = new List<patientsearchresult>();

            List<int> locationsList = new List<int>();
            if (locationId != null)
            {
                if (_locationIds.Contains(int.Parse(locationId))) { locationsList.Add(int.Parse(locationId)); }
            }
            else { locationsList.AddRange(_locationIds); }

            var patsMatchFull = PatientMatchesFullName( nameLast,  nameFirst, locationsList);
            var patsMatchFirstNamePartial = PatientMatchesPartialFirstName(nameLast, nameFirst,locationsList);
            var patsMatchLastNamePartial = PatientMatchesPartialLastName(nameLast, nameFirst, locationsList);
            var patsMatchDob = PatientMatchesDob(dob, locationsList);

            pats.AddRange(patsMatchFull);
            pats.AddRange(patsMatchFirstNamePartial);
            pats.AddRange(patsMatchLastNamePartial);
            pats.AddRange(patsMatchDob);

            pats = pats.GroupBy(p => p.id).Select(p => p.FirstOrDefault()).ToList();

            foreach(var pat in pats.ToList())
            {
                patientsearchresult patResult = new patientsearchresult();
                patResult.patient = pat;
                if (patsMatchFull.Any(p => p.id == pat.id)) { patResult.matchFullName = true; }
                if (patsMatchFirstNamePartial.Any(p => p.id == pat.id)) { patResult.matchFirstNamePartial = true; }
                if (patsMatchLastNamePartial.Any(p => p.id == pat.id)) { patResult.matchLastNamePartial = true; }
                if (patsMatchDob.Any(p => p.id == pat.id)) { patResult.matchDob = true; }

                if (patResult.matchFullName && patResult.matchDob) { patResult.matchRank = 1; patResult.matchCriteria = "Full Name and DOB"; }
                else if (patResult.matchFullName) { patResult.matchRank = 2; patResult.matchCriteria = "Full Name"; }
                else if (patResult.matchFirstNamePartial && patResult.matchDob) { patResult.matchRank = 3; patResult.matchCriteria = "Partial First Name and DOB"; }
                else if (patResult.matchFirstNamePartial) { patResult.matchRank = 4; patResult.matchCriteria = "Partial First Name"; }
                else if (patResult.matchLastNamePartial && patResult.matchDob) { patResult.matchRank = 5; patResult.matchCriteria = "Partial Last Name and DOB"; }
                else if (patResult.matchLastNamePartial) { patResult.matchRank = 6; patResult.matchCriteria = "Partial Last Name"; }
                
                if (patResult.matchRank >= 1)
                {
                    patResults.Add(patResult);
                }
            }


            return patResults;
        }

        public List<patient> PatientMatchesFullName(string nameLast, string nameFirst, List<int> locationsList)
        {
            var db = new DBContext();
            var pats = db.Patient.Where(p => p.organization_id == _organizationId && locationsList.Contains(p.location_id));
            if (!String.IsNullOrWhiteSpace(nameLast)) { pats = pats.Where(p => p.name_last == nameLast); }
            if (!String.IsNullOrWhiteSpace(nameFirst)) { pats = pats.Where(p => p.name_first == nameFirst); }
               
            return pats.ToList();
        }

        public List<patient> PatientMatchesPartialFirstName(string nameLast, string nameFirst,  List<int> locationsList)
        {
            var nameLength = Math.Min(3, nameFirst.Length);
            var db = new DBContext();
            var pats = db.Patient.Where(p => p.organization_id == _organizationId && locationsList.Contains(p.location_id));
            if (!String.IsNullOrWhiteSpace(nameLast)) { pats = pats.Where(p => p.name_last == nameLast); }
            if (!String.IsNullOrWhiteSpace(nameFirst)) { pats = pats.Where(p => p.name_first.StartsWith(nameFirst.Substring(0, nameLength))); }

            return pats.ToList();
        }

        public List<patient> PatientMatchesPartialLastName(string nameLast, string nameFirst, List<int> locationsList)
        {
            var nameLength = Math.Min(3, nameLast.Length);
            var db = new DBContext();
            var pats = db.Patient.Where(p => p.organization_id == _organizationId && locationsList.Contains(p.location_id));
            if (!String.IsNullOrWhiteSpace(nameLast)) { pats = pats.Where(p => p.name_last.StartsWith(nameLast.Substring(0,nameLength))); }
            if (!String.IsNullOrWhiteSpace(nameFirst)) { pats = pats.Where(p => p.name_first == nameFirst); }

            return pats.ToList();
        }
        public List<patient> PatientMatchesDob(string dob, List<int> locationsList)
        {
            if (!String.IsNullOrWhiteSpace(dob))
            {
                var db = new DBContext();
                var pats = db.Patient.Where(p => p.organization_id == _organizationId && locationsList.Contains(p.location_id));
                var birthdate = DateTime.Parse(dob); pats = pats.Where(p => p.date_of_birth == birthdate);
                return pats.ToList();
            }

            return new List<patient>();   
        }



        //public List<patient> SearchPatientsbyClientPatientId(string locationId,string master_patient_id, int skip = -1, int take = -1)
        //{
        //    List<int> locationsList = new List<int>();
        //    if (locationId != null)
        //    {
        //        if (_locationIds.Contains(int.Parse(locationId))) { locationsList.Add(int.Parse(locationId)); }
        //    }
        //    else { locationsList.AddRange(_locationIds); }

        //    var db = new DBContext();
        //    var pats = db.Patient.Where(p => p.organization_id == _organizationId && locationsList.Contains(p.location_id));
        //    if (!String.IsNullOrWhiteSpace(nameLast)) { pats = pats.Where(p => p.name_last == nameLast); }
        //    if (!String.IsNullOrWhiteSpace(nameFirst)) { pats = pats.Where(p => p.name_first == nameFirst); }
        //    if (!String.IsNullOrWhiteSpace(dob)) { var birthdate = DateTime.Parse(dob); pats = pats.Where(p => p.date_of_birth == birthdate); }
        //    if (!String.IsNullOrWhiteSpace(patientId)) { pats = pats.Where(p => p.pid == patientId); }
        //    if (!String.IsNullOrWhiteSpace(mrn)) { pats = pats.Where(p => p.medical_record_number == mrn); }

        //    if (skip >= 0 && take <= 0) { pats = pats.Skip(skip).Take(take); }
        //    return pats.ToList();
        //}

    }
}
