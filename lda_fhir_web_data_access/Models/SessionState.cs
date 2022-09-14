using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lda_fhir_web_data_access
{

    public class session_state
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Int64 id { get; set; }

        public string key { get; set; }
        public string value { get; set; }
        public DateTime created { get; set; }
        public DateTime expires { get; set; }

    }

}
