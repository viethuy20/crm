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
        public static void NotifyAll(Lead lead, bool email = false)
        {
            var users = MemberService.GetUsers();
            if (lead == null || !users.Any())
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
                    NotifyType = NotifyType.Lead,
                    Title = "Called by " + lead.User.DisplayName,
                    Description = lead.CompanyName,
                    HighlightColor = lead.EventColor
                };
                notify = MemberService.CreateUserNotification(notify);
                user.NotifyNumber++;
                MemberService.UpdateUser(user);
                NotificationHub.Notify(notify);
            }
            if (email)
            {
                NotificationService.NotifyAll(lead);
            }
        }

        public static void NotifyUser(IEnumerable<User> users, Lead lead, bool email = false)
        {
            if (lead == null || !users.Any())
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
                    NotifyType = NotifyType.Lead,
                    Title = lead.LeadStatusRecord.Status.DisplayName + " by " + lead.LeadStatusRecord.User.DisplayName,
                    Description = lead.CompanyName,
                    HighlightColor = lead.EventColor
                };
                notify = MemberService.CreateUserNotification(notify);
                user.NotifyNumber++;
                MemberService.UpdateUser(user);
                NotificationHub.NotifyUser(user, notify);
            }
            if (email)
            {
                NotificationService.NotifyUser(users, lead);
            }
        }

        public static void NotifyRole(IEnumerable<Role> roles, Lead lead, bool email = false)
        {
            if (lead == null || !roles.Any())
                return;
            var users = MemberService.GetUsersContainsInRole(roles.Select(m => m.Name).ToArray());
            UserNotification notify = null;
            foreach (var user in users)
            {
                if (CurrentUser.Identity.ID == user.ID)
                {
                    continue;
                }
                notify = new UserNotification
                {
                    UserID = user.ID,
                    EntryId = lead.ID,
                    NotifyType = NotifyType.Lead,
                    Title = "Called by " + lead.User.DisplayName,
                    Description = lead.CompanyName,
                    HighlightColor = lead.EventColor
                };
                notify = MemberService.CreateUserNotification(notify);
                user.NotifyNumber++;
                MemberService.UpdateUser(user);
            }

            if (notify != null)
            {
                notify.UserID = 0;
                foreach (var role in roles)
                {
                    NotificationHub.NotifyRole(role, notify);
                }
            }
            if (email)
            {
                NotificationService.NotifyRole(roles, lead);
            }
        }
    }
}