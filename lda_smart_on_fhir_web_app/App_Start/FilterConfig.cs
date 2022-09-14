using System.Web;
using System.Web.Mvc;

namespace lda_smart_on_fhir_web_app
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
