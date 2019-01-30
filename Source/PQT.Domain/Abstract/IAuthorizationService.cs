using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Abstract
{
    public interface IAuthorizationService
    {
        bool CheckAccess(int userID, string feature, string permissionType = null);
        bool CheckRole(int userID, string role);
        bool CheckRoleLevel(int userID, RoleLevel roleLevel);
    }
}
