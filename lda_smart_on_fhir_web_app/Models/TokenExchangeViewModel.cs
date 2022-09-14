using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lda_smart_on_fhir_web_app.Models
{
    public class TokenExchangeViewModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string id_token { get; set; }
        public string scope { get; set; }
        public string state { get; set; }
        public string patient{ get; set; }
        public string encounter { get; set; }
        public string location { get; set; }
        public string appointment { get; set; }
        public string fhirUser_Url { get; set; }
        public string ehr_username { get; set; }

    }
}
