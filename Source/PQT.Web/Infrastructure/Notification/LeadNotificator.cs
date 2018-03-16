using System;
using System.Collections.Generic;
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

        //private static ILeadService LeadRepository
        //{
        //    get { return DependencyResolver.Current.GetService<ILeadService>(); }
        //}

        private static IMembershipService MemberService
        {
            get { return DependencyResolver.Current.GetService<IMembershipService>(); }
        }
        public static void NotifyAll(Lead lead, bool email = false)
        {
            var users = MemberService.GetUsers();
            foreach (var user in users)
            {
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
                if (CurrentUser.Identity.ID == user.ID)
                {
                    CurrentUser.Identity.NotifyNumber++;
                    NotificationHub.Notify(notify);
                }
            }
            if (email)
            {
                NotificationService.NotifyAll(lead);
            }
        }

        public void NotifyUser(IEnumerable<User> users, Lead lead, bool email = false)
        {
            if (lead == null)
                return;
            foreach (var user in users)
            {
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
                if (CurrentUser.Identity.ID == user.ID)
                {
                    CurrentUser.Identity.NotifyNumber++;
                    NotificationHub.Notify(notify);
                }
            }
            if (email)
            {
                NotificationService.NotifyAll(lead);
            }
        }

        public void NotifyRole(IEnumerable<Role> roles, Lead lead, bool email = false)
        {
            if (lead == null)
                return;
            var users = MemberService.GetUsersContainsInRole(roles.Select(m=>m.Name).ToArray());
            foreach (var user in users)
            {
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
                if (CurrentUser.Identity.ID == user.ID)
                {
                    CurrentUser.Identity.NotifyNumber++;
                    NotificationHub.Notify(notify);
                }
            }
            if (email)
            {
                NotificationService.NotifyAll(lead);
            }
        }
    }
}