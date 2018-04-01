using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;
using PQT.Domain.Helpers;
using Humanizer;
using NS.Entity;
using ServiceStack;
using WebGrease.Css;

namespace PQT.Domain.Concrete
{
    public class EFUnitRepository : Repository, IUnitRepository
    {
        public EFUnitRepository(DbContext db)
            : base(db)
        {
        }

        #region Country

        public IEnumerable<Country> GetAllCountries()
        {
            return GetAll<Country>().AsEnumerable();
        }

        public Country GetCountry(int countryID)
        {
            return Get<Country>(countryID);
        }

        public Country GetCountry(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;
            return Get<Country>(m => m.Code.Trim().ToUpper() == code.Trim().ToUpper());
        }

        public Country CreateCountry(Country info)
        {
            return Create(info);
        }

        public bool UpdateCountry(Country info)
        {
            var country = Get<Country>(info.ID);
            if (country == null) return false;

            country.Name = info.Name;
            country.Code = info.Code;

            return Update(country);
        }

        public bool DeleteCountry(int countryID)
        {
            return Delete<Country>(countryID);
        }


        #endregion

        #region Counter

        public int GetCounter(string name)
        {
            return Get<Counter>(c => c.Name == name).Value;
        }

        public Counter SetCounter(string name, int value)
        {
            Counter counter = new Counter()
            {
                Name = name,
                Value = value
            };
            return Create(counter);
        }

        public IEnumerable<Counter> GetAllCounter()
        {
            return GetAll<Counter>();
        }

        public Counter GetCounter(int id)
        {
            return Get<Counter>(id);
        }

        public bool UpdateCounter(Counter counter)
        {
            return Update(counter);
        }

        #endregion

        #region Email Template Settings

        //[RequestCache]
        public EmailSetting GetEmailSetting(string type, string nameTemplate)
        {
            if (string.IsNullOrEmpty(nameTemplate) || string.IsNullOrEmpty(type))
            {
                return null;
            }
            return Get<EmailSetting>(e => e.TemplateName.ToUpper().Trim() == nameTemplate.ToUpper().Trim() && e.Type.ToUpper().Trim() == type.ToUpper().Trim());
        }
        public bool CreateEmailSetting(EmailSetting email)
        {
            if (email.ID > 0)
            {
                //var emailExist = Single<EmailSetting>(m => m.ID == email.ID);

                //emailExist.ReceiveRoles = email.ReceiveRoles;
                //emailExist.ReceiveUsers = email.ReceiveUsers;
                //UpdateCollection(email, e => e.ID == email.ID, e => e.ReceiveRoles, r => r.ID);
                //UpdateCollection(email, e => e.ID == email.ID, e => e.ReceiveUsers, u => u.ID);
                return Update(email);
            }
            return Create(email) != null;
        }

        public bool DeleteEmailAllInTemplate(string type, string nameTemplate)
        {
            var list = GetAll<EmailSetting>().ToList().Where(e => e.TemplateName.ToUpper().Trim() == nameTemplate.ToUpper().Trim() && e.Type.ToUpper().Trim() == type.ToUpper().Trim()).ToList();
            foreach (var emailReceiveSetting in list)
            {
                Delete<EmailSetting>(emailReceiveSetting.ID);
            }
            return true;
        }

        public decimal GetDepositNextCounter(string name, int start)
        {
            return GetNextCounter(name, start);
        }


