using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Notification;
using PQT.Web.Infrastructure.Utility;
using Quartz;

namespace PQT.Web.Models
{
    public class BookingModel
    {
        public Event Event { get; set; }
        public Booking Booking { get; set; }
        public List<int> SessionIds { get; set; }
        //public IEnumerable<Company> Companies { get; set; }
        public HttpPostedFileBase AttachmentFile { get; set; }

        public int EventID { get; set; }
        public int BookingID { get; set; }
        public string Message { get; set; }
        public BookingModel()
        {
            //Companies = new HashSet<Company>();
            SessionIds = new List<int>();
        }
        public void Prepare(int leadId)
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var eventRepo = DependencyHelper.GetService<IEventService>();
            //var companyRepo = DependencyHelper.GetService<ICompanyRepository>();
            var settingRepo = DependencyHelper.GetService<ISettingRepository>();

            var lead = leadRepo.GetLead(leadId);
            Event = eventRepo.GetEvent(lead.EventID);
            //Companies = companyRepo.GetAllCompanies();
            var discountSetting = settingRepo.GetSetting("Lead", "DiscountPercent");
            Booking = new Booking()
            {
                CompanyID = lead.CompanyID,
                Company = lead.Company,
                Address = lead.Company.Address,
                Tel = lead.Company.Tel,
                Fax = lead.Company.Fax,
                EventID = Event.ID,
                LeadID = leadId,
                SalesmanID = CurrentUser.Identity.ID,
                DiscountPercent = Convert.ToInt32(discountSetting.Value),
                SenderName = lead.ClientName,
                SenderDestination = "",
                SenderMail = lead.PersonalEmailAddress,
                SenderTel = lead.MobilePhone
            };
        }

        public void PrepareEdit(int bookingId)
        {
            var bookingService = DependencyHelper.GetService<IBookingService>();
            var leadService = DependencyHelper.GetService<ILeadService>();
            var settingRepo = DependencyHelper.GetService<ISettingRepository>();

            var booking = bookingService.GetBooking(bookingId);
            if (booking != null)
            {
                var lead = leadService.GetLead(booking.LeadID);
                Event = booking.Event;
                var discountSetting = settingRepo.GetSetting("Lead", "DiscountPercent");
                Booking = booking;
                Booking.CompanyID = lead.CompanyID;
                Booking.Company = lead.Company;
                Booking.DiscountPercent = Convert.ToInt32(discountSetting.Value);
                Booking.SenderName = lead.ClientName;
                Booking.SenderDestination = "";
                Booking.SenderMail = lead.PersonalEmailAddress;
                Booking.SenderTel = lead.MobilePhone;
                SessionIds = booking.EventSessions.Select(m => m.ID).ToList();
            }
        }

        public Booking Create()
        {
            var bookingService = DependencyHelper.GetService<IBookingService>();
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var lead = leadRepo.GetLead(Booking.LeadID);
            if (lead.LeadStatusRecord == LeadStatus.RequestBook)
            {
                throw new ObjectAlreadyExistsException("Cannot create request... This call has been requested for book");
            }
            if (lead.LeadStatusRecord == LeadStatus.Booked)
            {
                throw new ObjectAlreadyExistsException("Cannot create request... This call has been booked");
            }
            if (AttachmentFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Booking, AttachmentFile);
                Booking.Attachment = uploadPicture;
            }
            Booking = bookingService.CreateBooking(Booking, SessionIds, CurrentUser.Identity.ID);
            return Booking;
        }

        public bool Update()
        {
            var bookingService = DependencyHelper.GetService<IBookingService>();
            var booking = bookingService.GetBooking(Booking.ID);
            if (booking == null)
            {
                throw new ObjectAlreadyExistsException("Booking does not exist");
            }
            //if (!CurrentUser.HasRoleLevel(RoleLevel.ManagerLevel) && booking.BookingStatusRecord == BookingStatus.Approved)
            //{
            //    throw new ObjectAlreadyExistsException("Cannot modify... Booking has been approved. Please contact to manager");
            //}
            if (AttachmentFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Booking, AttachmentFile);
                Booking.Attachment = uploadPicture;
            }
            return bookingService.UpdateBooking(Booking, SessionIds, CurrentUser.Identity.ID);
        }

        public bool Approve(int id)
        {
            var bookingService = DependencyHelper.GetService<IBookingService>();
            var leadService = DependencyHelper.GetService<ILeadService>();
            Booking = bookingService.GetBooking(id);
            if (Booking == null)
            {
                throw new ObjectAlreadyExistsException("Booking not found");
            }
            if (Booking.BookingStatusRecord == BookingStatus.Approved)
            {
                throw new ObjectAlreadyExistsException("This booking has been approved");
            }
            if (Booking.BookingStatusRecord == BookingStatus.Rejected)
            {
                throw new ObjectAlreadyExistsException("This booking has been rejected");
            }
            if (Booking.Lead.LeadStatusRecord == LeadStatus.Booked)
            {
                throw new ObjectAlreadyExistsException("This call has been booked");
            }
            return TransactionWrapper.Do(() =>
            {
                Booking.Lead.LeadStatusRecord = new LeadStatusRecord(Booking.Lead.ID, LeadStatus.Booked, CurrentUser.Identity.ID);
                leadService.UpdateLead(Booking.Lead);
                if (bookingService.UpdateBooking(Booking, BookingStatus.Approved, CurrentUser.Identity.ID))
                {
                    var titleNotify = "Request for booking has been approved";
                    BookingNotificator.NotifyUser(new List<User> { Booking.Lead.User }, Booking.ID, titleNotify, true);
                    LeadNotificator.NotifyUpdateNCL(Booking.Lead.ID);
                    BookingNotificator.NotifyUpdateBooking(Booking.ID, false);
                    return true;
                }
                return false;
            });
        }

        public bool Reject()
        {
            var bookingService = DependencyHelper.GetService<IBookingService>();
            var leadService = DependencyHelper.GetService<ILeadService>();
            Booking = bookingService.GetBooking(BookingID);
            if (Booking == null)
            {
                throw new ObjectAlreadyExistsException("Booking not found");
            }
            if (Booking.BookingStatusRecord == BookingStatus.Approved)
            {
                throw new ObjectAlreadyExistsException("This booking has been approved");
            }
            if (Booking.BookingStatusRecord == BookingStatus.Rejected)
            {
                throw new ObjectAlreadyExistsException("This booking has been rejected");
            }

            return TransactionWrapper.Do(() =>
            {
                Booking.Lead.LeadStatusRecord = new LeadStatusRecord(Booking.Lead.ID, LeadStatus.Reject, CurrentUser.Identity.ID);
                leadService.UpdateLead(Booking.Lead);
                if (bookingService.UpdateBooking(Booking, BookingStatus.Rejected, CurrentUser.Identity.ID, Message))
                {
                    var titleNotify = "Request for booking has been rejected";
                    BookingNotificator.NotifyUser(new List<User> { Booking.Lead.User }, Booking.ID, titleNotify, true);
                    BookingNotificator.NotifyUpdateBooking(Booking.ID);
                    return true;
                }
                return false;
            });
        }
    }
}