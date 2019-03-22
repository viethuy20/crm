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
        private static IEventService EventService
        {
            get { return DependencyResolver.Current.GetService<IEventService>(); }
        }
        private static void NotifyUser(int currentUserId, IEnumerable<User> users, int leadId, string title = null, bool email = false)
        {
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
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.Lead, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    NotifyUser(currentUserId, notiUsers, leadId, title, email);
                }
            });
            thread.Start();
        }
        public static void NotifyUser(NotifyAction notifyAction, List<User> users, int leadId, string title = null, bool email = false)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.Lead, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    users.AddRange(notiUsers);
                }
                NotifyUser(currentUserId, users, leadId, title, email);
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
        private static void NotifyUser(int currentUserId, IEnumerable<User> users, int bookingId, string title = null, bool email = false)
        {
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
                        user.BookingNotifyNumber++;
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
        public static void NotifyUser(NotifyAction notifyAction, List<User> users, int leadId, string title = null, bool email = false)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.Booking, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    users.AddRange(notiUsers);
                }
                NotifyUser(currentUserId, users, leadId, title, email);
            });
            thread.Start();
        }
        public static void NotifyUser(NotifyAction notifyAction, int leadId, string title = null, bool email = false)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.Booking, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    NotifyUser(currentUserId, notiUsers, leadId, title, email);
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
        private static void NotifyUser(int currentUserId, IEnumerable<User> users, int eventId, string title)
        {
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
                            Description = "Operation Request",
                            HighlightColor = eventData.BackgroundColor
                        };
                        if (!string.IsNullOrEmpty(title))
                        {
                            notify.Title = title;
                        }
                        notify = UserNotificationService.CreateUserNotification(notify);
                        user.OpeEventNotifyNumber++;
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
        public static void NotifyUser(NotifyAction notifyAction, int leadId, string title)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.OpeEvent, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    NotifyUser(currentUserId, notiUsers, leadId, title);
                }
            });
            thread.Start();
        }
    }


    public class RecruitmentNotificator
    {
        private static IMembershipService MemberService
        {
            get { return DependencyResolver.Current.GetService<IMembershipService>(); }
        }
        private static ISettingRepository SettingRepository
        {
            get { return DependencyResolver.Current.GetService<ISettingRepository>(); }
        }
        private static IRecruitmentService RecruitmentService
        {
            get { return DependencyResolver.Current.GetService<IRecruitmentService>(); }
        }
        private static IUserNotificationService UserNotificationService
        {
            get { return DependencyResolver.Current.GetService<IUserNotificationService>(); }
        }
        private static void NotifyUser(int currentUserId, IEnumerable<User> users, int id, string title)
        {
            var thread = new Thread(() =>
            {
                var eventData = RecruitmentService.GetCandidate(id);
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
                            EventId = 0,
                            NotifyType = NotifyType.Recruitment,
                            Title = title,
                            EventCode = "",
                            Description = eventData.FullName,
                            HighlightColor = "#ffffff"
                        };
                        if (!string.IsNullOrEmpty(title))
                        {
                            notify.Title = title;
                        }
                        notify = UserNotificationService.CreateUserNotification(notify);
                        user.RecruitmentNotifyNumber++;
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
        public static void NotifyUser(NotifyAction notifyAction, List<User> users, int leadId, string title)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.Recruitment, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    users.AddRange(notiUsers);
                }
                NotifyUser(currentUserId, users, leadId, title);
            });
            thread.Start();
        }
        public static void NotifyUser(NotifyAction notifyAction, int leadId, string title)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.Recruitment, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    NotifyUser(currentUserId, notiUsers, leadId, title);
                }
            });
            thread.Start();
        }
    }

    public class RecruitmentPositionNotificator
    {
        private static IMembershipService MemberService
        {
            get { return DependencyResolver.Current.GetService<IMembershipService>(); }
        }
        private static ISettingRepository SettingRepository
        {
            get { return DependencyResolver.Current.GetService<ISettingRepository>(); }
        }
        private static IUnitRepository UnitService
        {
            get { return DependencyResolver.Current.GetService<IUnitRepository>(); }
        }
        private static IUserNotificationService UserNotificationService
        {
            get { return DependencyResolver.Current.GetService<IUserNotificationService>(); }
        }
        private static void NotifyUser(int currentUserId, IEnumerable<User> users, int id, string title)
        {
            var eventData = UnitService.GetRecruitmentPosition(id);
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
                        EventId = 0,
                        NotifyType = NotifyType.MasterFiles,
                        Title = title,
                        EventCode = "",
                        Description = eventData.Description,
                        HighlightColor = "#ffffff"
                    };
                    if (!string.IsNullOrEmpty(title))
                    {
                        notify.Title = title;
                    }
                    notify = UserNotificationService.CreateUserNotification(notify);
                    user.MasterFilesNotifyNumber++;
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
        }
        public static void NotifyUser(NotifyAction notifyAction, List<User> users, int leadId, string title)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.MasterFiles, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    users.AddRange(notiUsers);
                }
                NotifyUser(currentUserId, users, leadId, title);
            });
            thread.Start();
        }
        public static void NotifyUser(NotifyAction notifyAction, int leadId, string title)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.MasterFiles, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    NotifyUser(currentUserId, notiUsers, leadId, title);
                }
            });
            thread.Start();
        }
    }



    public class NewEventNotificator
    {
        private static ILeadNewService LeadNewService
        {
            get { return DependencyResolver.Current.GetService<ILeadNewService>(); }
        }
        private static IMembershipService MemberService
        {
            get { return DependencyResolver.Current.GetService<IMembershipService>(); }
        }
        private static IUserNotificationService UserNotificationService
        {
            get { return DependencyResolver.Current.GetService<IUserNotificationService>(); }
        }
        private static ISettingRepository SettingRepository
        {
            get { return DependencyResolver.Current.GetService<ISettingRepository>(); }
        }
        private static void NotifyUser(int currentUserId, IEnumerable<User> users, int bookingId, string title)
        {
            var booking = LeadNewService.GetLeadNew(bookingId);
            if (booking == null)
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
                        EntryId = booking.ID,
                        EventId = 0,
                        NotifyType = NotifyType.NewEvent,
                        Title = title,
                        EventCode = "",
                        Description = booking.NewTopics,
                        HighlightColor = booking.EventColor
                    };
                    notify = UserNotificationService.CreateUserNotification(notify);
                    user.NewEventNotifyNumber++;
                    MemberService.UpdateUser(user);
                    NotificationHub.NotifyUser(user, notify);
                }
                catch (Exception e)
                {
                }
            }
        }
        public static void NotifyUser(NotifyAction notifyAction, List<User> users, int leadId, string title)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.NewEvent, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    users.AddRange(notiUsers);
                }
                NotifyUser(currentUserId, users, leadId, title);
            });
            thread.Start();
        }
        public static void NotifyUser(NotifyAction notifyAction, int leadId, string title)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.NewEvent, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    NotifyUser(currentUserId, notiUsers, leadId, title);
                }
            });
            thread.Start();
        }
    }
    public class ReportCallNotificator
    {
        private static IReportCallService ReportCallService
        {
            get { return DependencyResolver.Current.GetService<IReportCallService>(); }
        }
        private static IMembershipService MemberService
        {
            get { return DependencyResolver.Current.GetService<IMembershipService>(); }
        }
        private static IUserNotificationService UserNotificationService
        {
            get { return DependencyResolver.Current.GetService<IUserNotificationService>(); }
        }
        private static ISettingRepository SettingRepository
        {
            get { return DependencyResolver.Current.GetService<ISettingRepository>(); }
        }
        private static void NotifyUser(int currentUserId, IEnumerable<User> users, int bookingId, string title)
        {
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
                        EntryId = bookingId,
                        EventId = 0,
                        NotifyType = NotifyType.ReportCall,
                        Title = title,
                        EventCode = "",
                        Description = "Report Call #" + bookingId,
                        HighlightColor = "#ffffff"
                    };
                    notify = UserNotificationService.CreateUserNotification(notify);
                    user.ReportCallNotifyNumber++;
                    MemberService.UpdateUser(user);
                    NotificationHub.NotifyUser(user, notify);
                }
                catch (Exception e)
                {
                }
            }
        }
        public static void NotifyUser(NotifyAction notifyAction, int leadId, string title)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.ReportCall, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    NotifyUser(currentUserId, notiUsers, leadId, title);
                }
            });
            thread.Start();
        }
    }
    public class InvoiceNotificator
    {
        private static IInvoiceService InvoiceService
        {
            get { return DependencyResolver.Current.GetService<IInvoiceService>(); }
        }
        private static IMembershipService MemberService
        {
            get { return DependencyResolver.Current.GetService<IMembershipService>(); }
        }
        private static IUserNotificationService UserNotificationService
        {
            get { return DependencyResolver.Current.GetService<IUserNotificationService>(); }
        }
        private static ISettingRepository SettingRepository
        {
            get { return DependencyResolver.Current.GetService<ISettingRepository>(); }
        }
        private static void NotifyUser(int currentUserId, IEnumerable<User> users, int invoiceId, string title)
        {
            var invoice = InvoiceService.GetInvoice(invoiceId);
            if (invoice == null)
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
                        EntryId = invoice.ID,
                        EventId = invoice.Booking.EventID,
                        NotifyType = NotifyType.Invoice,
                        Title = title,
                        EventCode = invoice.Booking.EventCode,
                        Description = invoice.InvoiceNo,
                        HighlightColor = invoice.Booking.EventColor
                    };
                    notify = UserNotificationService.CreateUserNotification(notify);
                    user.InvoiceNotifyNumber++;
                    MemberService.UpdateUser(user);
                    NotificationHub.NotifyUser(user, notify);
                }
                catch (Exception e)
                {
                }
            }
        }
        public static void NotifyUser(NotifyAction notifyAction, int leadId, string title)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.Invoice, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    NotifyUser(currentUserId, notiUsers, leadId, title);
                }
            });
            thread.Start();
        }
    }



    public class LeaveNotificator
    {
        private static IMembershipService MemberService
        {
            get { return DependencyResolver.Current.GetService<IMembershipService>(); }
        }
        private static ISettingRepository SettingRepository
        {
            get { return DependencyResolver.Current.GetService<ISettingRepository>(); }
        }
        private static ILeaveService LeaveService
        {
            get { return DependencyResolver.Current.GetService<ILeaveService>(); }
        }
        private static IUserNotificationService UserNotificationService
        {
            get { return DependencyResolver.Current.GetService<IUserNotificationService>(); }
        }
        private static void NotifyUser(int currentUserId, IEnumerable<User> users, int id, string title)
        {
            var eventData = LeaveService.GetLeave(id);
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
                        EventId = 0,
                        NotifyType = NotifyType.Leave,
                        Title = title,
                        EventCode = "",
                        Description = eventData.LeaveDateDisplay + " - " + eventData.UserDisplay,
                        HighlightColor = "#ffffff"
                    };
                    if (!string.IsNullOrEmpty(title))
                    {
                        notify.Title = title;
                    }
                    notify = UserNotificationService.CreateUserNotification(notify);
                    user.LeaveNotifyNumber++;
                    MemberService.UpdateUser(user);
                    NotificationHub.NotifyUser(user, notify);
                }
                catch (Exception e)
                {
                }
            }
        }
        public static void NotifyUser(NotifyAction notifyAction, List<User> users, int leadId, string title)
        {
            var currentUserId = CurrentUser.Identity.ID;
            var thread = new Thread(() =>
            {
                var setting = SettingRepository.GetNotifySetting(NotifyType.Leave, notifyAction);
                if (setting != null)
                {
                    var notiUsers = MemberService.GetUsersInRole(setting.AllRoles);
                    users.AddRange(notiUsers);
                }
                NotifyUser(currentUserId, users, leadId, title);
            });
            thread.Start();
        }
    }
}