        public IEnumerable<object> GetSelectListItemOfEmailTemplate(string type, string nameTemplate, EmailType emailType)
        {
            var usersResult = new List<object>();
            if (string.IsNullOrEmpty(nameTemplate) || string.IsNullOrEmpty(type))
            {
                return usersResult;
            }
            var emailSetting = Get<EmailSetting>(e => e.TemplateName.ToUpper().Trim() == nameTemplate.ToUpper().Trim() && e.Type.ToUpper().Trim() == type.ToUpper().Trim());
            if (emailSetting != null)
            {
                if (emailType == EmailType.To)
                {
                    var list = emailSetting.EmailTo.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    var userIds = new List<int>();
                    var roles = new List<string>();
                    foreach (var id in list)
                    {
                        try
                        {
                            userIds.Add(int.Parse(id));
                        }
                        catch (Exception)
                        {
                            roles.Add(id);
                        }
                    }
                    var usersByRole = GetAll<Role>(m => roles.Contains(StringHelper.RemoveSpecialCharacters(m.Name)));
                    if (usersByRole.Any())
                    {
                        usersResult.AddRange(usersByRole.Select(m => new { Value = StringHelper.RemoveSpecialCharacters(m.Name), Text = m.Name }));
                    }
                    var users = GetAll<User>(m => m.Status == EntityStatus.Normal && userIds.Contains(m.ID));
                    if (users.Any())
                    {
                        usersResult.AddRange(users.Select(m => new { Value = m.ID.ToString(), Text = m.DisplayName }));
                    }
                }
                else if (emailType == EmailType.Cc)
                {
                    var list = emailSetting.EmailCc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    var userIds = new List<int>();
                    var roles = new List<string>();
                    foreach (var id in list)
                    {
                        try
                        {
                            userIds.Add(int.Parse(id));
                        }
                        catch (Exception)
                        {
                            roles.Add(id);
                        }
                    }
                    var usersByRole = GetAll<Role>(m => roles.Contains(StringHelper.RemoveSpecialCharacters(m.Name)));
                    if (usersByRole.Any())
                    {
                        usersResult.AddRange(usersByRole.Select(m => new { Value = StringHelper.RemoveSpecialCharacters(m.Name), Text = m.Name }));
                    }
                    var users = GetAll<User>(m => m.Status == EntityStatus.Normal && userIds.Contains(m.ID));
                    if (users.Any())
                    {
                        usersResult.AddRange(users.Select(m => new { Value = m.ID.ToString(), Text = m.DisplayName }));
                    }
                }
                else if (emailType == EmailType.Bcc)
                {
                    var list = emailSetting.EmailBcc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    var userIds = new List<int>();
                    var roles = new List<string>();
                    foreach (var id in list)
                    {
                        try
                        {
                            userIds.Add(int.Parse(id));
                        }
                        catch (Exception)
                        {
                            roles.Add(id);
                        }
                    }
                    var usersByRole = GetAll<Role>(m => roles.Contains(StringHelper.RemoveSpecialCharacters(m.Name)));
                    if (usersByRole.Any())
                    {
                        usersResult.AddRange(usersByRole.Select(m => new { Value = StringHelper.RemoveSpecialCharacters(m.Name), Text = m.Name }));
                    }
                    var users = GetAll<User>(m => m.Status == EntityStatus.Normal && userIds.Contains(m.ID));
                    if (users.Any())
                    {
                        usersResult.AddRange(users.Select(m => new { Value = m.ID.ToString(), Text = m.DisplayName }));
                    }
                }
            }
            return usersResult;
        }

        #endregion


        #region Holiday

        public IEnumerable<Holiday> GetAllHolidays()
        {
            return GetAll<Holiday>().ToList();
        }

        public IEnumerable<Holiday> GetAllHolidays(int year)
        {
            return GetAll<Holiday>(m => m.Date.Year == year).ToList();
        }

        public Holiday GetHoliday(int holidayID)
        {
            return Get<Holiday>(holidayID);
        }

        public Holiday GetHoliday(DateTime holidayDate)
        {
            return Get<Holiday>(m => m.Date.Date == holidayDate.Date);
        }

        public Holiday CreateHoliday(Holiday holiday)
        {
            Holiday holidayExist = GetHoliday(holiday.Date);
            if (holidayExist != null)
            {
                holidayExist.Description = holiday.Description;
                Update(holidayExist);
                return holidayExist;
            }
            return Create(holiday);
        }

        public bool UpdateHoliday(Holiday holiday)
        {
            return Update(holiday);
        }

        public bool DeleteHoliday(DateTime date)
        {
            Holiday holidayExist = GetHoliday(date);
            if (holidayExist != null)
                return Delete<Holiday>(holidayExist.ID);
            return false;
        }

        public IEnumerable<Holiday> GetAllHolidaysbyMonthAndYear(int month, int year)
        {
            return GetAll<Holiday>(m => m.Date.Month == month && m.Date.Year == year);
        }

        #endregion
    }
}
