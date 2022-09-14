using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lda_smart_on_fhir_web_app.Models
{
    public class AuthTargetURIs
    {
        public string AuthUri { get; set; }

        public string TokenUri { get; set; }
    }

    public class OAuth2ViewModel
    {
        public string ClientId { get; set; }
        public string ClientSystem { get; set; }
        public string EhrUserName { get; set; }
        public string Secret { get; set; }
        public string ServiceUri { get; set; }
        public string LaunchContextId { get; set; }
        public string Scopes { get; set; }
        public string State { get; set; }
        public string LaunchUri { get; set; }
        public string RedirectUri { get; set; }
        public string ConformanceUri { get; set; }
        public string MetadataUri { get; set; }
        public string AuthUri { get; set; }
        public string TokenUri { get; set; }
        public string SmartExtension { get; set; }
        public string AuthRedirectBody { get; set; }
        public string AuthRedirectQueryString { get; set; }

        public string BearerToken { get; set; }

    }
}
