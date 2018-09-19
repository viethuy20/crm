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
        #endregion

        private void RetrieveNotifyResources()
        {
            _resoures.Clear();
            _resoures.AddRange(NotificationService.GetAllUserNotifications());
        }


        public override IEnumerable<UserNotification> GetAllUserNotifications(int userId, int pageSize = 10, int page = 1)
        {
            return _resoures.Where(m => m.UserID == userId).OrderByDescending(m => m.CreatedTime)
                .Skip((page - 1) * pageSize).Take(pageSize).AsEnumerable();
        }
        public override IEnumerable<UserNotification> GetAllUserNotificationsByEvent(int userId, int eventId, int pageSize = 10, int page = 1)
        {
            return _resoures.Where(m => m.UserID == userId && m.EventId == eventId).OrderByDescending(m => m.CreatedTime)
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
                if (user != null && user.NotifyNumber > 0)
                {
                    user.NotifyNumber--;
                    Update(user);
                }
                return Update(notify);
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
                Update(notify);
            }
            return countSeen;
        }

    }
}
