using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class ReportCall : Entity
    {
        public string DirectLine { get; set; } //Direct Line/Mobile number
        public string JobTitle { get; set; } //Client Name/Job Title
        public string LineExtension { get; set; } 
        public string Remark { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobilePhone1 { get; set; }
        public string MobilePhone2 { get; set; }
        public string MobilePhone3 { get; set; }
        public string WorkEmail { get; set; }
        public string WorkEmail1 { get; set; }
        public string PersonalEmail { get; set; }
        public string CompanyName { get; set; }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public string CreatedTimeDisplay
        {
            get
            {
                return CreatedTime.ToString("dd/MM/yyyy HH:mm");
            }
        }
        public string SalesmanName
        {
            get
            {
                if (User != null)
                {
                    return User.DisplayName;
                }

                return "";
            }
        }
    }
}
