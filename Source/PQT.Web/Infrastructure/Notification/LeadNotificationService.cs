using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NS.Mail;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Hubs;
using PQT.Web.Infrastructure.Helpers;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Infrastructure.Notification
{
    public class LeadNotificationService : AbstractNotificationService<Lead>
    {
        #region Override

        public override void NotifyAll(Lead lead)
        {
        }

        public override void NotifyUser(IEnumerable<User> users, Lead lead)
        {
            if (lead == null)
                return;
            SendEmailNotify(lead, users.Select(m => m.Email).ToArray());
        }

        public override void NotifyRole(IEnumerable<Role> roles, Lead lead)
        {
            if (lead == null)
                return;
        }

        #endregion

        private void SendEmailNotify(Lead lead, string[] users)
        {
            if (lead == null) return;
            if (lead.LeadStatusRecord == LeadStatus.RequestNCL || lead.LeadStatusRecord == LeadStatus.RequestLOI)
            {
                SendEmail_RequestNCL(lead, users);
            }
        }

        private void SendEmail_RequestNCL(Lead lead, string[] users)
        {
            string subject = lead.StatusDisplay + " #" + lead.ID;
            EmailHelper.SendEmail(users, subject, "Lead", "RequestNoCallList", lead);
        }

    }


    public class BookingNotificationService : AbstractNotificationService<Booking>
    {
        #region Override

        public override void NotifyAll(Booking lead)
        {
        }

        public override void NotifyUser(IEnumerable<User> users, Booking lead)
        {
            if (lead == null)
                return;
            SendEmailNotify(lead, users.Select(m => m.Email).ToArray());
        }

        public override void NotifyRole(IEnumerable<Role> roles, Booking lead)
        {
            if (lead == null)
                return;
        }

        #endregion

        private void SendEmailNotify(Booking booking, string[] users)
        {
            if (booking == null) return;
            if (booking.BookingStatusRecord == BookingStatus.Initial)
            {
                SendEmail_RequestBooking(booking, users);
            }
            else if (booking.BookingStatusRecord == BookingStatus.Approved)
            {
                SendEmail_ApprovedBooking(booking, users);
            }
            else if (booking.BookingStatusRecord == BookingStatus.Rejected)
            {
                SendEmail_RejectedBooking(booking, users);
            }
        }

        private void SendEmail_RequestBooking(Booking booking, string[] users)
        {
            string subject = "Request booking #" + booking.ID;
            EmailHelper.SendEmail(users, subject, "Lead", "RequestBooking", booking);
        }

        private void SendEmail_ApprovedBooking(Booking booking, string[] users)
        {
            string subject = "Approved booking #" + booking.ID;
            EmailHelper.SendEmail(users, subject, "Lead", "AlertApprovedBooking", booking);
        }

        private void SendEmail_RejectedBooking(Booking booking, string[] users)
        {
            string subject = "Rejected booking #" + booking.ID;
            EmailHelper.SendEmail(users, subject, "Lead", "AlertRejectedBooking", booking);
        }

    }
}