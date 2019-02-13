using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Domain.Concrete
{
    public class EFInvoiceService : Repository, IInvoiceService
    {
        public EFInvoiceService(DbContext db) : base(db)
        {

        }

        public string GetTempInvoiceNo()
        {
            return string.Format("VN{0}", GetNextTempCounter("Invoice", 1578));
        }
        public IEnumerable<Invoice> GetAllInvoices()
        {
            return GetAll<Invoice>().AsEnumerable();
        }

        public IEnumerable<Invoice> GetAllInvoices(Func<Invoice, bool> predicate)
        {
            return GetAll(predicate).AsEnumerable();
        }
        public Invoice GetInvoice(int id)
        {
            return Get<Invoice>(id);
        }
        public Invoice GetInvoiceByInvoiceNumber(string no)
        {
            if (no == null)
            {
                return null;
            }
            return Get<Invoice>(m => m.InvoiceNo.ToUpper().Trim() == no.Trim().ToUpper());
        }
        public Invoice GetInvoiceByBooking(int bookingId)
        {
            return Get<Invoice>(m => m.BookingID == bookingId);
        }

        public Invoice CreateInvoice(Invoice info)
        {
            var tempNo = GetTempInvoiceNo();
            if (tempNo == info.InvoiceNo)
            {
                //save invoice number for next time
                info.InvoiceNo = string.Format("VN{0}", GetNextCounter("Invoice", 1578));
            }
            else
            {
                var counter = Get<Counter>(c => c.Name == "Invoice");
                if (counter != null)
                {
                    var resultString = Regex.Match(info.InvoiceNo, @"\d+").Value;
                    if (!string.IsNullOrEmpty(resultString))
                    {
                        counter.Value = Int32.Parse(resultString);
                        Update(counter);
                    }
                }
            }
            return Create(info);
        }

        public bool UpdateInvoice(Invoice info)
        {
            return Update(info);
        }
        public bool DeleteInvoice(int id, string message)
        {
            var exist = Get<Invoice>(id);
            exist.DeleteRemarks = message;
            exist.DeletedTime = DateTime.Now;
            exist.EntityStatus = EntityStatus.Deleted;
            return Update(exist);
        }
    }
}
