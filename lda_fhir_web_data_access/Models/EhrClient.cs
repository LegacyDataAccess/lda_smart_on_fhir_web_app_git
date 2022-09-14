using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lda_fhir_web_data_access
{
    public class ehrclient
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int organization_id { get; set; }
        public int ehr_system_id { get; set; }
        public int user_id { get; set; }
        public string smart_client_id { get; set; }
        public string smart_scopes { get; set; }
        public string smart_meta_uri { get; set; }
        public string smart_base_uri { get; set; }
        public string smart_app_secret { get; set; }

        [ForeignKey("ehr_system_id")]
        public virtual ehrsystem ehr_system { get; set; }

    }
    public class ehrsystem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int id { get; set; }
        public string ehr_system { get; set; }
        public string ehr_system_version { get; set; }
    }
}
