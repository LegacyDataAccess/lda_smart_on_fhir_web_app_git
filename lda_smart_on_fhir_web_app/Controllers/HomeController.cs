using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using lda_smart_on_fhir_web_app.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;
using lda_fhir_web_data_access;
using lda_smart_on_fhir_web_app.Mappers;
using System.Configuration;
using Hl7.Fhir.Serialization;
using System.IdentityModel.Tokens.Jwt;

namespace lda_smart_on_fhir_web_app.Controllers
{
    public class HomeController : Controller
    {
        string lda_fhir_server_url = ConfigurationManager.AppSettings["lda_fhir_server_url"];
        string token_url = ConfigurationManager.AppSettings["token_url"];
        string patient_url = ConfigurationManager.AppSettings["patient_url"];

        [Route("/home/launch/{id}")]
        public ActionResult Launch(string id)
        {

            ViewBag.Message = "Launch LDA Smart on Fhir";

            string serviceUri = Request.Params["iss"];
            string launchContextId = Request.Params["launch"];

            var ehrClientDA = new EhrClientDataAccess();

            var client = ehrClientDA.GetEhrClient(id);

            OAuth2ViewModel lvm = new OAuth2ViewModel();

            var metadata =  GetMetaData(client.smart_meta_uri);
            
            //Generate a unique ID for this session.
            lvm.State = Guid.NewGuid().ToString();
            lvm.ClientId = id;
            lvm.ClientSystem = client.ehr_system.ehr_system;
            lvm.Secret = client.smart_app_secret;
            lvm.Scopes = client.smart_scopes; 
            lvm.ServiceUri = serviceUri;
            lvm.LaunchContextId = launchContextId;
            lvm.RedirectUri = RedirctUriBuilder();
            lvm.LaunchUri = lvm.RedirectUri + "Launch";
            lvm.ConformanceUri = client.smart_meta_uri;
            lvm.MetadataUri = metadata.MetadataUrl;
            lvm.AuthUri = metadata.AuthorizationUrl;
            lvm.TokenUri = metadata.TokenUrl;
            lvm.AuthRedirectBody =
                "scope=" + HttpUtility.UrlEncode(lvm.Scopes) + "&" +
                "response_type=code&" +
                "redirect_uri=" + HttpUtility.UrlEncode(lvm.RedirectUri) + "&" +
                "client_id=" + HttpUtility.UrlEncode(lvm.ClientId) + "&" +
                "launch=" + HttpUtility.UrlEncode(lvm.LaunchContextId) + "&" +
                "state=" + lvm.State.ToString() + "&" +
                "aud=" + HttpUtility.UrlEncode(lvm.ServiceUri);
            lvm.AuthRedirectQueryString = lvm.AuthUri + "?" +
                "scope=" + HttpUtility.UrlEncode(lvm.Scopes) + "&" +
                "response_type=code&" +
                "redirect_uri=" + HttpUtility.UrlEncode(lvm.RedirectUri) + "&" +
                "client_id=" + HttpUtility.UrlEncode(lvm.ClientId) + "&" +
                "launch=" + HttpUtility.UrlEncode(lvm.LaunchContextId) + "&" +
                "state=" + lvm.State.ToString() + "&" +
                "aud=" + HttpUtility.UrlEncode(lvm.ServiceUri);

            var lvmJson = JsonConvert.SerializeObject(lvm);
            //Session[lvm.State] = lvwJson;
            SessionStateDataAccess.SaveSessionValue(lvm.State, lvmJson);

            return View(lvm);
        }

        public ActionResult Index()
        {
            string authCode = Request.Params["code"];
            string state = Request.Params["state"];
            if(state == null || authCode == null)
            {
                return View("InvalidLogin");
            }
            string session = SessionStateDataAccess.GetSessionValue(state);
            OAuth2ViewModel lvm = JsonConvert.DeserializeObject<OAuth2ViewModel>(session); // Convert.ToString(Session[state]));

            TokenExchangeViewModel tvm = DoTokenExchange(lvm.ClientId,authCode,lvm.TokenUri);

            lvm.EhrUserName = tvm.ehr_username;
            lvm.BearerToken = tvm.access_token;

            var lvmJson = JsonConvert.SerializeObject(lvm);
            //Session[lvm.State] = lvmJson;
            SessionStateDataAccess.SaveSessionValue(lvm.State, lvmJson);

            //if (tvm.fhirUser_Url != null) {
            //    var lname = GetEhrFhirUser(lvm,tvm);
            //}

            var patient = GetEhrPatient(lvm,tvm);
            PatientViewModel pvm = PatientMapper.Map(patient);
            //pvm = GetLdaPatients(lvm, pvm);
            pvm = GetLdaPatientsfromDB(lvm, pvm);
            pvm.session_state = state;

            ViewBag.ClientId = lvm.ClientId;
            ViewBag.ClientSystem = lvm.ClientSystem;
            ViewBag.EhrUserName = lvm.EhrUserName;

            return View(pvm);
        }

