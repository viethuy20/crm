using System;
using System.Collections.Generic;
using System.Linq;
using System.util.collections;
using System.Web;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Utility;
using Quartz;

namespace PQT.Web.Models
{
    public class InvoiceModel
    {
        public Booking Booking { get; set; }
        public Invoice Invoice { get; set; }
        public IEnumerable<BankAccount> BankAccounts { get; set; }
        public InvoiceModel()
        {
            BankAccounts = new HashSet<BankAccount>();
        }
        public void Prepare(int id)
        {
            var bookingRepo = DependencyHelper.GetService<IBookingService>();
            var invoiceService = DependencyHelper.GetService<IInvoiceService>();
            var unitService = DependencyHelper.GetService<IUnitRepository>();
            Invoice = new Invoice { InvoiceNo = invoiceService.GetTempInvoiceNo(), CreatedTime = DateTime.Now };
            Booking = bookingRepo.GetBooking(id);
            if (Booking != null)
                Invoice.BookingID = Booking.ID;
            BankAccounts = unitService.GetAllBankAccounts();
        }
        public void PrepareEdit(int id)
        {
            var invoiceService = DependencyHelper.GetService<IInvoiceService>();
            var unitService = DependencyHelper.GetService<IUnitRepository>();
            Invoice = invoiceService.GetInvoice(id);
            if (Invoice != null)
            {
                Booking = Invoice.Booking;
            }
            BankAccounts = unitService.GetAllBankAccounts();
        }
        public Invoice Create()
        {
            var bookingService = DependencyHelper.GetService<IBookingService>();
            var invoiceService = DependencyHelper.GetService<IInvoiceService>();
            var booking = bookingService.GetBooking(Invoice.BookingID);
            if (booking == null)
            {
                throw new Exception("Booking not found");
            }
            var invoice = invoiceService.GetInvoiceByBooking(Invoice.BookingID);
            if (invoice != null)
            {
                throw new Exception("Booking has been invoiced");
            }
            Invoice = invoiceService.CreateInvoice(Invoice);
            if (Invoice != null)
            {
                booking.InvoiceNo = Invoice.InvoiceNo;
                bookingService.UpdateBooking(booking);
            }
            return Invoice;
        }

        public bool Update()
        {
            var invoiceService = DependencyHelper.GetService<IInvoiceService>();
            Invoice = invoiceService.GetInvoice(Invoice.ID);
            if (Invoice == null)
            {
                throw new ObjectAlreadyExistsException("Invoice does not exist");
            }
            return invoiceService.UpdateInvoice(Invoice);
        }

        public bool Delete(int id, string message)
        {
            return TransactionWrapper.Do(() =>
            {
                var invoiceService = DependencyHelper.GetService<IInvoiceService>();
                Invoice = invoiceService.GetInvoice(Invoice.ID);
                if (Invoice == null)
                {
                    throw new ObjectAlreadyExistsException("Invoice does not exist");
                }
                Invoice.DeleteRemarks = message;
                Invoice.DeletedTime = DateTime.Now;
                Invoice.EntityStatus = EntityStatus.Deleted;
                if (invoiceService.UpdateInvoice(Invoice))
                {
                    var bookingService = DependencyHelper.GetService<IBookingService>();
                    var booking = bookingService.GetBooking(Invoice.BookingID);
                    booking.InvoiceNo = null;
                    bookingService.UpdateBooking(booking);
                    return true;
                }
                return false;
            });
        }

    }
}