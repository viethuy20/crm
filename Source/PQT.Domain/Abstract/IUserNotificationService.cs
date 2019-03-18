using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Abstract
{
    public interface IUserNotificationService
    {
        IEnumerable<UserNotification> GetAllUserNotifications(int userId, string type, int pageSize = 10, int page = 1);
        IEnumerable<UserNotification> GetAllUserNotificationsByEvent(int userId, int eventId, int pageSize = 10, int page = 1);
        IEnumerable<UserNotification> GetAllUserNotificationsByEvent(int userId, int[] eventId, int pageSize = 10, int page = 1);
        IEnumerable<UserNotification> GetAllUserNotificationsByNewEvent(int userId, int pageSize = 10, int page = 1);
        UserNotification CreateUserNotification(UserNotification notify);
        bool UpdateUserNotification(UserNotification notify);
        bool SeenUserNotification(int notifyId);
        int SeenUserNotification(int userId, int entryId, NotifyType type);
    }
}
