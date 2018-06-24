using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;

namespace PQT.Domain.Concrete
{
    public class EFMembershipService : Repository, IMembershipService
    {
        public EFMembershipService(DbContext db)
            : base(db)
        {
        }

        #region IMembershipService Members

        public IEnumerable<User> GetUsers(Func<User, bool> predicate)
        {
            return GetAll(predicate, u => new
            {
                u.UserSalaryHistories,
                Roles = u.Roles.Select(r => r.Permissions),
            }).OrderBy(u => u.DisplayName)
                .AsEnumerable();
        }
        public IEnumerable<User> GetUsers()
        {
            return GetAll<User>(u => new
            {
                u.UserSalaryHistories,
                Roles = u.Roles.Select(r => r.Permissions),
            })
                .OrderBy(u => u.DisplayName)
                .AsEnumerable();
        }
        public IEnumerable<User> GetUsersDeleted()
        {
            return GetAll<User>(u => new
            {
                u.UserSalaryHistories,
                Roles = u.Roles.Select(r => r.Permissions),
            }).OrderBy(u => u.DisplayName)
                .AsEnumerable();
        }


        public virtual bool UpdateUser(User userInfo)
        {
            return Update(userInfo);
        }

        public void UpdateUserPicture(int id, string fileName)
        {
            User user = GetUser(id);
            if (user == null) return;

            user.Picture = fileName;
            Update(user);
            //_db.SaveChanges();
        }

        public virtual User GetUser(int id)
        {
            return Get<User>(u => u.ID == id, u => new
            {
                u.UserSalaryHistories,
                Roles = u.Roles.Select(r => r.Permissions),
            });
        }
        public virtual User GetUserIncludeAll(int id)
        {
            return Get<User>(u => u.ID == id, u => new
            {
                u.UserSalaryHistories,
                Roles = u.Roles.Select(r => r.Permissions),
            });
        }
        public User GetUserByName(string username)
        {
            return string.IsNullOrWhiteSpace(username)
                       ? null
                       : Get<User>(u => u.DisplayName != null &&
                                        u.DisplayName.ToLower() == username.ToLower(),
                                   u => u.Roles.Select(r => r.Permissions));
        }

        public User GetUserByEmail(string email)
        {
            return string.IsNullOrWhiteSpace(email)
                       ? null
                       : Get<User>(u => u.Email != null &&
                                        u.Email.Trim().ToLower() == email.Trim().ToLower(),
                                   u => u.Roles.Select(r => r.Permissions));
        }
        public IEnumerable<User> GetAllUserByEmail(string email)
        {
            return GetAll<User>(m => m.Email.Trim().ToLower() == email.Trim().ToLower(), u => new
            {
                u.UserSalaryHistories,
                Roles = u.Roles.Select(r => r.Permissions),
            }).ToList();
        }
        public User ValidateLogin(string email, string password)
        {
            password = EncryptHelper.EncryptPassword(password);
            return Get<User>(u => u.Email != null &&
                                  u.Email.Trim().ToLower() == email.Trim().ToLower() &&
                                  u.Password == password &&
                                  u.Status == EntityStatus.Normal);
        }

        public User CreateUser(User user)
        {
            user.Email = user.Email.Trim();
            user.Password = EncryptHelper.EncryptPassword(user.Password);
            return Create(user);
        }

        public bool DeleteUser(int id)
        {
            var user = GetUser(id);
            user.Status = EntityStatus.Deleted;
            return Update(user);
        }
        public bool ReActiveUser(int id)
        {
            var user = GetUser(id);
            user.Status = EntityStatus.Normal;
            return Update(user);
        }

        public IEnumerable<User> GetUsersInRole(params string[] roleName)
        {
            return GetAll<User>(u => u.Roles
                                      .Select(r => r.Name.ToUpper())
                                      .Intersect(roleName.Select(r1 => r1.ToUpper()))
                                      .Any(),
                u=>u.UserSalaryHistories,
                                u => u.Roles.Select(r => r.Permissions)).AsEnumerable();
        }

        public IEnumerable<User> GetUsersContainsInRole(params string[] roleName)
        {
            return GetAll<User>(u => u.Roles
                                      .Select(r => r.Name.ToUpper())
                                      .Intersect(roleName.Select(r1 => r1.ToUpper()))
                                      .Any(),
                u => u.UserSalaryHistories,
                                u => u.Roles.Select(r => r.Permissions)).AsEnumerable();
        }
        #endregion


        public EmailSetting GetEmailTemplate(string type, string nameTemplate)
        {
            return Get<EmailSetting>(e => e.TemplateName.ToUpper().Trim() == nameTemplate.ToUpper().Trim() && e.Type.ToUpper().Trim() == type.ToUpper().Trim());
        }

        public IEnumerable<User> GetAllSalesmans()
        {
            return GetAll<User>().AsEnumerable().Where(u => u.Roles.Any(r => r.RoleLevel == RoleLevel.SalesLevel));
        }

        public IEnumerable<UserNotification> GetAllUserNotifications(int userId, int pageSize = 10, int page = 1)
        {
            return _db.Set<UserNotification>()
                .Where(m => m.UserID == userId).OrderByDescending(m => m.CreatedTime)
                .Skip((page - 1) * pageSize).Take(pageSize).AsEnumerable();
            //return GetAll<UserNotification>(m => m.UserID == userId).ToList().Skip((page - 1) * pageSize).Take(pageSize);
        }
        public IEnumerable<UserNotification> GetAllUserNotificationsByEvent(int userId, int eventId, int pageSize = 10, int page = 1)
        {
            return _db.Set<UserNotification>()
                .Where(m => m.UserID == userId && m.EventId == eventId).OrderByDescending(m => m.CreatedTime)
                .Skip((page - 1) * pageSize).Take(pageSize).AsEnumerable();
            //return GetAll<UserNotification>(m => m.UserID == userId).ToList().Skip((page - 1) * pageSize).Take(pageSize);
        }
        public UserNotification CreateUserNotification(UserNotification notify)
        {
            return Create(notify);
        }
        public bool UpdateUserNotification(UserNotification notify)
        {
            return Update(notify);
        }
        public bool SeenUserNotification(int notifyId)
        {
            var notify = Get<UserNotification>(notifyId);
            notify.Seen = true;
            return Update(notify);
        }
        public int SeenUserNotification(int userId, int entryId)
        {
            var notifications = GetAll<UserNotification>(m => m.UserID == userId && m.EntryId == entryId);
            foreach (var userNotification in notifications)
            {
                userNotification.Seen = true;
                Update(userNotification);
            }
            return notifications.Count();
        }
    }
}
