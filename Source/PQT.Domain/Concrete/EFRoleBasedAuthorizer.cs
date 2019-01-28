using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using PQT.Domain;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Concrete
{
    public class EFRoleBasedAuthorizer : Repository, IRoleService
    {
        public static readonly List<User> UserRoleRecords = new List<User>();

        public EFRoleBasedAuthorizer(DbContext db)
            : base(db)
        {
        }

        #region IRoleService Members

        public IEnumerable<Role> GetAllRoles()
        {
            return GetAll<Role>(r => r.Permissions).AsEnumerable();
        }

        public Role GetRole(int id)
        {
            return Get<Role>(r => r.ID == id,
                             r => r.Permissions);
        }

        public Role GetRoleByName(string name)
        {
            return Get(Role.HasName(name));
        }

        public Role CreateRole(Role role, IEnumerable<int> rolePermissions)
        {
            role.Permissions = GetAll<Permission>(p => rolePermissions.Contains(p.ID)).ToList();
            return Create(role);
        }

        public Role UpdateRole(int id, Role roleInfo, IEnumerable<int> rolePermissions)
        {
            var role = Get<Role>(id);
            role.Name = roleInfo.Name;
            role.Description = roleInfo.Description;
            role.RoleLevel = roleInfo.RoleLevel;
            role.Permissions.Clear();
            role.Permissions = GetAll<Permission>(p => rolePermissions.Contains(p.ID)).ToList();
            Update(role);

            UserRoleRecords.Clear();
            return role;
        }

        public void DeleteRole(int id)
        {
            Delete<Role>(id);
            UserRoleRecords.Clear();
        }

        public virtual Permission CreatePermission(Permission perm)
        {
            return Create(perm);
        }

        public IEnumerable<Permission> GetUserPermissions(int userID)
        {
            var userCache = UserRoleRecords.FirstOrDefault(m => m.ID == userID);
            if (userCache != null) return userCache.Roles.SelectMany(r => r.Permissions).AsEnumerable();
            var user = Get<User>(u => u.ID == userID, u => new
            {
                Roles = u.Roles.Select(r => r.Permissions),
            });
            if (user == null)
                return new List<Permission>();
            if (UserRoleRecords.All(m => m.ID != userID))
                UserRoleRecords.Add(new User(user));
            return user.Roles.SelectMany(r => r.Permissions).AsEnumerable();
        }

        public void AssignRoles(User user, IEnumerable<int> userRoles)
        {
            var userExist = Get<User>(u => u.ID == user.ID, u => new
            {
                Roles = u.Roles.Select(r => r.Permissions),
            });
            if (user == null) return;
            userExist.Roles.Clear();
            userExist.Roles = GetAll<Role>(r => userRoles.Contains(r.ID)).ToList();
            user.Roles.Clear();
            user.Roles = GetAll<Role>(r => userRoles.Contains(r.ID)).ToList();
            var tmp = Update(userExist);

            var userCache = UserRoleRecords.FirstOrDefault(m => m.ID == user.ID);
            if (userCache != null) UserRoleRecords.Remove(userCache);
            //_db.SaveChanges();
        }
        public Permission EnsurePermissionRecord(string target, string right, string description = null)
        {
            return Get<Permission>(p => p.Target.ToLower() == target.ToLower() &&
                                        p.Right.ToLower() == right.ToLower() &&
                                        (string.IsNullOrEmpty(description) || p.Description == description))
                   ?? CreatePermission(new Permission { Target = target, Right = right, Description = description });
        }

        public bool CheckAccess(int userID, string feature, string permissionType = null)
        {
            string[] segments = feature.Split(new[] { '.' });
            if (segments.Length < 2) return false;
            string controller = segments[0];
            string action = segments[1];
            return CheckAccess(userID, controller, action, permissionType);
        }

        #endregion

        public virtual bool CheckAccess(int userID, string controller, string action, string permissionType = null)
        {
            IEnumerable<Permission> userPermissions = GetUserPermissions(userID);
            if (!string.IsNullOrEmpty(permissionType))
                return userPermissions.Any(Permission.HasRight(controller, action, permissionType));
            return userPermissions.Any(Permission.HasRight(controller, action));
        }
    }
}
