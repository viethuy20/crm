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
using PQT.Domain.Helpers.Encryption;

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
                Insert_Countries(db);
                Insert_Companies(db);
                Insert_Trainers(db);
                Insert_Events(db);
                Insert_menu(db);
                Insert_settings(db);
                db.SaveChanges();
            }
        }

        private void Insert_Countries(PQTDb db)
        {
            db.Countries.Add(new Country { Name = "Algeria", DialingCode = "213", Code = "AL" });
            db.Countries.Add(new Country { Name = "Angola", DialingCode = "244", Code = "AN" });
            db.Countries.Add(new Country { Name = "Benin", DialingCode = "229", Code = "BE" });
            db.Countries.Add(new Country { Name = "Botswana", DialingCode = "267", Code = "BT" });
            db.Countries.Add(new Country { Name = "Burkina Faso (Upper Volta)", DialingCode = "226", Code = "BF" });
            db.Countries.Add(new Country { Name = "Burundi", DialingCode = "257", Code = "BR" });
            db.Countries.Add(new Country { Name = "Cameroon", DialingCode = "237", Code = "CR" });
            db.Countries.Add(new Country { Name = "Cape Verde", DialingCode = "238", Code = "CV" });
            db.Countries.Add(new Country { Name = "Central African Republic", DialingCode = "236", Code = "CA" });
            db.Countries.Add(new Country { Name = "Democratic Republic of Congo", DialingCode = "243", Code = "DRC" });
            db.Countries.Add(new Country { Name = "Egypt", DialingCode = "20", Code = "EG" });
            db.Countries.Add(new Country { Name = "Eritrea", DialingCode = "291", Code = "ER" });
            db.Countries.Add(new Country { Name = "Ethiopia", DialingCode = "251", Code = "ET" });
            db.Countries.Add(new Country { Name = "Gabon", DialingCode = "241", Code = "GB" });
            db.Countries.Add(new Country { Name = "Gambia", DialingCode = "220", Code = "GA" });
            db.Countries.Add(new Country { Name = "Ghana", DialingCode = "233", Code = "GH" });
            db.Countries.Add(new Country { Name = "Guinea", DialingCode = "224", Code = "GU" });
            db.Countries.Add(new Country { Name = "Ivory Coast", DialingCode = "225", Code = "IC" });
            db.Countries.Add(new Country { Name = "Kenya", DialingCode = "254", Code = "KY" });
            db.Countries.Add(new Country { Name = "Lesotho", DialingCode = "266", Code = "LE" });
            db.Countries.Add(new Country { Name = "Liberia", DialingCode = "231", Code = "LI" });
            db.Countries.Add(new Country { Name = "Libya", DialingCode = "218", Code = "LB" });
            db.Countries.Add(new Country { Name = "Madagascar", DialingCode = "261", Code = "MD" });
            db.Countries.Add(new Country { Name = "Malawi", DialingCode = "265", Code = "MW" });
            db.Countries.Add(new Country { Name = "Mali", DialingCode = "223", Code = "ML" });
            db.Countries.Add(new Country { Name = "Mauritania", DialingCode = "222", Code = "MU" });
            db.Countries.Add(new Country { Name = "Mauritius", DialingCode = "230", Code = "MA" });
            db.Countries.Add(new Country { Name = "Morocco", DialingCode = "212", Code = "MR" });
            db.Countries.Add(new Country { Name = "Mozambique", DialingCode = "258", Code = "MZ" });
            db.Countries.Add(new Country { Name = "Namibia", DialingCode = "264", Code = "NM" });
            db.Countries.Add(new Country { Name = "Niger", DialingCode = "227", Code = "NG" });
            db.Countries.Add(new Country { Name = "Nigeria", DialingCode = "234", Code = "NI" });
            db.Countries.Add(new Country { Name = "Republic of Chad", DialingCode = "235", Code = "CH" });
            db.Countries.Add(new Country { Name = "Republic of Congo", DialingCode = "242", Code = "CG" });
            db.Countries.Add(new Country { Name = "Republic of Djibouti", DialingCode = "253", Code = "RD" });
            db.Countries.Add(new Country { Name = "Republic of Equatorial Guinea", DialingCode = "240", Code = "RE" });
            db.Countries.Add(new Country { Name = "Republic of Guinea-Bissau", DialingCode = "245", Code = "RG" });
            db.Countries.Add(new Country { Name = "Republic of Sao Tome and Principe", DialingCode = "239", Code = "RS" });
            db.Countries.Add(new Country { Name = "Republic of Seychelles", DialingCode = "248", Code = "SY" });
            db.Countries.Add(new Country { Name = "Rwanda", DialingCode = "250", Code = "RW" });
            db.Countries.Add(new Country { Name = "Senegal", DialingCode = "221", Code = "SN" });
            db.Countries.Add(new Country { Name = "Sierra Leone", DialingCode = "232", Code = "SL" });
            db.Countries.Add(new Country { Name = "Somalia", DialingCode = "252", Code = "SO" });
            db.Countries.Add(new Country { Name = "South Africa", DialingCode = "27", Code = "SA" });
            db.Countries.Add(new Country { Name = "South Sudan", DialingCode = "211", Code = "SS" });
            db.Countries.Add(new Country { Name = "Sudan", DialingCode = "249", Code = "SU" });
            db.Countries.Add(new Country { Name = "Swaziland", DialingCode = "268", Code = "SW" });
            db.Countries.Add(new Country { Name = "Tanzania", DialingCode = "255", Code = "TA" });
            db.Countries.Add(new Country { Name = "Togo", DialingCode = "228", Code = "TO" });
            db.Countries.Add(new Country { Name = "Tunisia", DialingCode = "216", Code = "TN" });
            db.Countries.Add(new Country { Name = "Uganda", DialingCode = "256", Code = "UG" });
            db.Countries.Add(new Country { Name = "Union of the Comoros", DialingCode = "269", Code = "UC" });
            db.Countries.Add(new Country { Name = "Zaire", DialingCode = "243", Code = "ZA" });
            db.Countries.Add(new Country { Name = "Zambia", DialingCode = "260", Code = "ZM" });
            db.Countries.Add(new Country { Name = "Zimbabwe", DialingCode = "263", Code = "ZI" });

            db.SaveChanges();
        }

        private void Insert_Companies(PQTDb db)
        {
            for (int i = 1; i <= 10; i++)
            {
                db.Companies.Add(new Company
                {
                    CountryID = i,
                    CompanyName = "Company TEST " + i
                });
            }
            db.SaveChanges();
        }
        private void Insert_Trainers(PQTDb db)
        {
            for (int i = 1; i <= 5; i++)
            {
                db.Trainers.Add(new Trainer()
                {
                    Name = "Trainer TEST " + i,
                    Passport = "23414 " + i,
                    Email = "email " + i + "@gmail.com",
                    BusinessPhone = "134124124" + i,
                    Address = "Address 32414214" + i
                });
            }
            db.SaveChanges();
        }
        private void Insert_Events(PQTDb db)
        {
            for (int i = 1; i <= 5; i++)
            {
                var rand = new Random((int) DateTime.Now.Ticks).Next(0x1000000);
                db.Events.Add(new Event
                {
                    EventCode = string.Format("{0:X6}", rand),
                    EventName = "Event TEST " + i,
                    ShortDescription = "A Singapore based industry expert, with a solid track record of operational excellence, innovation and solutions provided in Africa the world’s fastest growing continent.",
                    Description = "We provide business intelligences, education and consulting services to the world. Together with international experts, customers, governments and communities, we help companies thrive by applying our insights and over 20 years of experience. We have over 06 operating offices, over 200 employees, and agents in 15 countries who are committed to train the corporate world in a responsible way, reducing process waste, improving business environment and the communities, and economy where we live and work. We are also a dynamic and rapidly-growing business with a commitment to excellence, offering promising opportunities for career development.",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(30),
                    UserID = 1,
                    BackgroundColor = string.Format("#{0:X6}", rand)
            });
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
        var finance = new Role { Name = "Finance", RoleLevel = RoleLevel.ManagerLevel };
        var hR = new Role { Name = "HR", RoleLevel = RoleLevel.ManagerLevel };

        db.Users.Add(new User { DisplayName = "ADMIN", Password = EncryptHelper.EncryptPassword("123456"), Email = "ADMIN@LOCALHOST", BusinessPhone = "+84 0168 7040 132", Roles = { rAdmin, salesman } });
        db.Users.Add(new User { DisplayName = "Manager", Password = EncryptHelper.EncryptPassword("123456"), Email = "QA@LOCALHOST", BusinessPhone = "+84 0168 7040 132", Roles = { sQA } });
        db.Users.Add(new User { DisplayName = "Salesman1", Password = EncryptHelper.EncryptPassword("123456"), Email = "SALES1@LOCALHOST", BusinessPhone = "+84 0168 7040 132", Roles = { salesman } });
        db.Users.Add(new User { DisplayName = "Salesman2", Password = EncryptHelper.EncryptPassword("123456"), Email = "SALES2@LOCALHOST", BusinessPhone = "+84 0168 7040 132", Roles = { salesman } });
        db.Users.Add(new User { DisplayName = "Salesman3", Password = EncryptHelper.EncryptPassword("123456"), Email = "SALES3@LOCALHOST", BusinessPhone = "+84 0168 7040 132", Roles = { salesman } });
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
        db.MenuItems.Add(new Menu { Title = "Sales Group", Url = "/SalesGroup/Index", ParentID = m11.ID, Icon = "i-role" });

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