        public ActionResult Search()
        {
            string session_state = Request.Params["session_state"];
            string lastname = Request.Params["last_name"];
            string firstname = Request.Params["first_name"];
            string dob = Request.Params["dob"];

            string session = SessionStateDataAccess.GetSessionValue(session_state);
            OAuth2ViewModel lvm = JsonConvert.DeserializeObject<OAuth2ViewModel>(session); // Convert.ToString(Session[session_state]));
            var pvm = new PatientViewModel();
            pvm.last_name = lastname;
            pvm.first_name = firstname;
            pvm.dob = dob;

            //pvm = GetLdaPatients(lvm, pvm);
            pvm = GetLdaPatientsfromDB(lvm, pvm);
            pvm.session_state = session_state;

            ViewBag.ClientId = lvm.ClientId;
            ViewBag.ClientSystem = lvm.ClientSystem;
            ViewBag.EhrUserName = lvm.EhrUserName;

            return View("Index",pvm);
        }


        public ActionResult Error()
        {
            return View();
        }

        public PatientViewModel GetLdaPatientsfromDB(OAuth2ViewModel lvm, PatientViewModel pvm)
        {
            //Get user for clientid
            var userDA = new UserDataAccess();
            var user = userDA.GetUserbyClientId(lvm.ClientId);

            var patientDA = new PatientDataAccess(user.organization_id, user.locations.Select(l => l.location_id).ToList());
            //var patients = patientDA.SearchPatients(pvm.last_name, pvm.first_name, pvm.dob, null, null);
            var patients = patientDA.SearchPatientMatches(pvm.last_name, pvm.first_name, pvm.dob, null, null); 
            pvm.ldaPatients = new List<LdaPatient>();
            foreach (var p in patients)
            {
                var lda_pat = new LdaPatient();

                lda_pat.last_name = NullCheck.NullToEmptyString(p.patient.name_last);
                lda_pat.first_name = NullCheck.NullToEmptyString(p.patient.name_first);
                lda_pat.ll_app_name = NullCheck.NullToEmptyString(p.patient.legacylink_app_name);
                lda_pat.ll_app_url = NullCheck.NullToEmptyString(p.patient.legacylink_app_url);
                lda_pat.location = NullCheck.NullToEmptyString(p.patient.location_code);
                lda_pat.gender = NullCheck.NullToEmptyString(p.patient.gender_display);
                lda_pat.mrn = NullCheck.NullToEmptyString(p.patient.medical_record_number);

                lda_pat.ll_app_url = lda_pat.ll_app_url.Replace("{location_code}", p.patient.location_code).Replace("{pid}", p.patient.pid).Replace("{medical_record_number}", p.patient.medical_record_number);

                if(p.patient.date_of_birth != null)
                {
                    DateTime dob =(DateTime) p.patient.date_of_birth;
                    lda_pat.dob = dob.ToString("MM/dd/yyyy");
                }
                

                lda_pat.match_criteria = p.matchCriteria;
                lda_pat.match_rank = p.matchRank;

                pvm.ldaPatients.Add(lda_pat);
            }


            return pvm;

        }

