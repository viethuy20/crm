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
        public string InvoiceNo { get; set; }
        public decimal AdminCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public SalaryCurrency Currency { get; set; }
        public int BookingID { get; set; }
        [ForeignKey("BookingID")]
        public virtual Booking Booking { get; set; }
        public string Remarks { get; set; }
        public string DeleteRemarks { get; set; }
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
