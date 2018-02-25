using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NS.Entity;
using PQT.Domain;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;

namespace PQT.Seed
{
    [TestClass]
    public class DatabaseSeeder
    {
        [TestMethod]
        public void Insert_data_for_test()
        {
            using (var db = new PQTDb())
            {
                db.Database.Delete();
                db.Database.Create();

                Insert_roles_and_users(db);
                Insert_menu(db);
                Insert_Country(db);
                Insert_settings(db);
                db.SaveChanges();
            }
        }
        private void Insert_Country(PQTDb db)
        {
            List<string> cultureList = new List<string>();
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => c.Name.Contains("-"));//CultureInfo.GetCultures(CultureTypes.AllCultures & CultureTypes.NeutralCultures);
            //loop through all the cultures found
            foreach (CultureInfo culture in cultures.Where(m => !string.IsNullOrEmpty(m.Name)))
            {
                try
                {
                    var region = new RegionInfo(culture.LCID);
                    if (!(cultureList.Contains(region.TwoLetterISORegionName)))
                    {
                        cultureList.Add(region.TwoLetterISORegionName);
                        db.Countries.Add(new Country { Code = region.TwoLetterISORegionName, Name = region.EnglishName });
                    }
                }
                catch (Exception)
                {
                }
            }
            db.SaveChanges();
        }
        private void Insert_settings(PQTDb db)
        {
            //db.Settings.Add(new Setting { Module = Enum.GetName(typeof(Setting.ModuleType), Setting.ModuleType.SystemConfig), Name = Enum.GetName(typeof(Setting.ModuleKey.SystemConfig), Setting.ModuleKey.SystemConfig.LoginByEmail), Value = true.ToString(), Type = "Checkbox", Summary = "Use email to login", Note = "", Description = "Check to login using Email instead of DisplayName" });
            db.Settings.Add(new Setting { Module = Enum.GetName(typeof(Setting.ModuleType), Setting.ModuleType.Lead), Name = Enum.GetName(typeof(Setting.ModuleKey.Lead), Setting.ModuleKey.Lead.NumberDaysExpired), Value = "10", Type = "Textbox", Summary = "Number Days Expired", Note = "", Description = "{0} working days rule excluding public holiday in office location" });
            db.Settings.Add(new Setting { Module = Enum.GetName(typeof(Setting.ModuleType), Setting.ModuleType.Lead), Name = Enum.GetName(typeof(Setting.ModuleKey.Lead), Setting.ModuleKey.Lead.MaxBlockeds), Value = "5", Type = "Textbox", Summary = "Maximum Blocks", Note = "", Description = "Maximum number blocks for each salesman" });
            db.Settings.Add(new Setting { Module = Enum.GetName(typeof(Setting.ModuleType), Setting.ModuleType.Lead), Name = Enum.GetName(typeof(Setting.ModuleKey.Lead), Setting.ModuleKey.Lead.MaxLOIs), Value = "5", Type = "Textbox", Summary = "Maximum LOIs", Note = "", Description = "Maximum number Letter of intention(LOI) for each salesman" });

            db.SaveChanges();
        }

        private void Insert_roles_and_users(PQTDb db)
        {

            var rAdmin = new Role { Name = "Admin", RoleLevel = RoleLevel.AdminLevel };
            var sManager = new Role { Name = "Manager", RoleLevel = RoleLevel.ManagerLevel };
            var sQA = new Role { Name = "QA", RoleLevel = RoleLevel.ManagerLevel };
            var salesman = new Role { Name = "Salesman", RoleLevel = RoleLevel.SalesLevel };
            var finance = new Role { Name = "Finance", RoleLevel = RoleLevel.SalesLevel };
            var hR = new Role { Name = "HR", RoleLevel = RoleLevel.SalesLevel };

            db.Users.Add(new User { DisplayName = "ADMIN", Password = EncryptHelper.EncryptPassword("123456"), Email = "ADMIN@LOCALHOST", BusinessPhone = "+84 0168 7040 132", Roles = { rAdmin, salesman } });
            db.Users.Add(new User { DisplayName = "Manager", Password = EncryptHelper.EncryptPassword("123456"), Email = "QA@LOCALHOST", BusinessPhone = "+84 0168 7040 132", Roles = { sQA } });
            db.Users.Add(new User { DisplayName = "Salesman", Password = EncryptHelper.EncryptPassword("123456"), Email = "SALES@LOCALHOST", BusinessPhone = "+84 0168 7040 132", Roles = { salesman } });
            db.Users.Add(new User { DisplayName = "Manager", Password = EncryptHelper.EncryptPassword("123456"), Email = "MANAGER@LOCALHOST", BusinessPhone = "+84 0168 7040 132", Roles = { sManager } });
            db.Users.Add(new User { DisplayName = "Finance", Password = EncryptHelper.EncryptPassword("123456"), Email = "FINANCE@LOCALHOST", BusinessPhone = "+84 0168 7040 132", Roles = { finance } });
            db.Users.Add(new User { DisplayName = "HR", Password = EncryptHelper.EncryptPassword("123456"), Email = "HR@LOCALHOST", BusinessPhone = "+84 0168 7040 132", Roles = { hR } });
            db.SaveChanges();
        }
        private void Insert_menu(PQTDb db)
        {
            Func<string, Role> getRoleByName =
                roleName => db.Roles.FirstOrDefault(r => r.Name == roleName);
            Menu m11 = db.MenuItems.Add(new Menu { Title = "Users", Icon = "fa fa-users" });
            Menu m13 = db.MenuItems.Add(new Menu { Title = "System Settings", Icon = "fa fa-cogs" });
            Menu m12 = db.MenuItems.Add(new Menu { Title = "Master Files", Icon = "fa fa-cubes" });

            db.SaveChanges();


            db.MenuItems.Add(new Menu { Title = "Users", Url = "/Users/Index", ParentID = m11.ID, Icon = "i-multi-agents" });
            db.MenuItems.Add(new Menu { Title = "Deleted Users", Url = "/Users/ListDeletedUsers", ParentID = m11.ID, Icon = "i-multi-agents" });
            db.MenuItems.Add(new Menu { Title = "Roles", Url = "/Roles/Index", ParentID = m11.ID, Icon = "i-role" });

            db.MenuItems.Add(new Menu { Title = "Countries", Url = "/Country/Index", ParentID = m12.ID, Icon = "i-holiday" });
            db.MenuItems.Add(new Menu { Title = "Companies", Url = "/Company/Index", ParentID = m12.ID, Icon = "i-holiday" });

            db.MenuItems.Add(new Menu { Title = "Menus", Url = "/Menus/Index", ParentID = m13.ID, Icon = "i-menu" });
            db.MenuItems.Add(new Menu { Title = "Settings", Url = "/Settings/Index", ParentID = m13.ID, Icon = "i-configuration" });
            db.MenuItems.Add(new Menu { Title = "Email Settings", Url = "/EmailSetting/Index", ParentID = m13.ID, Icon = "i-configuration" });
            db.MenuItems.Add(new Menu { Title = "Logs", Url = "/Audit/Index", ParentID = m13.ID, Icon = "i-log" });
            db.SaveChanges();
        }
    }
}
