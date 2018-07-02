using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Forms.VisualStyles;

namespace PQT.Domain.Entities
{
    public class Company : Entity
    {
        public Company()
        {
            ManagerUsers = new HashSet<User>();
        }
        public int? CountryID { get; set; }
        [ForeignKey("CountryID")]
        public virtual Country Country { get; set; }
        public string CompanyName { get; set; }
        public string ProductOrService { get; set; }
        public string Sector { get; set; }
        public string Industry { get; set; }
        public string Ownership { get; set; }
        public string BusinessUnits { get; set; }
        public string Designation { get; set; }
        public string Email { get; set; }
        public string PersonalContact { get; set; }
        public decimal BudgetPerHead { get; set; }
        public int FinancialYear { get; set; }
        public int Tier { get; set; }//tier: 1-red 2-blue -3-black

        public string CountryName
        {
            get
            {
                if (Country != null)
                {
                    return Country.Name;
                }

                return "";
            }
        }
        public string CountryCode
        {
            get
            {
                if (Country != null)
                {
                    return Country.Code;
                }

                return "";
            }
        }

        public string Address { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public virtual ICollection<User> ManagerUsers { get; set; }
    }
}
