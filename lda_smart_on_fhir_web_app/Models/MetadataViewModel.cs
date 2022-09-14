using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lda_smart_on_fhir_web_app.Models
{

    public class MetadataViewModel
    {
        public string MetadataUrl { get; set; }
        public string TokenUrl { get; set; }
        public string AuthorizationUrl { get; set; }        
    }
}
