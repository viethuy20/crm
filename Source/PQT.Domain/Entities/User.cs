using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using PQT.Domain.Helpers;
using NS.Entity;

namespace PQT.Domain.Entities
{
    public class User : EntityBase
    {
        public User()
        {
            Roles = new HashSet<Role>();
            Status = EntityStatus.Normal;
        }

        #region Primitive

        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string BusinessPhone { get; set; }
        public string MobilePhone { get; set; }
        public string Picture { get; set; }
        public DateTime? LastAccess { get; set; }
        public string Address { get; set; }
        public EntityStatus Status { get; set; }

        #endregion

        #region Navigation properties

        public virtual ICollection<Role> Roles { get; set; }
        #endregion

        #region Helpers

        public static Func<User, bool> HasName(string username)
        {
            return u => u.DisplayName.Trim().ToLower() == username.Trim().ToLower();
        }

        public string AvatarUrl
        {
            get
            {
                return UrlHelper.Root + "/data/user_img/" + ID + "/" + Picture;
            }
        }
        #endregion
    }
}
