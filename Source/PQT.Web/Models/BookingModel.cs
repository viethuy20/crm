﻿using System;
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
        public HttpPostedFileBase ProofOfPaymentFile { get; set; }
        public HttpPostedFileBase LetterOfUnderstakingFile { get; set; }

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
                SenderName = lead.FullName,
                SenderDestination = lead.JobTitle,
                SenderMail = lead.WorkEmail,
                SenderTel = lead.MobilePhone1
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
                Booking.SenderName = lead.JobTitle;
                Booking.SenderDestination = "";
                Booking.SenderMail = lead.PersonalEmail;
                Booking.SenderTel = lead.MobilePhone1;
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
            if (ProofOfPaymentFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Booking, ProofOfPaymentFile);
                Booking.ProofOfPayment = uploadPicture;
            }
            if (LetterOfUnderstakingFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Booking, LetterOfUnderstakingFile);
                Booking.LetterOfUnderstaking = uploadPicture;
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
            if (!CurrentUser.HasRoleLevel(RoleLevel.ManagerLevel) && booking.BookingStatusRecord == BookingStatus.Approved)
            {
                throw new ObjectAlreadyExistsException("Cannot modify... Booking has been approved. Please contact to manager");
            }
            if (AttachmentFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Booking, AttachmentFile);
                Booking.Attachment = uploadPicture;
            }
            if (ProofOfPaymentFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Booking, ProofOfPaymentFile);
                Booking.ProofOfPayment = uploadPicture;
            }
            if (LetterOfUnderstakingFile != null)
            {
                string uploadPicture = FileUpload.Upload(FileUploadType.Booking, LetterOfUnderstakingFile);
                Booking.LetterOfUnderstaking = uploadPicture;
            }
            return bookingService.UpdateBooking(Booking, SessionIds, CurrentUser.Identity.ID);
        }

        public bool Approve(int id)
        {
            var bookingService = DependencyHelper.GetService<IBookingService>();
            var leadService = DependencyHelper.GetService<ILeadService>();
            var comService = DependencyHelper.GetService<ICompanyRepository>();
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
            var currentUserId = CurrentUser.Identity.ID;
            return TransactionWrapper.Do(() =>
            {
                Booking.Lead.LeadStatusRecord = new LeadStatusRecord(Booking.Lead.ID, LeadStatus.Booked, currentUserId);
                leadService.UpdateLead(Booking.Lead);
                if (bookingService.UpdateBooking(Booking, BookingStatus.Approved, currentUserId))
                {
                    var titleNotify = "Request for booking has been approved";
                    var membershipService = DependencyHelper.GetService<IMembershipService>();
                    var leadUser = Booking.Lead.User.TransferUserID > 0
                        ? membershipService.GetUser((int)Booking.Lead.User.TransferUserID)
                        : Booking.Lead.User;
                    BookingNotificator.NotifyUser(currentUserId, new List<User> { leadUser }, Booking.ID, titleNotify, true);
                    LeadNotificator.NotifyUpdateNCL(Booking.Lead.ID);
                    BookingNotificator.NotifyUpdateBooking(Booking.ID, false);
                    var com = comService.GetCompany(Booking.CompanyID);
                    if (com.Tier.ToString() != TierType.Tier1)//Update company => Tier = 1 after booked;
                    {
                        com.Tier = Convert.ToInt32(TierType.Tier1);
                        comService.UpdateCompany(com);
                    }
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

            var currentUserId = CurrentUser.Identity.ID;
            return TransactionWrapper.Do(() =>
            {
                Booking.Lead.LeadStatusRecord = new LeadStatusRecord(Booking.Lead.ID, LeadStatus.Reject, currentUserId);
                leadService.UpdateLead(Booking.Lead);
                if (bookingService.UpdateBooking(Booking, BookingStatus.Rejected, currentUserId, Message))
                {
                    var titleNotify = "Request for booking has been rejected";
                    var membershipService = DependencyHelper.GetService<IMembershipService>();
                    var leadUser = Booking.Lead.User.TransferUserID > 0
                        ? membershipService.GetUser((int)Booking.Lead.User.TransferUserID)
                        : Booking.Lead.User;
                    BookingNotificator.NotifyUser(currentUserId, new List<User> { leadUser }, Booking.ID, titleNotify, true);
                    BookingNotificator.NotifyUpdateBooking(Booking.ID);
                    return true;
                }
                return false;
            });
        }
    }
}