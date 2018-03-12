using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web.Compilation;

namespace PQT.Domain.Entities
{
    public class Lead : Entity
    {
        public Lead()
        {
            PhoneCalls = new HashSet<PhoneCall>();
        }
        public string GeneralLine { get; set; }
        public string DirectLine { get; set; } //Direct Line/Mobile number
        public string ClientName { get; set; } //Client Name/Job Title
        public string Remark { get; set; }//Date/Highlights
        public DateTime CreatedDate { get; set; }
        public DateTime CallBackDate { get; set; }
        public int? LeadStatusID { get; set; }
        [ForeignKey("LeadStatusID")]
        public virtual LeadStatusRecord LeadStatusRecord { get; set; }
        public int ProjectEventID { get; set; }
        [ForeignKey("ProjectEventID")]
        public virtual ProjectEvent ProjectEvent { get; set; }
        public int CompanyID { get; set; }
        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        public virtual ICollection<PhoneCall> PhoneCalls { get; set; }
    }
}
