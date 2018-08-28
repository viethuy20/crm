using System;
using System.Collections.Generic;
using System.Linq;
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
        public InvoiceModel()
        {
        }
        public void Prepare(int id)
        {
            var bookingRepo = DependencyHelper.GetService<IBookingService>();
            Invoice = new Invoice();
            Booking = bookingRepo.GetBooking(id);
            if (Booking != null)
                Invoice.BookingID = Booking.ID;
        }
        public void PrepareEdit(int id)
        {
            var invoiceService = DependencyHelper.GetService<IInvoiceService>();
            Invoice = invoiceService.GetInvoice(id);
            if (Invoice != null)
                Booking = Invoice.Booking;
        }
        public Booking Create()
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
                booking.InvoiceNo = invoice.InvoiceNo;
                bookingService.UpdateBooking(booking);
            }
            return Booking;
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