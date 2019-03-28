using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using NS;
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
        public int GetCountUsers(string searchValue, bool isHrUser)
        {
            IQueryable<User> queries = null;
            if (isHrUser)
            {
                queries = _db.Set<User>().Where(m => m.UserStatus.Value == UserStatus.Live.Value &&
                                                     (m.Status.Value == EntityUserStatus.Normal.Value ||
                                                      m.Status.Value == EntityUserStatus.ApprovedEmployment.Value));
            }
            else
                queries = _db.Set<User>().Where(m => m.Status.Value == EntityUserStatus.Normal.Value ||
                                                      m.Status.Value == EntityUserStatus.ApprovedEmployment.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m =>
                    m.EmploymentDate == dtSearch ||
                    m.EmploymentEndDate == dtSearch
                    );
                else
                {
                    var searchUserStatus = Enumeration.GetAll<UserStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m => m.DisplayName.ToLower().Contains(searchValue) ||
                                                 searchUserStatus.Contains(m.UserStatus.Value) ||
                                                 (m.DirectSupervisor != null && m.DirectSupervisor.DisplayName.ToLower()
                                                      .Contains(searchValue)) ||
                                                 (m.UserNo.ToLower().Contains(searchValue)) ||
                                                 (m.Email != null && m.Email.Contains(searchValue)) ||
                                                 (m.PersonalEmail != null && m.PersonalEmail.Contains(searchValue)) ||
                                                 (m.BusinessPhone != null && m.BusinessPhone.Contains(searchValue)) ||
                                                 (m.MobilePhone != null && m.MobilePhone.Contains(searchValue)) ||
                                                 (m.Roles.Any(r => r.Name.ToLower().Contains(searchValue))));
                }
            }
            return queries
                .Include(m => m.DirectSupervisor)
                .Include(m => m.Roles).Count();
        }
        public IEnumerable<User> GetUsers(string searchValue, bool isHrUser, string sortColumnDir, string sortColumn, int page, int pageSize)
        {

            IQueryable<User> queries = null;
            if (isHrUser)
            {
                queries = _db.Set<User>().Where(m => m.UserStatus.Value == UserStatus.Live.Value &&
                                                     (m.Status.Value == EntityUserStatus.Normal.Value ||
                                                      m.Status.Value == EntityUserStatus.ApprovedEmployment.Value));
            }
            else
                queries = _db.Set<User>().Where(m => m.Status.Value == EntityUserStatus.Normal.Value ||
                                                      m.Status.Value == EntityUserStatus.ApprovedEmployment.Value);
            if (!string.IsNullOrEmpty(searchValue))
            {
                bool isValid = DateTime.TryParseExact(
                    searchValue, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dtSearch);
                if (isValid)
                    queries = queries.Where(m =>
                    m.EmploymentDate == dtSearch ||
                    m.EmploymentEndDate == dtSearch
                    );
                else
                {
                    var searchUserStatus = Enumeration.GetAll<UserStatus>()
                        .Where(m => m.DisplayName.ToLower().Contains(searchValue)).Select(m => m.Value).ToArray();
                    queries = queries.Where(m => m.DisplayName.ToLower().Contains(searchValue) ||
                                                 searchUserStatus.Contains(m.UserStatus.Value) ||
                                                 (m.DirectSupervisor != null && m.DirectSupervisor.DisplayName.ToLower()
                                                      .Contains(searchValue)) ||
                                                 (m.UserNo.ToLower().Contains(searchValue)) ||
                                                 (m.Email != null && m.Email.Contains(searchValue)) ||
                                                 (m.PersonalEmail != null && m.PersonalEmail.Contains(searchValue)) ||
                                                 (m.BusinessPhone != null && m.BusinessPhone.Contains(searchValue)) ||
                                                 (m.MobilePhone != null && m.MobilePhone.Contains(searchValue)) ||
                                                 (m.Roles.Any(r => r.Name.ToLower().Contains(searchValue))));
                }
            }

            switch (sortColumn)
            {
                case "UserNo":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.UserNo)
                        : queries.OrderByDescending(s => s.UserNo);
                    break;
                case "DisplayName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.DisplayName)
                        : queries.OrderByDescending(s => s.DisplayName);
                    break;
                case "FirstName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.FirstName)
                        : queries.OrderByDescending(s => s.FirstName);
                    break;
                case "LastName":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.LastName)
                        : queries.OrderByDescending(s => s.LastName);
                    break;
                case "Email":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Email)
                        : queries.OrderByDescending(s => s.Email);
                    break;
                case "PersonalEmail":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.PersonalEmail)
                        : queries.OrderByDescending(s => s.PersonalEmail);
                    break;
                case "UserStatus":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.UserStatus.Value)
                        : queries.OrderByDescending(s => s.UserStatus.Value);
                    break;
                case "DirectSupervisor":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.DirectSupervisor.DisplayName)
                        : queries.OrderByDescending(s => s.DirectSupervisor.DisplayName);
                    break;
                case "BusinessPhone":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.BusinessPhone)
                        : queries.OrderByDescending(s => s.BusinessPhone);
                    break;
                case "MobilePhone":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.MobilePhone)
                        : queries.OrderByDescending(s => s.MobilePhone);
                    break;
                case "Extension":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Extension)
                        : queries.OrderByDescending(s => s.Extension);
                    break;
                case "RolesHtml":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.Roles.Select(m => m.Name).FirstOrDefault())
                        : queries.OrderByDescending(s => s.Roles.Select(m => m.Name).FirstOrDefault());
                    break;
                case "DateOfBirth":
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.DateOfBirth)
                        : queries.OrderByDescending(s => s.DateOfBirth);
                    break;
                default:
                    queries = sortColumnDir == "asc"
                        ? queries.OrderBy(s => s.ID)
                        : queries.OrderByDescending(s => s.ID);
                    break;
            }
            return queries.Skip(page).Take(pageSize)
                .Include(m => m.DirectSupervisor)
                .Include(m => m.OfficeLocation)
                .Include(m => m.Roles)
                .ToList();
        }
        public IEnumerable<User> GetUsersDeleted()
        {
            return _db.Set<User>().Include(m => m.Roles.Select(r => r.Permissions))
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
            if (string.IsNullOrWhiteSpace(userNo))
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
        #endregion


        public EmailSetting GetEmailTemplate(string type, string nameTemplate)
        {
            return Get<EmailSetting>(e => e.TemplateName.ToUpper().Trim() == nameTemplate.ToUpper().Trim() && e.Type.ToUpper().Trim() == type.ToUpper().Trim());
        }

        public IEnumerable<User> GetAllUsersByLive()
        {
            return _db.Set<User>()
                .Where(u => u.Status.Value == EntityUserStatus.ApprovedEmployment.Value &&
                            u.UserStatus.Value == UserStatus.Live.Value).AsEnumerable();
        }
        public IEnumerable<User> GetAllUsersForLeave(int supervisorId)
        {
            return _db.Set<User>().Where(m => m.UserStatus.Value == UserStatus.Live.Value &&
                            m.Status.Value != EntityUserStatus.Deleted.Value &&
                            m.DirectSupervisorID == supervisorId).AsEnumerable();
        }
        public IEnumerable<User> GetAllSalesmans()
        {
            return _db.Set<User>().Include(m => m.Roles)
                .Where(u => u.Status.Value == EntityUserStatus.Normal.Value &&
                            u.UserStatus.Value == UserStatus.Live.Value &&
                            u.Roles.Any(r => r.RoleLevel.Value == RoleLevel.SalesLevel.Value)).AsEnumerable();
        }
        public IEnumerable<User> GetPossibleUsers(string roleName, string searchValue, bool isSalesUser, User currentUser)
        {
            roleName = roleName.ToUpper();
            IQueryable<User> queries = _db.Set<User>().Include(m => m.Roles)
                .Where(m => m.Roles.Any(r => r.Name.ToUpper().Contains(roleName)));
            if (!string.IsNullOrEmpty(searchValue))
            {
                searchValue = searchValue.ToLower();
                queries = queries.Where(m =>
                    (m.DisplayName != null &&
                     m.DisplayName.ToLower().Contains(searchValue)) ||
                    (m.Email != null && m.Email.ToLower().Contains(searchValue)));
            }
            if (isSalesUser)
            {
                if (currentUser != null &&
                    (currentUser.BusinessDevelopmentUnit.Value != BusinessDevelopmentUnit.None.Value ||
                    currentUser.SalesManagementUnit.Value != SalesManagementUnit.None.Value))
                {
                    queries = queries.Where(m => m.DirectSupervisorID == currentUser.ID);
                    return queries.AsEnumerable();
                }
                return new List<User>();
            }
            return queries.AsEnumerable();
        }
        public IEnumerable<User> GetAllSupervisors()
        {
            return _db.Set<User>()
                .Where(m => m.Status.Value != EntityStatus.Deleted.Value &&
                            (m.FinanceAdminUnit.Value != FinanceAdminUnit.None.Value ||
                             m.SalesManagementUnit.Value != SalesManagementUnit.None.Value ||
                             m.HumanResourceUnit.Value == HumanResourceUnit.Coordinator.Value ||
                             m.ProjectManagementUnit.Value != ProjectManagementUnit.None.Value)).AsEnumerable();
        }

        public IEnumerable<Role> GetRolesByName(string[] rolesName)
        {
            rolesName = rolesName.Select(m => m.ToLower()).ToArray();
            return _db.Set<Role>().Where(m => rolesName.Contains(m.Name.ToLower())).AsEnumerable();
        }
        public IEnumerable<User> GetAllInterviewers(string[] rolesReviews)
        {
            var rolesIds = GetRolesByName(rolesReviews).Select(m => m.ID);
            return _db.Set<User>().Include(m => m.Roles)
                .Where(m => m.Status.Value != EntityStatus.Deleted.Value && (
                                m.HumanResourceUnit.Value == HumanResourceUnit.Coordinator.Value ||
                                m.SalesManagementUnit.Value != SalesManagementUnit.None.Value ||
                                m.FinanceAdminUnit.Value == FinanceAdminUnit.Manager.Value ||
                                m.ProjectManagementUnit.Value != ProjectManagementUnit.None.Value ||
                                m.Roles.Select(r => r.ID).Any(r => rolesIds.Contains(r)))).AsEnumerable();
        }
        public IEnumerable<User> GetAllSupervisorsAssigned()
        {
            return _db.Set<User>()
                .Where(u => u.DirectSupervisorID != null &&
                u.Status.Value == EntityUserStatus.Normal.Value &&
                u.UserStatus.Value == UserStatus.Live.Value).Select(m => m.DirectSupervisor).Distinct().AsEnumerable();
        }

    }
}