        public PatientViewModel GetLdaPatients(OAuth2ViewModel lvm, PatientViewModel pvm)
        {
            //Get user for clientid
            var userDA = new UserDataAccess();
            var user = userDA.GetUserbyClientId(lvm.ClientId);

            //Get bearer token
            string bearerToken = "";
            using (HttpClient httpClient = new HttpClient())
            {
                FormUrlEncodedContent tokenRequestForm = new FormUrlEncodedContent(
                  new[]
                  {
                       new KeyValuePair<string,string>("username", user.username),
                       new KeyValuePair<string,string>("password", user.password),
                       new KeyValuePair<string,string>("grant_type", "password")
                  }
                  );
                string requestString = tokenRequestForm.ReadAsStringAsync().Result;
                StringContent requestContent = new StringContent(requestString);
                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                httpClient.DefaultRequestHeaders.Add("Accept", "text/json");

                var response = httpClient.PostAsync(lda_fhir_server_url + token_url, requestContent).Result;
                JObject jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                bearerToken = jsonResponse["access_token"].ToString();
            }

            //Get search results from lda_fhir_server
            //build search querystring
            string searchQueryString = lda_fhir_server_url + patient_url + "?";
            if (pvm.last_name != null) { 
                searchQueryString = searchQueryString + "lastname=" + HttpUtility.UrlEncode(pvm.last_name); }
            if (pvm.first_name != null) { 
                if (searchQueryString.Last() != '?') { searchQueryString = searchQueryString + "&"; } 
                searchQueryString = searchQueryString + "firstname=" + HttpUtility.UrlEncode(pvm.first_name); }
            if (pvm.dob != null) { 
                if (searchQueryString.Last() != '?') { searchQueryString = searchQueryString + "&"; } 
                searchQueryString = searchQueryString + "dob=" + HttpUtility.UrlEncode(pvm.dob); }


            var patientBundle = new Bundle();

            using (HttpClient httpClient = new HttpClient())
            {
                StringContent requestContent = new StringContent("");
                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                var response = httpClient.GetAsync(searchQueryString).Result;
                //patientBundle =  JsonConvert.DeserializeObject<Bundle>(response.Content.ReadAsStringAsync().Result);
                var jparser = new FhirJsonParser(new ParserSettings
                {
                    AcceptUnknownMembers = true,
                    AllowUnrecognizedEnums = true
                });
                //var jparser = new FhirJsonParser();
                patientBundle = jparser.Parse<Hl7.Fhir.Model.Bundle>(response.Content.ReadAsStringAsync().Result);
            }
            pvm.ldaPatients = new List<LdaPatient>();

            foreach (var e in patientBundle.Entry)
            {
                var p = (Patient)e.Resource;
                var lda_pat = new LdaPatient();

                var name = p.Name.Where(n => n.TypeName.ToLower() == "official").FirstOrDefault();
                if (name == null) { name = p.Name.FirstOrDefault(); }

                if (name.Given != null)
                {
                    var givenNames = name.Given.ToList();
                    if (givenNames[0] != null) { lda_pat.first_name = givenNames[0]; }
                }
                if (name.Family != null) { lda_pat.last_name = name.Family; }

                if (p.BirthDate != null) { lda_pat.dob = p.BirthDate; }
                if (p.Gender != null) { lda_pat.gender = p.Gender.ToString(); }

                if (p.Identifier.Where(x => x.Type.Text == "Medical Record Number").Any()) { lda_pat.mrn = NullCheck.NullToEmptyString(p.Identifier.Where(x => x.Type.Text == "Medical Record Number").FirstOrDefault().Value); }
                if (p.Extension.Where(x => x.Url == "locationname").Any()) { lda_pat.location = NullCheck.NullToEmptyString(p.Extension.Where(x => x.Url == "locationname").FirstOrDefault().Value); }
                if (p.Extension.Where(x => x.Url == "legacylink_app_name").Any()) { lda_pat.ll_app_name = NullCheck.NullToEmptyString(p.Extension.Where(x => x.Url == "legacylink_app_name").FirstOrDefault().Value); }
                if (p.Extension.Where(x => x.Url == "legacylink_app_url").Any()) { 
                    lda_pat.ll_app_url = NullCheck.NullToEmptyString(p.Extension.Where(x => x.Url == "legacylink_app_url").FirstOrDefault().Value);
                }

                pvm.ldaPatients.Add(lda_pat);
            }

            return pvm;

        }

        public Patient GetEhrPatient(OAuth2ViewModel lvm, TokenExchangeViewModel tvm)
        {
            var settings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
            };
            
            var messageHandler = new HttpClientEventHandler();

            messageHandler.OnBeforeRequest += (object sender, BeforeHttpRequestEventArgs e) =>
            {
                e.RawRequest.Headers
                .Add("Authorization", $"Bearer {lvm.BearerToken}");
            };
            var client = new FhirClient(lvm.ServiceUri, settings, messageHandler);

            var identity = ResourceIdentity.Build("Patient", tvm.patient);

            var patientResource = client.Read<Hl7.Fhir.Model.Patient>(identity);
            return patientResource;


        }

