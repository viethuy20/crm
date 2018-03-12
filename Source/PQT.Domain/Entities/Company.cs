using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Forms.VisualStyles;

namespace PQT.Domain.Entities
{
    public class Company : Entity
    {
        public int CountryID { get; set; }
        [ForeignKey("CountryID")]
        public virtual Country Country { get; set; }
        public string Name { get; set; }
        public string ProductOrService { get; set; }
        public string Sector { get; set; }
        public string Industry { get; set; }
        public string Ownership { get; set; }
        public string BusinessUnits { get; set; }
        public string Designation { get; set; }
        public string Email { get; set; }
        public string PersonalContact { get; set; }

        public string CountryName
        {
            get
            {
                if (Country!=null)
                {
                    return Country.Name;
                }

                return "";
            }
        }
    }
}
