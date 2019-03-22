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

        //public string GetTempUserNo()
        //{
        //    return string.Format("EMP{0}", GetNextTempCounter("User", 1).ToString("D3"));
        //}
        public int GetCountUsers(Func<User, bool> predicate)
        {
            if (predicate != null)
            {
                return _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(predicate).Count();
            }
            return _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).Where(m => m.Status == EntityUserStatus.Normal).Count();
        }
        public IEnumerable<User> GetUsers(Func<User, bool> predicate, string sortColumnDir, Func<User, object> orderBy, int page, int pageSize)
        {
            if (predicate != null)
            {
                if (sortColumnDir == "asc")
                {
                    return _db.Set<User>()
                        .Include(m => m.DirectSupervisor)
                        .Include(m => m.Roles.Select(r => r.Permissions))
                        .Where(predicate).OrderBy(orderBy).ThenByDescending(s => s.ID).Skip(page)
                        .Take(pageSize).AsEnumerable();
                }
                return _db.Set<User>()
                    .Include(m => m.DirectSupervisor)
                    .Include(m => m.Roles.Select(r => r.Permissions))
                    .Where(predicate).OrderByDescending(orderBy).ThenByDescending(s => s.ID).Skip(page)
                    .Take(pageSize).AsEnumerable();
            }
            if (sortColumnDir == "asc")
            {
                return _db.Set<User>()
                    .Include(m => m.DirectSupervisor)
                    .Include(m => m.Roles.Select(r => r.Permissions))
                    .OrderBy(orderBy).ThenByDescending(s => s.ID).Skip(page)
                    .Take(pageSize).AsEnumerable();
            }
            return _db.Set<User>()
                .Include(m => m.DirectSupervisor)
                .Include(m => m.Roles.Select(r => r.Permissions))
                .OrderByDescending(orderBy).ThenByDescending(s => s.ID).Skip(page)
                .Take(pageSize).AsEnumerable();
        }
        public IEnumerable<User> GetUsers(Func<User, bool> predicate = null)
        {
            if (predicate == null)
            {
                return _db.Set<User>().AsEnumerable();
            }
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

        public bool UpdateUserIncludeCollection(User userInfo)
        {
            return TransactionWrapper.Do(() =>
            {
                var itemExist = Get<User>(m => m.ID == userInfo.ID, m => m.UserContracts);
                Update(userInfo);
                if (userInfo.UserContracts != null && userInfo.UserContracts.Any())
                {
                    foreach (var photo in itemExist.UserContracts.Where(m => !userInfo.UserContracts.Select(n => n.ID).Contains(m.ID)).ToList())
                    {
                        itemExist.UserContracts.Remove(photo);
                        Delete(photo);
                    }
                    UpdateCollection(userInfo, m => m.ID == userInfo.ID, m => m.UserContracts, m => m.ID);
                }
                else if (itemExist.UserContracts != null)
                    foreach (var photo in itemExist.UserContracts.ToList())
                    {
                        itemExist.UserContracts.Remove(photo);
                        Delete(photo);
                    }
                Update(itemExist);
                return true;
            });
        }

        public virtual User GetUser(int id)
        {
            return _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions)).FirstOrDefault(u => u.ID == id);
        }
        public User GetUserIncludeAll(int id)
        {
            return Get<User>(u => u.ID == id, u => new
            {
                u.UserSalaryHistories,
                u.UserContracts,
                u.OfficeLocation,
                Roles = u.Roles.Select(r => r.Permissions),
            });
            //return _db.Set<User>()
            //    .Include(m => m.Roles)
            //    .Include(m => m.Roles.Select(r => r.Permissions))
            //    .Include(m => m.UserSalaryHistories)
            //    .Include(m => m.UserContracts)
            //    .Include(m => m.OfficeLocation)
            //    .FirstOrDefault(u => u.ID == id);
        }

        public User GetUserByNo(string userNo)
        {
             if(string.IsNullOrWhiteSpace(userNo))
                 return null;
            var number = userNo.Trim().ToUpper();
            return _db.Set<User>()
                .FirstOrDefault(u => u.Status.Value == EntityStatus.Normal.Value && u.UserNo != null &&
                                     u.UserNo.Trim().ToUpper() == number);
        }
        public User GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;
            var number = email.Trim().ToLower();
            return _db.Set<User>()
                .FirstOrDefault(u => u.Status.Value == EntityStatus.Normal.Value && u.Email != null &&
                                     u.Email.Trim().ToLower() == number);
        }
        public User ValidateLogin(string email, string password)
        {
            var emaiLower = email.Trim().ToLower();
            return _db.Set<User>()
                .FirstOrDefault(u => u.Email != null &&
                                     u.Email.Trim().ToLower() == emaiLower &&
                                     u.Password == password &&
                                     u.UserStatus.Value == UserStatus.Live.Value &&
                                     u.Status.Value == EntityUserStatus.Normal.Value);
        }

        public User CreateUser(User user)
        {
            if (user.Email != null)
                user.Email = user.Email.Trim();

            //var tempNo = GetTempUserNo();
            //if (tempNo == user.UserNo)
            //{
            user.UserNo = string.Format("EMP{0}", GetNextCounter("User", 1).ToString("D3"));
            //}
            //else
            //{
            //    SetCounter("User", user.UserNo);
            //}
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
                    OfficeLocation = u.OfficeLocation,
                    DirectSupervisor = u.DirectSupervisor,
                    UserContracts = u.UserContracts,
                    UserSalaryHistories = u.UserSalaryHistories,
                }).AsEnumerable();
        }
        public IEnumerable<User> GetUsersInRoleLevel(params string[] roleName)
        {
            return GetAll<User>(u => u.Status == EntityUserStatus.Normal &&
                                     u.UserStatus == UserStatus.Live &&
                                     u.Roles
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
            return _db.Set<User>()
                .Include(m => m.Roles.Select(r => r.Permissions))
                .Where(u => u.Status.Value == EntityUserStatus.Normal.Value &&
                            u.UserStatus.Value == UserStatus.Live.Value &&
                            u.Roles.Any(r => r.RoleLevel.Value == RoleLevel.SalesLevel.Value)).AsEnumerable();
        }
        public IEnumerable<User> GetAllSupervisors()
        {
            return _db.Set<User>()
                .Where(u => u.DirectSupervisorID != null && 
                u.Status.Value == EntityUserStatus.Normal.Value && 
                u.UserStatus.Value == UserStatus.Live.Value).Select(m => m.DirectSupervisor).Distinct().AsEnumerable();
        }

    }
}
