using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PQT.Domain.Entities
{
    public class Booking : Entity
    {
        public Booking()
        {
            Delegates = new HashSet<Delegate>();
            EventSessions = new HashSet<EventSession>();
        }
        public string Address { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }

        public decimal FeePerDelegate { get; set; }
        public double DiscountPercent { get; set; }// load default from setting
        public string Remarks { get; set; }
        public decimal RevenueAmount { get; set; }
        public decimal TotalPaidRevenue { get; set; }

        public string AuthoriserName { get; set; }// able to same as sender or boss of sender (noted: add check box `same as sender`)
        public string AuthoriserDestination { get; set; }
        public string AuthoriserTel { get; set; }
        public string AuthoriserMail { get; set; }

        public string SenderName { get; set; }//get info from lead
        public string SenderDestination { get; set; }
        public string SenderTel { get; set; }
        public string SenderMail { get; set; }

        public string Attachment { get; set; }

        public int? BookingStatusID { get; set; }
        [ForeignKey("BookingStatusID")]
        public virtual BookingStatusRecord BookingStatusRecord { get; set; }

        public int CompanyID { get; set; }
        [ForeignKey("CompanyID")]
        public virtual Company Company { get; set; }
        public int EventID { get; set; }
        [ForeignKey("EventID")]
        public virtual Event Event { get; set; }
        public int SalesmanID { get; set; }
        [ForeignKey("SalesmanID")]
        public virtual User Salesman { get; set; }
        public int LeadID { get; set; }
        [ForeignKey("LeadID")]
        public virtual Lead Lead { get; set; }

        public virtual ICollection<Delegate> Delegates { get; set; }
        public virtual ICollection<EventSession> EventSessions { get; set; }
    }
}
