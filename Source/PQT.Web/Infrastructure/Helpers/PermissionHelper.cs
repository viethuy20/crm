using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
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


        public static void AddOrUpdateAppSettings(string key, string value)
        {
            try
            {
                Configuration webConfigApp = WebConfigurationManager.OpenWebConfiguration("~");
                webConfigApp.AppSettings.Settings[key].Value = value;
                webConfigApp.Save();
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }
    }
}