using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IInvoiceService
    {
        string GetTempInvoiceNo();
        IEnumerable<Invoice> GetAllInvoices();
        Invoice GetInvoice(int id);
        Invoice GetInvoiceByBooking(int bookingId);
        Invoice CreateInvoice(Invoice info);
        bool UpdateInvoice(Invoice info);
        bool DeleteInvoice(int id, string message);
    }
}
