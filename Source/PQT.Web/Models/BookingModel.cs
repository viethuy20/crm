using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Helpers;
using PQT.Web.Infrastructure;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Models
{
    public class BookingModel
    {
        public Event Event { get; set; }
        public Booking Booking { get; set; }
        public List<int> SessionIds { get; set; }
        public IEnumerable<Company> Companies { get; set; }
        public HttpPostedFileBase AttachmentFile { get; set; }

        public int BookingID { get; set; }
        public string Message { get; set; }

        public BookingModel()
        {
            Companies = new HashSet<Company>();
            SessionIds = new List<int>();
        }
        public void Prepare(int leadId)
        {
            var leadRepo = DependencyHelper.GetService<ILeadService>();
            var eventRepo = DependencyHelper.GetService<IEventService>();
            var companyRepo = DependencyHelper.GetService<ICompanyRepository>();
            var settingRepo = DependencyHelper.GetService<ISettingRepository>();

            var lead = leadRepo.GetLead(leadId);
            Event = eventRepo.GetEvent(lead.EventID);
            Companies = companyRepo.GetAllCompanies();
            var discountSetting = settingRepo.GetSetting("Lead", "DiscountPercent");
            Booking = new Booking()
            {
                CompanyID = lead.CompanyID,
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

        public Booking Create()
        {
            var bookingService = DependencyHelper.GetService<IBookingService>();
            if (AttachmentFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Booking, AttachmentFile);
                Booking.Attachment = uploadPicture;
            }
            return bookingService.CreateBooking(Booking, SessionIds, CurrentUser.Identity.ID);
        }

        public bool Update()
        {
            var bookingService = DependencyHelper.GetService<IBookingService>();
            if (AttachmentFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Booking, AttachmentFile);
                Booking.Attachment = uploadPicture;
            }
            return bookingService.UpdateBooking(Booking, SessionIds, CurrentUser.Identity.ID);
        }
    }
}