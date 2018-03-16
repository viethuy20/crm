using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public int? LeadStatusID { get; set; }
        [ForeignKey("LeadStatusID")]
        public virtual LeadStatusRecord LeadStatusRecord { get; set; }
        public int EventID { get; set; }
        [ForeignKey("EventID")]
        public virtual Event Event { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please choose company")]
        public int CompanyID { get; set; }
        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        public virtual ICollection<PhoneCall> PhoneCalls { get; set; }

        public string CompanyName
        {
            get
            {
                if (Company != null)
                {
                    return Company.CompanyName;
                }

                return "";
            }
        }
        public DateTime CallBackDate
        {
            get
            {
                var call = PhoneCalls.LastOrDefault();
                if (call != null && call.CallBackDate != null)
                {
                    return Convert.ToDateTime(call.CallBackDate);
                }
                return default(DateTime);
            }
        }

        public string StatusDisplay
        {
            get
            {
                if (LeadStatusRecord != null)
                {
                    return LeadStatusRecord.Status.DisplayName;
                }
                return "";
            }
        }
        public string DateCreatedDisplay
        {
            get
            {
                if (LeadStatusRecord != null)
                {
                    return LeadStatusRecord.UpdatedTime.ToString("dd/MM/yyyy");
                }
                return CreatedTime.ToString("dd/MM/yyyy");
            }
        }

        public int StatusCode
        {
            get
            {
                if (LeadStatusRecord != null)
                {
                    return Convert.ToInt32(LeadStatusRecord.Status.Value);
                }
                return 0;
            }
        }
        public string EventColor
        {
            get
            {
                if (Event != null)
                {
                    return Event.BackgroundColor;
                }

                return "#0aa89e";
            }
        }
    }
}
