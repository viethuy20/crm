using System;
using System.Collections.Generic;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Abstract
{
    public interface IRoleService : IAuthorizationService
    {
        IEnumerable<Role> GetAllRoles();
        IEnumerable<Role> GetAllRoles(Func<Role, bool> predicate);
        IEnumerable<Role> GetUserRoles(int userID);
        IEnumerable<Role> GetUserRoles(string email);
        Role GetRole(int id);
        Role GetRoleByName(string name);
        IEnumerable<Role> GetAllRoleByLevel(RoleLevel level);
        Role CreateRole(Role roleInfo, IEnumerable<int> rolePermissions);
        Role UpdateRole(int id, Role roleInfo, IEnumerable<int> rolePermissions);
        void DeleteRole(int id);

        IEnumerable<Permission> GetPermissions();
        Permission CreatePermission(Permission perm);
        IEnumerable<Permission> GetUserPermissions(int userID);
        Permission EnsurePermissionRecord(string target, string right, string description = null);
        void AssignRoles(User user, IEnumerable<int> userRoles);
        //void AssignBranches(User user, IEnumerable<int> branches);
    }
}
