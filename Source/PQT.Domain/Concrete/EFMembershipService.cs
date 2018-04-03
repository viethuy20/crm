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
                Roles = u.Roles.Select(r => r.Permissions),
            }).OrderBy(u => u.DisplayName)
                .AsEnumerable();
        }
        public IEnumerable<User> GetUsers()
        {
            return GetAll<User>(u => new
            {
                Roles = u.Roles.Select(r => r.Permissions),
            })
                .OrderBy(u => u.DisplayName)
                .AsEnumerable();
        }
        public IEnumerable<User> GetUsersDeleted()
        {
            return GetAll<User>(u => new
            {
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
                                u => u.Roles.Select(r => r.Permissions)).AsEnumerable();
        }

        public IEnumerable<User> GetUsersContainsInRole(params string[] roleName)
        {
            return GetAll<User>(u => u.Roles
                                      .Select(r => r.Name.ToUpper())
                                      .Intersect(roleName.Select(r1 => r1.ToUpper()))
                                      .Any(),
                                u => u.Roles.Select(r => r.Permissions)).AsEnumerable();
        }
        #endregion


        public IEnumerable<User> GetAllUserOfEmailTemplate(string type, string nameTemplate, EmailType emailType)
        {
            var usersResult = new List<User>();
            if (string.IsNullOrEmpty(nameTemplate) || string.IsNullOrEmpty(type))
            {
                return usersResult;
            }
            var emailSetting = Get<EmailSetting>(e => e.TemplateName.ToUpper().Trim() == nameTemplate.ToUpper().Trim() && e.Type.ToUpper().Trim() == type.ToUpper().Trim());
            if (emailSetting != null)
            {
                if (emailType == EmailType.To)
                {
                    if (emailSetting.EmailTo == null)
                    {
                        return usersResult;
                    }
                    var list = emailSetting.EmailTo.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    var userIds = new List<int>();
                    var roles = new List<string>();
                    foreach (var id in list)
                    {
                        try
                        {
                            userIds.Add(int.Parse(id));
                        }
                        catch (Exception)
                        {
                            roles.Add(id);
                        }
                    }
                    var usersByRole = GetAll<User>(m => m.Status == EntityStatus.Normal).ToList().Where(m => m.Roles.Select(r => StringHelper.RemoveSpecialCharacters(r.Name)).Any(roles.Contains));
                    if (usersByRole.Any())
                    {
                        usersResult.AddRange(usersByRole);
                    }
                    var users = GetAll<User>(m => m.Status == EntityStatus.Normal && userIds.Contains(m.ID));
                    if (users.Any())
                    {
                        usersResult.AddRange(users);
                    }
                }
                else if (emailType == EmailType.Cc)
                {
                    if (emailSetting.EmailCc == null)
                    {
                        return usersResult;
                    }
                    var list = emailSetting.EmailCc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    var userIds = new List<int>();
                    var roles = new List<string>();
                    foreach (var id in list)
                    {
                        try
                        {
                            userIds.Add(int.Parse(id));
                        }
                        catch (Exception)
                        {
                            roles.Add(id);
                        }
                    }
                    var usersByRole = GetAll<User>(m => m.Status == EntityStatus.Normal).ToList().Where(m => m.Roles.Select(r => StringHelper.RemoveSpecialCharacters(r.Name)).Any(roles.Contains));
                    if (usersByRole.Any())
                    {
                        usersResult.AddRange(usersByRole);
                    }
                    var users = GetAll<User>(m => m.Status == EntityStatus.Normal && userIds.Contains(m.ID));
                    if (users.Any())
                    {
                        usersResult.AddRange(users);
                    }
                }
                else if (emailType == EmailType.Bcc)
                {
                    if (emailSetting.EmailBcc == null)
                    {
                        return usersResult;
                    }
                    var list = emailSetting.EmailBcc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    var userIds = new List<int>();
                    var roles = new List<string>();
                    foreach (var id in list)
                    {
                        try
                        {
                            userIds.Add(int.Parse(id));
                        }
                        catch (Exception)
                        {
                            roles.Add(id);
                        }
                    }
                    var usersByRole = GetAll<User>(m => m.Status == EntityStatus.Normal).ToList().Where(m => m.Roles.Select(r => StringHelper.RemoveSpecialCharacters(r.Name)).Any(roles.Contains));
                    if (usersByRole.Any())
                    {
                        usersResult.AddRange(usersByRole);
                    }
                    var users = GetAll<User>(m => m.Status == EntityStatus.Normal && userIds.Contains(m.ID));
                    if (users.Any())
                    {
                        usersResult.AddRange(users);
                    }
                }
            }
            return usersResult.DistinctBy(m => m.ID);
        }



        public IEnumerable<User> GetAllSalesmans()
        {
            return GetAll<User>().AsEnumerable().Where(u => u.Roles.Any(r => r.RoleLevel == RoleLevel.SalesLevel));
        }

        public IEnumerable<UserNotification> GetAllUserNotifications(int userId,int pageSize = 10, int page = 1)
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
        public int SeenUserNotification(int userId, int leadId)
        {
            var notifications = GetAll<UserNotification>(m=>m.UserID == userId && m.EntryId == leadId);
            foreach (var userNotification in notifications)
            {
                userNotification.Seen = true;
                Update(userNotification);
            }
            return notifications.Count();
        }
    }
}
