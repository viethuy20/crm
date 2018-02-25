using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NS;

namespace PQT.Domain.Enum
{
    public class RoleLevel : Enumeration
    {
        public static readonly RoleLevel None = New<RoleLevel>(0, "None");
        public static readonly RoleLevel AdminLevel = New<RoleLevel>(1, "Admin Level");
        public static readonly RoleLevel ManagerLevel = New<RoleLevel>(2, "Manager Level");
        public static readonly RoleLevel SalesLevel = New<RoleLevel>(3, "Sales Level");
    }
}
