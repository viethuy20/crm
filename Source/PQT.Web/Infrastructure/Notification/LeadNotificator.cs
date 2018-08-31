using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        private static ISettingRepository SettingRepository
        {
            get { return DependencyResolver.Current.GetService<ISettingRepository>(); }
        }
        private static IUserNotificationService UserNotificationService
        {
            get { return DependencyResolver.Current.GetService<IUserNotificationService>(); }
        }

        public static void NotifyUpdateNCL(int leadId, params string[] excludeIds)
        {
            var thread = new Thread(() =>
            {
                var lead = LeadRepository.GetLead(leadId);
                if (lead == null)
                    return;
                NotificationHub.Notify(lead, excludeIds);
            });
            thread.Start();
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

        private static IEventService EventService
        {
            get { return DependencyResolver.Current.GetService<IEventService>(); }
        }
        public static void NotifyUser(IEnumerable<User> users, int leadId, string title = null, bool email = false)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var lead = LeadRepository.GetLead(leadId);
                if (lead == null)
                    return;
                if (lead.Event == null)
                {
                    lead.Event = EventService.GetEvent(lead.EventID);
                }
                foreach (var user in users)
                {
                    if (user == null || currentUserId == user.ID)
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
                    notify = UserNotificationService.CreateUserNotification(notify);
                    user.NotifyNumber++;
                    MemberService.UpdateUser(user);
                    NotificationHub.NotifyUser(user, notify);
                }

                //if (email)
                //{
                //    NotificationService.NotifyUser(users, lead);
                //}

            });
            thread.Start();
        }

        public static void NotifyUser(NotifyAction notifyAction, int leadId, string title = null, bool email = false)
        {
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.Lead, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    NotifyUser(notiUsers, leadId, title, email);
                }
            });
            thread.Start();
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
        private static ISettingRepository SettingRepository
        {
            get { return DependencyResolver.Current.GetService<ISettingRepository>(); }
        }
        private static IBookingService BookingService
        {
            get { return DependencyResolver.Current.GetService<IBookingService>(); }
        }
        private static IEventService EventService
        {
            get { return DependencyResolver.Current.GetService<IEventService>(); }
        }
        private static IUserNotificationService UserNotificationService
        {
            get { return DependencyResolver.Current.GetService<IUserNotificationService>(); }
        }

        public static void NotifyEmailForUser(IEnumerable<User> users, Booking booking)
        {
            if (booking == null)
                return;
            NotificationService.NotifyUser(users, booking);
        }

        public static void NotifyUpdateBooking(int bookingId, bool reloadTableLead = true)
        {
            var thread = new Thread(() =>
            {
                var booking = BookingService.GetBooking(bookingId);
                if (booking == null)
                    return;
                booking.ReloadTableLead = reloadTableLead;
                NotificationHub.Notify(booking);

            });
            thread.Start();
        }
        public static void NotifyUser(IEnumerable<User> users, int bookingId, string title = null, bool email = false)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var booking = BookingService.GetBooking(bookingId);
                if (booking == null)
                    return;
                if (booking.Event == null)
                {
                    booking.Event = EventService.GetEvent(booking.EventID);
                }
                foreach (var user in users)
                {
                    if (user == null || currentUserId == user.ID)
                    {
                        continue;
                    }
                    try
                    {
                        var notify = new UserNotification
                        {
                            UserID = user.ID,
                            EntryId = booking.ID,
                            EventId = booking.EventID,
                            NotifyType = NotifyType.Booking,
                            Title = booking.StatusDisplay,
                            EventCode = booking.Event != null ? booking.Event.EventCode : "",
                            Description = booking.CompanyName,
                            HighlightColor = booking.EventColor
                        };
                        if (!string.IsNullOrEmpty(title))
                        {
                            notify.Title = title;
                        }
                        notify = UserNotificationService.CreateUserNotification(notify);
                        user.NotifyNumber++;
                        MemberService.UpdateUser(user);
                        NotificationHub.NotifyUser(user, notify);
                    }
                    catch (Exception e)
                    {
                    }
                }

                //if (email)
                //{
                //    NotificationService.NotifyUser(users, booking);
                //}

            });
            thread.Start();
        }


        public static void NotifyUser(NotifyAction notifyAction, int leadId, string title = null, bool email = false)
        {
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.Booking, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    NotifyUser(notiUsers, leadId, title, email);
                }
            });
            thread.Start();
        }
    }
    public class OpeEventNotificator
    {
        private static IMembershipService MemberService
        {
            get { return DependencyResolver.Current.GetService<IMembershipService>(); }
        }
        private static ISettingRepository SettingRepository
        {
            get { return DependencyResolver.Current.GetService<ISettingRepository>(); }
        }
        private static IEventService EventService
        {
            get { return DependencyResolver.Current.GetService<IEventService>(); }
        }
        private static IUserNotificationService UserNotificationService
        {
            get { return DependencyResolver.Current.GetService<IUserNotificationService>(); }
        }

        public static void NotifyUser(IEnumerable<User> users, int eventId, string title)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var eventData = EventService.GetEvent(eventId);
                if (eventData == null)
                    return;
                foreach (var user in users)
                {
                    if (user == null || currentUserId == user.ID)
                    {
                        continue;
                    }
                    try
                    {
                        var notify = new UserNotification
                        {
                            UserID = user.ID,
                            EntryId = eventData.ID,
                            EventId = eventData.ID,
                            NotifyType = NotifyType.OpeEvent,
                            Title = title,
                            EventCode = eventData.EventCode,
                            Description = "Operation info",
                            HighlightColor = eventData.BackgroundColor
                        };
                        if (!string.IsNullOrEmpty(title))
                        {
                            notify.Title = title;
                        }
                        notify = UserNotificationService.CreateUserNotification(notify);
                        user.NotifyNumber++;
                        MemberService.UpdateUser(user);
                        NotificationHub.NotifyUser(user, notify);
                    }
                    catch (Exception e)
                    {
                    }
                }

                //if (email)
                //{
                //    NotificationService.NotifyUser(users, booking);
                //}

            });
            thread.Start();
        }


        public static void NotifyUser(NotifyAction notifyAction, int leadId,string title)
        {
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.OpeEvent, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    NotifyUser(notiUsers, leadId, title);
                }
            });
            thread.Start();
        }
    }
}