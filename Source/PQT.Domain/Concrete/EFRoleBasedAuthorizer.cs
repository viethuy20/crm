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

        public EFRoleBasedAuthorizer(DbContext db)
            : base(db)
        {
        }

        #region IRoleService Members

        public virtual IEnumerable<Role> GetAllRoles()
        {
            return GetAll<Role>(u => new
            {
                u.Permissions,
            }).AsEnumerable();
        }

        public virtual Role GetRole(int id)
        {
            return Get<Role>(r => r.ID == id,
                             r => new { r.Permissions });
        }

        public virtual Role GetRoleByName(string name)
        {
            return Get(Role.HasName(name));
        }

        public virtual Role CreateRole(Role role, IEnumerable<int> rolePermissions)
        {
            role.Permissions = GetAll<Permission>(p => rolePermissions.Contains(p.ID)).ToList();
            return Create(role);
        }

        public virtual Role UpdateRole(int id, Role roleInfo, IEnumerable<int> rolePermissions)
        {
            var role = Get<Role>(id);
            role.Name = roleInfo.Name;
            role.Description = roleInfo.Description;
            role.RoleLevel = roleInfo.RoleLevel;
            role.Permissions.Clear();
            role.Permissions = GetAll<Permission>(p => rolePermissions.Contains(p.ID)).ToList();
            Update(role);
            return role;
        }

        public virtual void DeleteRole(int id)
        {
            Delete<Role>(id);
        }

        public virtual Permission CreatePermission(Permission perm)
        {
            return Create(perm);
        }

        public virtual User GetUserPermissions(int userID)
        {
            var user = Get<User>(u => u.ID == userID, u => new
            {
                Roles = u.Roles.Select(r => r.Permissions),
            });
            if (user == null)
                return new User();
            return user;
        }

        public virtual void AssignRoles(User user, IEnumerable<int> userRoles)
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
        }
        public virtual Permission EnsurePermissionRecord(string target, string right, string description = null)
        {
            return Get<Permission>(p => p.Target.ToLower() == target.ToLower() &&
                                        p.Right.ToLower() == right.ToLower() &&
                                        (string.IsNullOrEmpty(description) || p.Description == description))
                   ?? CreatePermission(new Permission { Target = target, Right = right, Description = description });
        }

        public virtual bool CheckAccess(int userID, string feature, string permissionType = null)
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
            var userPermissions = GetUserPermissions(userID);
            if (!string.IsNullOrEmpty(permissionType))
                return userPermissions.Roles.SelectMany(r => r.Permissions).Any(Permission.HasRight(controller, action, permissionType));
            return userPermissions.Roles.SelectMany(r => r.Permissions).Any(Permission.HasRight(controller, action));
        }

        public virtual bool CheckRole(int userID, string role)
        {
            var user = GetUserPermissions(userID);
            return user.Roles.Any(Role.HasName(role));
        }

        public virtual bool CheckRoleLevel(int userID, RoleLevel roleLevel)
        {
            var user = GetUserPermissions(userID);
            return user.Roles.Any(Role.HasLevel(roleLevel));
        }
    }
}
