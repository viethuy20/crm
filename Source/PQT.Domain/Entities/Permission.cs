using System;
using System.ComponentModel.DataAnnotations.Schema;
using NS;

namespace PQT.Domain.Entities
{
    public class Permission : EntityBase
    {
        public string Target { get; set; }
        public string Right { get; set; }
        public string Description { get; set; }//Type

        [NotMapped]
        public string DisplayName { get; set; }
        // LINQ predicade
        public static Func<Permission, bool> HasRight(string target, string right)
        {
            return p => p.Target.ToLower() == target.ToLower() &&
                        p.Right.ToLower() == right.ToLower();
        }
        public static Func<Permission, bool> HasRight(string target, string right, string description)
        {
            return p => p.Target.ToLower() == target.ToLower() &&
                        p.Right.ToLower() == right.ToLower() &&
                        p.Description == description;
        }
    }


    public class AreaType : Enumeration
    {
        public static readonly AreaType SettingPermission = New<AreaType>("SettingPermission", "Setting Permission");
        //public static readonly AreaType Admin = New<AreaType>("Admin", "Admin");
        //public static readonly AreaType AdminNew = New<AreaType>("AdminNew", "Admin New");
    }

}
