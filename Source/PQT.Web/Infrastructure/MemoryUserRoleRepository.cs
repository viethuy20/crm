using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using PQT.Domain.Abstract;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;

namespace PQT.Web.Infrastructure
{
    public class MemoryUserRoleRepository : EFRoleBasedAuthorizer
    {
        private List<User> _userRoles = new List<User>();

        #region Factory

        public MemoryUserRoleRepository(DbContext db)
            : base(db)
        {
        }

        #endregion

        #region Decorator Properties

        public EFRoleBasedAuthorizer RoleBasedAuthorizer
        {
            get { return DependencyHelper.GetService<EFRoleBasedAuthorizer>(); }
        }

        #endregion
        public override IEnumerable<Role> GetAllRoles()
        {
            return RoleBasedAuthorizer.GetAllRoles();
        }

        public override Role GetRole(int id)
        {
            return RoleBasedAuthorizer.GetRole(id);
        }

        public override Role GetRoleByName(string name)
        {
            return RoleBasedAuthorizer.GetRoleByName(name);
        }

        public override Role CreateRole(Role role, IEnumerable<int> rolePermissions)
        {
            return RoleBasedAuthorizer.CreateRole(role, rolePermissions);
        }

        public override Role UpdateRole(int id, Role roleInfo, IEnumerable<int> rolePermissions)
        {
            var role = RoleBasedAuthorizer.UpdateRole(id, roleInfo, rolePermissions);
            _userRoles.Clear();
            return role;
        }
        public override void DeleteRole(int id)
        {
            RoleBasedAuthorizer.DeleteRole(id);
            _userRoles.Clear();
        }


        public override Permission CreatePermission(Permission perm)
        {
            return RoleBasedAuthorizer.CreatePermission(perm);
        }

        public override User GetUserPermissions(int userID)
        {
            var userCache = _userRoles.FirstOrDefault(m => m.ID == userID);
            if (userCache != null) return userCache;
            var user = RoleBasedAuthorizer.GetUserPermissions(userID);

            var userNew = new User(user);
            if (_userRoles.All(m => m.ID != userID))
            {
                _userRoles.Add(userNew);
            }
            return userNew;
            //return userNew.Roles.SelectMany(r => r.Permissions).AsEnumerable();
        }

        public override Permission EnsurePermissionRecord(string target, string right, string description = null)
        {
            return RoleBasedAuthorizer.EnsurePermissionRecord(target, right, description);
        }

        public override void AssignRoles(User user, IEnumerable<int> userRoles)
        {
            RoleBasedAuthorizer.AssignRoles(user,userRoles);
            var userCache = _userRoles.FirstOrDefault(m => m.ID == user.ID);
            if (userCache != null) _userRoles.Remove(userCache);
        }

        public override bool CheckAccess(int userID, string feature, string permissionType = null)
        {
            string[] segments = feature.Split(new[] { '.' });
            if (segments.Length < 2) return false;
            string controller = segments[0];
            string action = segments[1];
            return CheckAccess(userID, controller, action, permissionType);
        }

        public override bool CheckAccess(int userID, string controller, string action, string permissionType = null)
        {
            var  user = GetUserPermissions(userID);
            if (user == null) return false;

            if (!string.IsNullOrEmpty(permissionType))
                return user.Roles.SelectMany(r => r.Permissions).Any(Permission.HasRight(controller, action, permissionType));
            return user.Roles.SelectMany(r => r.Permissions).Any(Permission.HasRight(controller, action));
        }
    }
}
