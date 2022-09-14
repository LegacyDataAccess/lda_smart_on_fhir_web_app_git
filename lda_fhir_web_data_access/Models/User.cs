using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lda_fhir_web_data_access
{

    public class user
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int id { get; set; }

        public string username { get; set; }
        public string password { get; set; }
        public string name { get; set; }

        public int organization_id { get; set; }

        [ForeignKey("user_id")]
        public virtual List<userlocation> locations { get; set; }


    }

    public class userlocation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int id { get; set; }
        public int user_id { get; set; }
        public int location_id { get; set; }

    }
}
