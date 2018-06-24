using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Web.Hubs;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Infrastructure.Notification
{
    public class LeadNotificator
    {
        private static INotificationService<Lead> NotificationService
        {
            get { return DependencyResolver.Current.GetService<INotificationService<Lead>>(); }
        }
        private static ILeadService LeadRepository
        {
            get { return DependencyResolver.Current.GetService<ILeadService>(); }
        }

        private static IMembershipService MemberService
        {
            get { return DependencyResolver.Current.GetService<IMembershipService>(); }
        }

        public static void NotifyUpdateNCL(Lead lead)
        {
            if (lead == null)
                return;
            NotificationHub.Notify(lead);
        }

        //public static void NotifyAll(Lead lead, bool email = false)
        //{
        //    var users = MemberService.GetUsers();
        //    if (lead == null || !users.Any())
        //        return;
        //    foreach (var user in users)
        //    {
        //        if (CurrentUser.Identity.ID == user.ID)
        //        {
        //            continue;
        //        }
        //        var notify = new UserNotification
        //        {
        //            UserID = user.ID,
        //            EntryId = lead.ID,
        //            NotifyType = NotifyType.Lead,
        //            Title = "Called by " + lead.User.DisplayName,
        //            Description = lead.CompanyName,
        //            HighlightColor = lead.EventColor
        //        };
        //        notify = MemberService.CreateUserNotification(notify);
        //        user.NotifyNumber++;
        //        MemberService.UpdateUser(user);
        //        NotificationHub.Notify(notify);
        //    }
        //    if (email)
        //    {
        //        NotificationService.NotifyAll(lead);
        //    }
        //}

        public static void NotifyUser(IEnumerable<User> users, Lead lead, string title = null, bool email = false)
        {
            if (lead == null)
                return;
            foreach (var user in users)
            {
                if (CurrentUser.Identity.ID == user.ID)
                {
                    continue;
                }
                var notify = new UserNotification
                {
                    UserID = user.ID,
                    EntryId = lead.ID,
                    EventId = lead.EventID,
                    NotifyType = NotifyType.Lead,
                    Title = lead.StatusDisplay,
                    EventCode = lead.Event.EventCode,
                    Description = lead.CompanyName,
                    HighlightColor = lead.EventColor
                };
                if (!string.IsNullOrEmpty(title))
                {
                    notify.Title = title;
                }
                notify = MemberService.CreateUserNotification(notify);
                user.NotifyNumber++;
                MemberService.UpdateUser(user);
                NotificationHub.NotifyUser(user, notify);
            }

            //if (email)
            //{
            //    NotificationService.NotifyUser(users, lead);
            //}
        }
    }


    public class BookingNotificator
    {
        private static INotificationService<Booking> NotificationService
        {
            get { return DependencyResolver.Current.GetService<INotificationService<Booking>>(); }
        }

        private static IMembershipService MemberService
        {
            get { return DependencyResolver.Current.GetService<IMembershipService>(); }
        }
        private static IBookingService BookingService
        {
            get { return DependencyResolver.Current.GetService<IBookingService>(); }
        }

        public static void NotifyEmailForUser(IEnumerable<User> users,Booking booking)
        {
            if (booking == null)
                return;
            NotificationService.NotifyUser(users, booking);
        }

        public static void NotifyUser(IEnumerable<User> users, int bookingId, string title = null, bool email = false)
        {
            var booking = BookingService.GetBooking(bookingId);
            if (booking == null)
                return;

            foreach (var user in users)
            {
                if (CurrentUser.Identity.ID == user.ID)
                {
                    continue;
                }
                var notify = new UserNotification
                {
                    UserID = user.ID,
                    EntryId = booking.ID,
                    EventId = booking.EventID,
                    NotifyType = NotifyType.Booking,
                    Title = booking.StatusDisplay,
                    EventCode = booking.Event.EventCode,
                    Description = booking.CompanyName,
                    HighlightColor = booking.EventColor
                };
                if (!string.IsNullOrEmpty(title))
                {
                    notify.Title = title;
                }
                notify = MemberService.CreateUserNotification(notify);
                user.NotifyNumber++;
                MemberService.UpdateUser(user);
                NotificationHub.NotifyUser(user, notify);
            }

            //if (email)
            //{
            //    NotificationService.NotifyUser(users, booking);
            //}
        }
    }
}