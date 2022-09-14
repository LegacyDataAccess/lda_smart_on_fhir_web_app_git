using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lda_fhir_web_data_access
{
    public static class NullCheck
    {
        public static string NullToEmptyString(Object input)
        {
            string output = "";

            if (input != null) { return input.ToString(); }
            return output;
        }

        public static string NullToLiteralNull(Object input)
        {
            string output = "NULL";

            if (input != null) { return input.ToString(); }
            return output;
        }
    }
}
