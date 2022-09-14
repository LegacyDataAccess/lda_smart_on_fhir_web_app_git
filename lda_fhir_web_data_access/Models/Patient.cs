using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace lda_fhir_web_data_access
{

    public class patient
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public long id { get; set; }

        public int organization_id { get; set; }
        public string organization_name { get; set; }
        public int location_id { get; set; }
        public string location_code { get; set; }
        public string location_name { get; set; }
        public string pid { get; set; }

        public string medical_record_number { get; set; }

        public string name_last { get; set; }

        public string name_first { get; set; }

        public string name_middle { get; set; }

        public string name_suffix { get; set; }
        public string name_alias { get; set; }


        public string ssn { get; set; }

        public DateTime? date_of_birth { get; set; }

        public DateTime? date_of_death { get; set; }

        public string address_line_1 { get; set; }

        public string address_line_2 { get; set; }

        public string address_city { get; set; }

        public string address_state { get; set; }

        public string address_zip { get; set; }

        public int? gender_enum_id { get; set; }
        public string gender_code { get; set; }

        public string gender_display { get; set; }
        public string gender_system_uri { get; set; }

        public int? marital_status_code_id { get; set; }

        public string marital_status_code { get; set; }

        public string marital_status_display { get; set; }
        public string marital_status_system_uri { get; set; }


        public string driver_license { get; set; }

        public string phone_primary { get; set; }

        public string phone_secondary { get; set; }

        public string phone_work { get; set; }

        public string email { get; set; }
        public string race { get; set; }
        public string ethnicity { get; set; }
        public string language { get; set; }
        public string comment { get; set; }
        public bool? active { get; set; }
        public string legacylink_app_name { get; set; }
        public string legacylink_app_url { get; set; }

        [ForeignKey("resource_id")]
        public virtual List<patientextension> extensions { get; set; }
    }
    public class patientextension : ResourceExtensionBase
    {
    }

    public class patientsearchresult
    {
        public patient patient { get; set; }
        public string matchCriteria { get; set; }
        public int matchRank { get; set; }
        public bool matchFullName { get; set; }
        public bool matchFirstNamePartial { get; set; }
        public bool matchLastNamePartial { get; set; }
        public bool matchDob { get; set; }
    }

}