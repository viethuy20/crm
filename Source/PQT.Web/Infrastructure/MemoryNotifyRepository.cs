using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using PQT.Domain.Abstract;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Web.Infrastructure
{
    public class MemoryNotifyRepository : EFUserNotificationService
    {
        private List<UserNotification> _resoures = new List<UserNotification>();

        #region Factory

        public MemoryNotifyRepository(DbContext db)
            : base(db)
        {
            RetrieveNotifyResources();
        }

        #endregion

        #region Decorator Properties

        public EFUserNotificationService NotificationService
        {
            get { return DependencyHelper.GetService<EFUserNotificationService>(); }
        }
        public IMembershipService MembershipService
        {
            get { return DependencyHelper.GetService<IMembershipService>(); }
        }
        #endregion

        private void RetrieveNotifyResources()
        {
            _resoures.Clear();
            _resoures.AddRange(NotificationService.GetAllUserNotifications());
        }

        
        public override IEnumerable<UserNotification> GetAllUserNotifications(int userId,string type, int pageSize = 10, int page = 1)
        {
            return _resoures.Where(m => m.UserID == userId &&
                                        ((string.IsNullOrEmpty(type)
                                          && m.NotifyType == NotifyType.Lead
                                         ) || m.NotifyType == type)).OrderByDescending(m => m.CreatedTime)
                .Skip((page - 1) * pageSize).Take(pageSize).AsEnumerable();
        }
        public override IEnumerable<UserNotification> GetAllUserNotificationsByEvent(int userId, int eventId, int pageSize = 10, int page = 1)
        {
            return _resoures.Where(m => m.UserID == userId && m.EventId == eventId).OrderByDescending(m => m.CreatedTime)
                .Skip((page - 1) * pageSize).Take(pageSize).AsEnumerable();
        }
        public override IEnumerable<UserNotification> GetAllUserNotificationsByEvent(int userId, int[] eventIds, int pageSize = 10, int page = 1)
        {
            return _resoures.Where(m => m.UserID == userId && eventIds.Contains(m.EventId)).OrderByDescending(m => m.CreatedTime)
                .Skip((page - 1) * pageSize).Take(pageSize).AsEnumerable();
        }
        public override IEnumerable<UserNotification> GetAllUserNotificationsByNewEvent(int userId, int pageSize = 10, int page = 1)
        {
            return _resoures.Where(m => m.UserID == userId && m.NotifyType == NotifyType.NewEvent).OrderByDescending(m => m.CreatedTime)
                .Skip((page - 1) * pageSize).Take(pageSize).AsEnumerable();
        }

        private UserNotification GetUserNotification(int id)
        {
            return _resoures.FirstOrDefault(m => m.ID == id);
        }
        public override UserNotification CreateUserNotification(UserNotification notify)
        {
            var menu = NotificationService.CreateUserNotification(notify);
            _resoures.Add(menu);
            return menu;
        }
        public override bool UpdateUserNotification(UserNotification notify)
        {
            if (!NotificationService.UpdateUserNotification(notify)) return false;
            _resoures.Remove(GetUserNotification(notify.ID));
            _resoures.Add(notify);
            return true;
        }
        public override bool SeenUserNotification(int notifyId)
        {
            var notify = GetUserNotification(notifyId);
            if (notify == null) return false;
            if (!notify.Seen)
            {
                notify.Seen = true;
                var user = Get<User>(m => m.ID == notify.UserID);
                if (notify.NotifyType == NotifyType.Booking)
                {
                    if (user != null && user.BookingNotifyNumber > 0)
                    {
                        user.BookingNotifyNumber--;
                        MembershipService.UpdateUser(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.Invoice)
                {
                    if (user != null && user.InvoiceNotifyNumber > 0)
                    {
                        user.InvoiceNotifyNumber--;
                        MembershipService.UpdateUser(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.Recruitment)
                {
                    if (user != null && user.RecruitmentNotifyNumber > 0)
                    {
                        user.RecruitmentNotifyNumber--;
                        MembershipService.UpdateUser(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.OpeEvent)
                {
                    if (user != null && user.OpeEventNotifyNumber > 0)
                    {
                        user.OpeEventNotifyNumber--;
                        MembershipService.UpdateUser(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.NewEvent)
                {
                    if (user != null && user.NewEventNotifyNumber > 0)
                    {
                        user.NewEventNotifyNumber--;
                        MembershipService.UpdateUser(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.MasterFiles)
                {
                    if (user != null && user.MasterFilesNotifyNumber > 0)
                    {
                        user.MasterFilesNotifyNumber--;
                        MembershipService.UpdateUser(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.ReportCall)
                {
                    if (user != null && user.ReportCallNotifyNumber > 0)
                    {
                        user.ReportCallNotifyNumber--;
                        MembershipService.UpdateUser(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.Leave)
                {
                    if (user != null && user.LeaveNotifyNumber > 0)
                    {
                        user.LeaveNotifyNumber--;
                        MembershipService.UpdateUser(user);
                    }
                }
                else
                {
                    if (user != null && user.NotifyNumber > 0)
                    {
                        user.NotifyNumber--;
                        MembershipService.UpdateUser(user);
                    }
                }
                return NotificationService.UpdateUserNotification(notify);
            }
            return true;
        }
        public override int SeenUserNotification(int userId, int entryId, NotifyType type)
        {
            var notifications = _resoures.Where(m => m.UserID == userId && m.EntryId == entryId && m.NotifyType.Value == type.Value).ToList();
            var countSeen = 0;
            foreach (var notify in notifications)
            {
                if (notify.Seen) continue;
                notify.Seen = true;
                countSeen++;
                NotificationService.UpdateUserNotification(notify);
            }
            return countSeen;
        }

    }
}
