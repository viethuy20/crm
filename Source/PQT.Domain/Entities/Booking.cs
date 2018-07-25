using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;

namespace PQT.Domain.Entities
{
    public class Booking : Entity
    {
        public Booking()
        {
            Delegates = new HashSet<Delegate>();
            EventSessions = new HashSet<EventSession>();
            PaymentStatus = PaymentStatus.Initial;
        }
        public string Address { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }

        public decimal FeePerDelegate { get; set; }
        public double DiscountPercent { get; set; }// load default from setting
        public string Remarks { get; set; }
        public decimal TotalWrittenRevenue { get; set; }//Total Written Revenue
        public decimal TotalPaidRevenue { get; set; }

        public bool SameAsSender { get; set; }
        public string AuthoriserName { get; set; }// able to same as sender or boss of sender (noted: add check box `same as sender`)
        public string AuthoriserDestination { get; set; }
        public string AuthoriserTel { get; set; }
        public string AuthoriserMail { get; set; }

        public string SenderName { get; set; }//get info from lead
        public string SenderDestination { get; set; }
        public string SenderTel { get; set; }
        public string SenderMail { get; set; }

        public string Attachment { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string ProofOfPayment { get; set; }
        public string LetterOfUnderstaking { get; set; }

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

        [NotMapped]
        public bool ReloadTableLead { get; set; }
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
        public string StatusDisplay
        {
            get
            {
                if (BookingStatusRecord != null)
                {
                    return BookingStatusRecord.Status.DisplayName;
                }
                return "";
            }
        }

        public string PaymentStatusDisplay
        {
            get
            {
                if (PaymentStatus != null)
                {
                    return PaymentStatus.DisplayName;
                }
                return "";
            }
        }

        public string StatusUser
        {
            get
            {
                if (BookingStatusRecord != null && BookingStatusRecord.User != null)
                {
                    return BookingStatusRecord.User.DisplayName;
                }
                return "";
            }
        }
        public DateTime StatusUpdateTime
        {
            get
            {
                if (BookingStatusRecord != null)
                {
                    return BookingStatusRecord.UpdatedTime;
                }
                return default(DateTime);
            }
        }
        public string StatusUpdateTimeStr
        {
            get
            {
                if (BookingStatusRecord != null)
                {
                    return BookingStatusRecord.UpdatedTime.ToString("dd-MMM-yyyy HH:mm:ss");
                }
                return "";
            }
        }
        public string ClassStatus
        {
            get
            {
                return StringHelper.RemoveSpecialCharacters(StatusDisplay);
            }
        }

        public int StatusCode
        {
            get
            {
                if (BookingStatusRecord != null)
                {
                    return Convert.ToInt32(BookingStatusRecord.Status.Value);
                }
                return 0;
            }
        }
        public object Serializing()
        {
            return new
            {
                EventID,
                EventColor,
                StatusCode,
                Salesman,
                DateCreatedDisplay = StatusUpdateTimeStr,
                StatusDisplay,
                CompanyName,
                ReloadTableLead,
                ID
            };
        }
    }
}
