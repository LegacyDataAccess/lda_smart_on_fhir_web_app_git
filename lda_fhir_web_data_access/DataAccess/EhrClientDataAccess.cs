using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lda_fhir_web_data_access
{
    public class EhrClientDataAccess
    {
        public EhrClientDataAccess()
        {

        }

        public ehrclient GetEhrClient(string smart_client_id)
        {
            var db = new DBContext();
            var client = db.EhrClient.Where(p => p.smart_client_id == smart_client_id).FirstOrDefault();

            client.ehr_system = db.EhrSystem.Where(p => p.id == client.ehr_system_id).FirstOrDefault();

            return client;
        }
    }
}
