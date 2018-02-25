using System.Collections.Generic;
using System.Linq;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using NS.Mvc;

namespace PQT.Web.Models
{
    public class RoleModel
    {
        public const string PermissionNameAdminPrefix = "CheckBox.Permission.Admin";

        public Role Role { get; set; }
        public IDictionary<string, IList<Permission>> PermissionAdmins { get; set; }
    }
}