        public string GetEhrFhirUser(OAuth2ViewModel lvm, TokenExchangeViewModel tvm)
        {

            using (HttpClient httpClient = new HttpClient())
            {
                StringContent requestContent = new StringContent("");
                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tvm.access_token);

                var response = httpClient.GetAsync(tvm.fhirUser_Url).Result;

                return response.Content.ReadAsStringAsync().Result;
            }


        }

        public TokenExchangeViewModel DoTokenExchange(string clientId,string authCode,string tokenUri)
        {
            var tvm = new TokenExchangeViewModel();
            string tokenExchangeBody =
                "code=" + HttpUtility.UrlEncode(authCode) + "&" +
                "grant_type=authorization_code&" +
                "redirect_uri=" + HttpUtility.UrlEncode(RedirctUriBuilder()) + "&" +
                "client_id=" + HttpUtility.UrlEncode(clientId);

            using (HttpClient httpClient = new HttpClient())
            {
                StringContent requestContent = new StringContent(tokenExchangeBody);
                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var response = httpClient.PostAsync(tokenUri, requestContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    JObject jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    tvm.access_token = jsonResponse.Value<string>("access_token");
                    tvm.token_type = jsonResponse.Value<string>("token_type");
                    tvm.patient = jsonResponse.Value<string>("patient");
                    tvm.location = jsonResponse.Value<string>("location");
                    tvm.encounter = jsonResponse.Value<string>("encounter");
                    tvm.appointment = jsonResponse.Value<string>("appointment");
                    tvm.id_token = jsonResponse.Value<string>("id_token");
                    tvm.state = jsonResponse.Value<string>("state");

                    var handler = new JwtSecurityTokenHandler();
                    if (tvm.id_token != null)
                    {
                        var id_token = handler.ReadJwtToken(tvm.id_token);
                        tvm.ehr_username = id_token.Subject;
                        if (id_token.Payload.ContainsKey("fhirUser"))
                        {
                            object fhirUser_Url = "";
                            bool hasfhirUser = id_token.Payload.TryGetValue("fhirUser", out fhirUser_Url);
                            if (hasfhirUser) { tvm.fhirUser_Url = fhirUser_Url.ToString(); }
                        }
                    }
                    else if (tvm.access_token != null)
                    {
                        var access_token = handler.ReadJwtToken(tvm.access_token);
                        tvm.ehr_username = access_token.Subject;
                    }
                }
            }
            return tvm;
        }

        public MetadataViewModel GetMetaData(string metadataUri)
        {
            //// Build the token request payload.
            //// Not sure this is all necessary but I ain't going to change it.
            //FormUrlEncodedContent tokenRequestForm = new FormUrlEncodedContent(
            //  new[]
            //  {
            //      new KeyValuePair<string,string>("name", "LDA SMART on FHIR"),
            //      new KeyValuePair<string,string>("description", "LDA SMART on FHIR"),
            //      new KeyValuePair<string,string>("url", serviceUrl)
            //  }
            //);

            //I should comment on what this dictionary is for.
            Dictionary<string, string> myDictionary = new Dictionary<string, string>();

            //Make the call to the meta data location
            using (HttpClient httpClient = new HttpClient())
            {
                //string requestString = tokenRequestForm.ReadAsStringAsync().Result;
                //StringContent requestContent = new StringContent(requestString);
                StringContent requestContent = new StringContent("");
                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/fhir+json");

                string medatadataUrl = metadataUri;
                var response = httpClient.GetAsync(medatadataUrl + "?_format=json").Result;
                JObject jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                myDictionary = GetAuthenticationDictionaryFromJson(jsonResponse);
            }
            var metadata = new MetadataViewModel();
            metadata.MetadataUrl = metadataUri;
            //Put it in the dictionary.
            if (myDictionary.ContainsKey("token"))
            {
                metadata.TokenUrl = new Uri(myDictionary["token"]).ToString();
                //result = true;
            }
            //else
            //{
            //    //Should be throwing or logging an error here.
            //    metadata.TokenUrl = "Sorry, couldn't find Token URI";
            //    result = false;
            //}

            if (myDictionary.ContainsKey("authorize"))
            {
                metadata.AuthorizationUrl = new Uri(myDictionary["authorize"]).ToString();
                //result = true;
            }
            //else
            //{
            //    //Should be throwing or logging an error here.
            //    _authorizeUrl = "Sorry, couldn't find Authorize URI";
            //    result = false;
            //}

            return metadata;
        }


        Dictionary<string, string> GetAuthenticationDictionaryFromJson(JObject rss)
        {
            JArray rssValue = (JArray)rss["rest"];

            //Get the json token that contains the list of urls we care about.
            JToken myJToken = rssValue[0]["security"]["extension"][0]["extension"];

            Dictionary<string, string> myDictionary = new Dictionary<string, string>();

            //Shove that list into a dictionary object.
            //Truth is, this should be done directly from the JToken but I can't figure that out.
            foreach (JObject content in myJToken.Children<JObject>())
            {
                myDictionary.Add(content["url"].ToString(), content["valueUri"].ToString());
            }

            return myDictionary;
        }

        public string RedirctUriBuilder()
        {

            //Build out the redirectUri.

            //Get the name of the current action and controller.
            var thisControllerName = HttpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
            var thisActionName = HttpContext.Request.RequestContext.RouteData.Values["action"].ToString();

            var urlBuilder = new System.UriBuilder(Request.Url.AbsoluteUri)
            {
                //Note that I'm not including the action name here. 
                // If your destination is not the Index page then you'll need to make some changes here.
                Path = thisControllerName, //Url.Action(thisActionName, thisControllerName),
                Query = null,
            };

            Uri uri = urlBuilder.Uri;

            //We got's to add the slash at the end to keep the OAuth server happy.
            return urlBuilder.Uri.ToString() + @"/index"; ;

        }
    }
}