﻿using System;
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
        public virtual IEnumerable<UserNotification> GetAllUserNotifications(int userId, string type, int pageSize = 10, int page = 1)
        {
            return GetAll<UserNotification>(m => m.UserID == userId &&
                                                 ((string.IsNullOrEmpty(type)
                                                   && m.NotifyType == NotifyType.Lead
                                                  ) || m.NotifyType == type))
                .OrderByDescending(m => m.CreatedTime)
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
                    return true;
                notify.Seen = true;
                var user = Get<User>(m => m.ID == notify.UserID);
                if (notify.NotifyType == NotifyType.Booking)
                {
                    if (user != null && user.BookingNotifyNumber > 0)
                    {
                        user.BookingNotifyNumber--;
                        Update(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.Invoice)
                {
                    if (user != null && user.InvoiceNotifyNumber > 0)
                    {
                        user.InvoiceNotifyNumber--;
                        Update(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.Recruitment)
                {
                    if (user != null && user.RecruitmentNotifyNumber > 0)
                    {
                        user.RecruitmentNotifyNumber--;
                        Update(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.OpeEvent)
                {
                    if (user != null && user.OpeEventNotifyNumber > 0)
                    {
                        user.OpeEventNotifyNumber--;
                        Update(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.NewEvent)
                {
                    if (user != null && user.NewEventNotifyNumber > 0)
                    {
                        user.NewEventNotifyNumber--;
                        Update(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.MasterFiles)
                {
                    if (user != null && user.MasterFilesNotifyNumber > 0)
                    {
                        user.MasterFilesNotifyNumber--;
                        Update(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.ReportCall)
                {
                    if (user != null && user.ReportCallNotifyNumber > 0)
                    {
                        user.ReportCallNotifyNumber--;
                        Update(user);
                    }
                }
                else if (notify.NotifyType == NotifyType.Leave)
                {
                    if (user != null && user.LeaveNotifyNumber > 0)
                    {
                        user.LeaveNotifyNumber--;
                        Update(user);
                    }
                }
                else
                {
                    if (user != null && user.NotifyNumber > 0)
                    {
                        user.NotifyNumber--;
                        Update(user);
                    }
                }
                return Update(notify);
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
