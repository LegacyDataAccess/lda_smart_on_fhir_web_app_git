using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lda_smart_on_fhir_web_app.Models
{
    public class PatientViewModel
    {
        public string session_state { get; set; }
        public string dob { get; set; }
        public string location { get; set; }
        public string name_type { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        //public List<PatientName> patientNames { get; set; }

        public List<LdaPatient> ldaPatients { get; set; }
    }

    public class LdaPatient
    {
        public string name_type { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public string mrn { get; set; }
        public string location { get; set; }
        public string ll_app_name { get; set; }
        public string ll_app_url { get; set; }

        public int match_rank { get; set; }
        public string match_criteria { get; set; }
    }

    public class PatientName
    {

        public string name_type { get; set; }
        public string first_name { get; set; }
        public string middle_name_1 { get; set; }
        public string middle_name_2 { get; set; }
        public string last_name { get; set; }
        public string suffix { get; set; }


    }





}
