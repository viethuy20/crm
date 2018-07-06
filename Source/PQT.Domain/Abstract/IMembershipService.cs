using System;
using System.Collections.Generic;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Abstract
{
    public interface IMembershipService : IAuthenticationService
    {
        int GetCountUsers(Func<User, bool> predicate);
        IEnumerable<User> GetUsers(Func<User, bool> predicate, string sortColumnDir, string sortColumn, int page, int pageSize);
        IEnumerable<User> GetUsers(string sortColumnDir, string sortColumn, int page, int pageSize);
        IEnumerable<User> GetUsers(Func<User, bool> predicate);
        IEnumerable<User> GetUsersDeleted();
        User GetUser(int id);
        User GetUserIncludeAll(int id);
        User GetUserByEmail(string email);
        User CreateUser(User userInfo);
        bool UpdateUser(User userInfo);
        bool DeleteUser(int id);
        IEnumerable<User> GetUsersInRole(params string[] roleName);
        EmailSetting GetEmailTemplate(string type, string nameTemplate);

        bool ReActiveUser(int id);

        IEnumerable<User> GetAllSalesmans();

        IEnumerable<UserNotification> GetAllUserNotifications(int userId, int pageSize = 10, int page = 1);
        IEnumerable<UserNotification> GetAllUserNotificationsByEvent(int userId, int eventId, int pageSize = 10, int page = 1);
        UserNotification CreateUserNotification(UserNotification notify);
        bool UpdateUserNotification(UserNotification notify);
        bool SeenUserNotification(int notifyId);
        int SeenUserNotification(int userId, int entryId, NotifyType type);
    }
}
