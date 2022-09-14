using Hl7.Fhir.Model;
using lda_smart_on_fhir_web_app.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lda_smart_on_fhir_web_app.Mappers
{
    public class PatientMapper
    {
        public static PatientViewModel Map(Patient p)
        {
            PatientViewModel pvm = new PatientViewModel();

            if (p.BirthDate != null && !String.IsNullOrEmpty(p.BirthDate))
            {
                pvm.dob = p.BirthDate;
            }

            pvm.location = null;

            var name = p.Name.Where(n => n.TypeName.ToLower() == "official").FirstOrDefault();
            if (name == null) { name = p.Name.FirstOrDefault(); }

            if (name.TypeName != null) { pvm.name_type = name.TypeName; }
            if (name.Given != null)
            {
                var givenNames = name.Given.ToList();
                if (givenNames[0] != null) { pvm.first_name = givenNames[0]; }
            }
            if (name.Family != null) { pvm.last_name = name.Family; }


            //pname.suffix = name.Suffix.

            //pvm.patientNames = new List<PatientName>();
            //foreach (var name in p.Name)
            //{
            //    var pname = new PatientName();
            //    if (name.TypeName != null) { pname.name_type = name.TypeName; }
            //    if (name.Given != null)
            //    {
            //        var givenNames = name.Given.ToList();
            //        if (givenNames[0] != null) { pname.first_name = givenNames[0]; }
            //        if (givenNames[1] != null) { pname.middle_name_1 = givenNames[1]; }
            //        if (givenNames[2] != null) { pname.middle_name_2 = givenNames[2]; }
            //    }
            //    if (pname.last_name != null) { pname.last_name = name.Family; }
            //    //pname.suffix = name.Suffix.
            //}

            return pvm;
        }
    }
}
