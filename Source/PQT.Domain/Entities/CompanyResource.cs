using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class CompanyResource : Entity
    {
        public string Country { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Organisation { get; set; } //company name
        public string Role { get; set; }
        public string MobilePhone1 { get; set; }
        public string MobilePhone2 { get; set; }
        public string MobilePhone3 { get; set; }
        public string WorkEmail { get; set; }
        public string PersonalEmail{ get; set; }
        public int? CompanyID { get; set; }
        //[ForeignKey("CompanyID")]
        //public virtual Company Company { get; set; }
        public int? CountryID { get; set; }
    }
}
