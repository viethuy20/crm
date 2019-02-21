using System;
using System.Collections.Generic;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Abstract
{
    public interface IRoleService : IAuthorizationService
    {
        IEnumerable<Role> GetAllRoles();
        Role GetRole(int id);
        Role GetRoleByName(string name);
        Role CreateRole(Role roleInfo, IEnumerable<int> rolePermissions);
        Role UpdateRole(int id, Role roleInfo, IEnumerable<int> rolePermissions);
        void DeleteRole(int id);
        void AssignRoles(User user, IEnumerable<int> userRoles);
        Permission EnsurePermissionRecord(string target, string right, string description = null);

        void RemoveCacheUserRole(int id);
    }
}
