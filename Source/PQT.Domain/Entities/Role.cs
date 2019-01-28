using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentValidation.Validators;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class Role : EntityBase
    {
        public Role()
        {
            Permissions = new HashSet<Permission>();
        }

        public Role(Role r)
        {
            Name = r.Name;
            Description = r.Description;
            RoleLevel = r.RoleLevel;
            if (r.Permissions != null)
            {
                Permissions = r.Permissions.Select(p => new Permission(p)).ToList();
            }
            else
            {
                Permissions = new HashSet<Permission>();
            }
        }
        // Primitive properties
        public string Name { get; set; }
        public string Description { get; set; }

        public RoleLevel RoleLevel { get; set; }

        // Navigation properties
        public virtual ICollection<Permission> Permissions { get; set; }

        // LINQ expression methods
        public static Func<Role, bool> HasName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return r => false;

            return r => r.Name.Trim().ToLower() == name.Trim().ToLower();
        }
        public static Func<Role, bool> HasLevel(RoleLevel level)
        {
            if (string.IsNullOrWhiteSpace(level))
                return r => false;

            return r => r.RoleLevel == level;
        }
        public static Func<Role, bool> ContainName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return r => false;

            return r => r.Name.Trim().ToLower().Contains(name.Trim().ToLower());
        }
    }
}
