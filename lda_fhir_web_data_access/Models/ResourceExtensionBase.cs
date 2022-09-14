using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lda_fhir_web_data_access
{
    public abstract class ResourceExtensionBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public long id { get; set; }
        public long resource_id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }
}
