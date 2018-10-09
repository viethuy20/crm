using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Concrete
{
    public class EFUserNotificationService : Repository, IUserNotificationService
    {
        public EFUserNotificationService(DbContext db)
            : base(db)
        {
        }

        public IEnumerable<UserNotification> GetAllUserNotifications()
        {
            return GetAll<UserNotification>().AsEnumerable();
        }
        public virtual IEnumerable<UserNotification> GetAllUserNotifications(int userId, int pageSize = 10, int page = 1)
        {
            return GetAll<UserNotification>(m => m.UserID == userId).OrderByDescending(m => m.CreatedTime)
                .Skip((page - 1) * pageSize).Take(pageSize).AsEnumerable();
        }
        public virtual IEnumerable<UserNotification> GetAllUserNotificationsByEvent(int userId, int eventId, int pageSize = 10, int page = 1)
        {
            return GetAll<UserNotification>(m => m.UserID == userId && m.EventId == eventId).OrderByDescending(m => m.CreatedTime)
                .Skip((page - 1) * pageSize).Take(pageSize).AsEnumerable();
        }
        public virtual IEnumerable<UserNotification> GetAllUserNotificationsByEvent(int userId, int[] eventId, int pageSize = 10, int page = 1)
        {
            return GetAll<UserNotification>(m => m.UserID == userId && eventId.Contains(m.EventId)).OrderByDescending(m => m.CreatedTime)
                .Skip((page - 1) * pageSize).Take(pageSize).AsEnumerable();
        }
        public virtual IEnumerable<UserNotification> GetAllUserNotificationsByNewEvent(int userId, int pageSize = 10, int page = 1)
        {
            return GetAll<UserNotification>(m => m.UserID == userId && m.NotifyType == NotifyType.NewEvent).OrderByDescending(m => m.CreatedTime)
                .Skip((page - 1) * pageSize).Take(pageSize).AsEnumerable();
        }
        public virtual UserNotification CreateUserNotification(UserNotification notify)
        {
            return Create(notify);
        }
        public virtual bool UpdateUserNotification(UserNotification notify)
        {
            return Update(notify);
        }
        public virtual bool SeenUserNotification(int notifyId)
        {
            return TransactionWrapper.Do(() =>
            {
                var notify = _db.Set<UserNotification>().FirstOrDefault(m => m.ID == notifyId);
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
            });
        }
        public virtual int SeenUserNotification(int userId, int entryId, NotifyType type)
        {
            var notifications = _db.Set<UserNotification>().Where(m => m.UserID == userId && m.EntryId == entryId && m.NotifyType.Value == type.Value && !m.Seen).ToList();
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
