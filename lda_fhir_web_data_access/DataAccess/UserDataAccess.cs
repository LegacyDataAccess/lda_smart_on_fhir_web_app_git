using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lda_fhir_web_data_access
{
    public class UserDataAccess
    {
        public UserDataAccess()
        {

        }


        public user GetUser(int userId)
        {
            var db = new DBContext();
            var user = db.User.Where(p => p.id == userId).FirstOrDefault();

            user.locations = db.UserLocation.Where(p => p.user_id == user.id).ToList();

            return user;
        }

        public user GetUserbyClientId(string clientId)
        {
            var db = new DBContext();
            var ehrClient = db.EhrClient.Where(p => p.smart_client_id == clientId).FirstOrDefault();

            var user = GetUser(ehrClient.user_id);

            return user;
        }
    }
}
