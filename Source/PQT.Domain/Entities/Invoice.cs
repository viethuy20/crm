using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class Invoice : Entity
    {
        public Invoice()
        {
            CreatedTime = DateTime.Today;
        }
        public string InvoiceNo { get; set; }
        public decimal AdminCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public SalaryCurrency Currency { get; set; }
        public int BookingID { get; set; }
        [ForeignKey("BookingID")]
        public virtual Booking Booking { get; set; }
        public int? BankAccountID { get; set; }
        [ForeignKey("BankAccountID")]
        public virtual BankAccount BankAccount { get; set; }
        public string ChequePayableTo { get; set; }
        public string Remarks { get; set; }
        public string DeleteRemarks { get; set; }
        [NotMapped]
        public int FontSize { get; set; }
        public DateTime InvoiceDate
        {
            get
            {
                return CreatedTime.Date;
            }
        }
        public string InvoiceDateStr
        {
            get
            {
                return CreatedTime.ToString("dd-MMM-yyyy");
            }
        }
    }
}
