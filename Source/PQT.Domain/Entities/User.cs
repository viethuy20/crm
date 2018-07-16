﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using PQT.Domain.Helpers;
using NS.Entity;
using PQT.Domain.Enum;

namespace PQT.Domain.Entities
{
    public class User : EntityBase
    {
        public User()
        {
            Roles = new HashSet<Role>();
            Status = EntityStatus.Normal;
            UserSalaryHistories= new HashSet<UserSalaryHistory>();

            BusinessDevelopmentUnit = BusinessDevelopmentUnit.None;
            SalesManagementUnit = SalesManagementUnit.None;
            SalesSupervision = SalesSupervision.None;
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

        public int? TransferUserID { get; set; }
        [ForeignKey("TransferUserID")]
        public virtual User TransferUser { get; set; }

        public int NotifyNumber { get; set; }
        public BusinessDevelopmentUnit BusinessDevelopmentUnit { get; set; }
        public SalesManagementUnit SalesManagementUnit { get; set; }
        public SalesSupervision SalesSupervision { get; set; }
        public DateTime? DateOfContract { get; set; }
        public DateTime? DateOfStarting { get; set; }
        public decimal? BasicSalary { get; set; }
        public ICollection<UserSalaryHistory> UserSalaryHistories { get; set; }

        public string Ip { get; set; }
        public string Extension { get; set; }//private number for employees
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
                if (string.IsNullOrEmpty(Picture))
                {
                    return "/content/img/profile.png";
                }
                return "/data/user_img/" + ID + "/" + Picture;
            }
        }
        #endregion

        public string RolesHtml
        {
            get
            {
                if (Roles != null)
                {
                    return string.Join("<br/>", Roles.Select(m => m.Name).OrderBy(m => m).ToArray());
                }
                return "";
            }
        }
    }
}
