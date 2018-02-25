using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PQT.Domain.Enum;
using PQT.Web.Infrastructure.Utility;

namespace PQT.Web.Infrastructure.Helpers
{
    public class PermissionHelper
    {
        public static int SalesmanId()
        {
            if (CurrentUser.Identity == null || CurrentUser.HasRoleLevel(RoleLevel.AdminLevel) || CurrentUser.HasRoleLevel(RoleLevel.ManagerLevel))
            {
                return 0;
            }
            return CurrentUser.Identity.ID;
        }
    }
}