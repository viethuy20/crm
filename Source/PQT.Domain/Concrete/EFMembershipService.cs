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

        public int GetCountUsers(Func<User, bool> predicate)
        {
            if (predicate != null)
            {
                return _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).Count();
            }
            return _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).Count();
        }
        public IEnumerable<User> GetUsers(Func<User, bool> predicate, string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            IEnumerable<User> users = new HashSet<User>();
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "DisplayName":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderBy(s => s.DisplayName).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "FirstName":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderBy(s => s.FirstName).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "LastName":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderBy(s => s.LastName).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Email":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderBy(s => s.Email).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "BusinessPhone":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderBy(s => s.BusinessPhone).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "MobilePhone":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderBy(s => s.MobilePhone).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Extension":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderBy(s => s.Extension).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "RolesHtml":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderBy(s => s.RolesHtml).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "DateOfBirth":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderBy(s => s.DateOfBirth).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    default:
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderBy(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "DisplayName":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderByDescending(s => s.DisplayName).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "FirstName":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderByDescending(s => s.FirstName).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "LastName":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderByDescending(s => s.LastName).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Email":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderByDescending(s => s.Email).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "BusinessPhone":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderByDescending(s => s.BusinessPhone).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "MobilePhone":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderByDescending(s => s.MobilePhone).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Extension":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderByDescending(s => s.Extension).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "RolesHtml":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderByDescending(s => s.RolesHtml).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "DateOfBirth":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderByDescending(s => s.DateOfBirth).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    default:
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).OrderByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                }
            }
            return users;
        }
        public IEnumerable<User> GetUsers(string sortColumnDir, string sortColumn, int page, int pageSize)
        {
            IEnumerable<User> users = new HashSet<User>();
            if (sortColumnDir == "asc")
            {
                switch (sortColumn)
                {
                    case "DisplayName":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderBy(s => s.DisplayName).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "FirstName":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderBy(s => s.FirstName).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "LastName":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderBy(s => s.LastName).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Email":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderBy(s => s.Email).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "BusinessPhone":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderBy(s => s.BusinessPhone).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "MobilePhone":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderBy(s => s.MobilePhone).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Extension":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderBy(s => s.Extension).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "RolesHtml":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderBy(s => s.RolesHtml).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "DateOfBirth":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderBy(s => s.DateOfBirth).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    default:
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderBy(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                }
            }
            else
            {
                switch (sortColumn)
                {
                    case "DisplayName":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderByDescending(s => s.DisplayName).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "FirstName":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderByDescending(s => s.FirstName).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "LastName":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderByDescending(s => s.LastName).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Email":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderByDescending(s => s.Email).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "BusinessPhone":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderByDescending(s => s.BusinessPhone).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "MobilePhone":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderByDescending(s => s.MobilePhone).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "Extension":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderByDescending(s => s.Extension).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "RolesHtml":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderByDescending(s => s.RolesHtml).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    case "DateOfBirth":
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderByDescending(s => s.DateOfBirth).ThenByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                    default:
                        users = _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).OrderByDescending(s => s.ID).Skip(page).Take(pageSize).AsEnumerable();
                        break;
                }
            }
            return users;
        }
        public IEnumerable<User> GetUsers(Func<User, bool> predicate)
        {
            return _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).AsEnumerable();
        }
        public IEnumerable<User> GetUsersDeleted()
        {
            return _db.Set<User>().Include(m => m.UserSalaryHistories)
                .Include(m => m.Roles.Select(r => r.Permissions))
                .Where(m => m.Status.Value == EntityStatus.Deleted.Value).OrderBy(u => u.DisplayName)
                .AsEnumerable();
        }


        public virtual bool UpdateUser(User userInfo)
        {
            return Update(userInfo);
        }

        public virtual User GetUser(int id)
        {
            return Get<User>(u => u.ID == id, u => new
            {
                Roles = u.Roles.Select(r => r.Permissions),
            });
        }
        public User GetUserIncludeAll(int id)
        {
            return Get<User>(u => u.ID == id, u => new
            {
                u.UserSalaryHistories,
                u.OfficeLocation,
                Roles = u.Roles.Select(r => r.Permissions),
            });
        }

        public User GetUserByEmail(string email)
        {
            return string.IsNullOrWhiteSpace(email)
                ? null
                : Get<User>(u => u.Email != null &&
                                 u.Email.Trim().ToLower() == email.Trim().ToLower(),
                    u => new
                    {
                        Roles = u.Roles.Select(r => r.Permissions),
                    });
        }
        public User ValidateLogin(string email, string password)
        {
            //password = EncryptHelper.EncryptPassword(password);
            return Get<User>(u => u.Email != null &&
                                  u.Email.Trim().ToLower() == email.Trim().ToLower() &&
                                  u.Password == password &&
                                  u.UserStatus == UserStatus.Live &&
                                  u.Status == EntityUserStatus.Normal);
        }

        public User CreateUser(User user)
        {
            user.Email = user.Email.Trim();
            //user.Password = EncryptHelper.EncryptPassword(user.Password);
            return Create(user);
        }

        public bool DeleteUser(int id)
        {
            var user = GetUser(id);
            user.Status = EntityUserStatus.Deleted;
            return Update(user);
        }
        public bool ReActiveUser(int id)
        {
            var user = GetUser(id);
            user.Status = EntityUserStatus.Normal;
            return Update(user);
        }

        public IEnumerable<User> GetUsersInRole(params string[] roleName)
        {
            if (!roleName.Any())
            {
                return new List<User>();
            }
            return GetAll<User>(u => u.Roles
                    .Select(r => r.Name.ToUpper())
                    .Intersect(roleName.Select(r1 => r1.ToUpper()))
                    .Any(),
                u => new
                {
                    Roles = u.Roles.Select(r => r.Permissions),
                }).AsEnumerable();
        }
        public IEnumerable<User> GetUsersInRoleLevel(params string[] roleName)
        {
            return GetAll<User>(u => u.Roles
                    .Select(r => r.RoleLevel.Value)
                    .Intersect(roleName.Select(r1 => r1.ToUpper()))
                    .Any(),
                u => new
                {
                    Roles = u.Roles.Select(r => r.Permissions),
                }).AsEnumerable();
        }

        #endregion


        public EmailSetting GetEmailTemplate(string type, string nameTemplate)
        {
            return Get<EmailSetting>(e => e.TemplateName.ToUpper().Trim() == nameTemplate.ToUpper().Trim() && e.Type.ToUpper().Trim() == type.ToUpper().Trim());
        }

        public IEnumerable<User> GetAllSalesmans()
        {
            return GetAll<User>(u => u.Roles.Any(r => r.RoleLevel == RoleLevel.SalesLevel),
                    u => new
                    {
                        Roles = u.Roles.Select(r => r.Permissions),
                    }).AsEnumerable();
        }

    }
}
