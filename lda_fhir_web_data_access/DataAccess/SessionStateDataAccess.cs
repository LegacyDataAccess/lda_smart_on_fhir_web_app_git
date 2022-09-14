using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lda_fhir_web_data_access
{
    public static class SessionStateDataAccess
    {
 
        public static string GetSessionValue(string key)
        {
            var db = new DBContext();
            var session = db.SessionState.Where(p => p.key == key).FirstOrDefault();
            if (session.expires < DateTime.Now)
            {
                return session.value;
            }
            else
            {
                DeleteSessionVariable(key);
                throw (new Exception("Session has expired"));
            }
        }

        public static void DeleteSessionVariable(string key)
        {
            var db = new DBContext();
            var session = db.SessionState.Where(p => p.key == key).FirstOrDefault();
            db.SessionState.Remove(session);
            db.SaveChanges();
            
        }

        public static void SaveSessionValue(string key,string value)
        {
            var db = new DBContext();
            var session = db.SessionState.Where(p => p.key == key).FirstOrDefault();
            if (session != null) {
                session.value = value;
            }
            else {
                var newSession = new session_state();
                newSession.key = key;
                newSession.value = value;
                newSession.created = DateTime.Now;
                newSession.expires = DateTime.Now;
                db.SessionState.Add(newSession);
            }

            db.SaveChanges();
        }
    }
}
