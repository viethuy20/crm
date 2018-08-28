using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

        public Invoice GetInvoice(int id)
        {
            return Get<Invoice>(id);
        }
        public Invoice GetInvoiceByBooking(int bookingId)
        {
            return Get<Invoice>(m => m.BookingID == bookingId);
        }

        public Invoice CreateInvoice(Invoice info)
        {
            info.InvoiceNo = string.Format("VN{0}", GetNextCounter("Invoice", 1578));
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
