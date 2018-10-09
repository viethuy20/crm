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
                    var users = GetAll<User>(m => m.Status == EntityUserStatus.Normal && userIds.Contains(m.ID));
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
                    var users = GetAll<User>(m => m.Status == EntityUserStatus.Normal && userIds.Contains(m.ID));
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
                    var users = GetAll<User>(m => m.Status == EntityUserStatus.Normal && userIds.Contains(m.ID));
                    if (users.Any())
                    {
                        usersResult.AddRange(users.Select(m => new { Value = m.ID.ToString(), Text = m.DisplayName }));
                    }
                }
            }
            return usersResult;
        }

        #endregion

        #region Office Location


        public IEnumerable<OfficeLocation> GetAllOfficeLocations()
        {
            return GetAll<OfficeLocation>().AsEnumerable();
        }

        public OfficeLocation GetOfficeLocation(int id)
        {
            return Get<OfficeLocation>(id);
        }
        public OfficeLocation GetOfficeLocation(string name)
        {
            return Get<OfficeLocation>(m => m.Name.ToLower().Trim() == name.ToLower().Trim());
        }

        public OfficeLocation CreateOfficeLocation(OfficeLocation info)
        {
            return Create(info);
        }

        public bool UpdateOfficeLocation(OfficeLocation holiday)
        {
            return Update(holiday);
        }

        public bool DeleteOfficeLocation(int id)
        {
            return Delete<OfficeLocation>(id);
        }

        #endregion
        #region RecruitmentPosition


        public IEnumerable<RecruitmentPosition> GetAllRecruitmentPositions()
        {
            return GetAll<RecruitmentPosition>().AsEnumerable();
        }

        public RecruitmentPosition GetRecruitmentPosition(int id)
        {
            return Get<RecruitmentPosition>(id);
        }

        public RecruitmentPosition CreateRecruitmentPosition(RecruitmentPosition info)
        {
            return Create(info);
        }

        public bool UpdateRecruitmentPosition(RecruitmentPosition holiday)
        {
            return Update(holiday);
        }

        public bool DeleteRecruitmentPosition(int id)
        {
            return Delete<RecruitmentPosition>(id);
        }

        #endregion RecruitmentPosition

        #region BankAccount


        public IEnumerable<BankAccount> GetAllBankAccounts()
        {
            return GetAll<BankAccount>().AsEnumerable();
        }

        public BankAccount GetBankAccount(int id)
        {
            return Get<BankAccount>(id);
        }
        public BankAccount GetBankAccount(string accountNo)
        {
            return Get<BankAccount>(m => m.AccountNumber.ToLower().Trim() == accountNo.ToLower().Trim());
        }

        public BankAccount CreateBankAccount(BankAccount info)
        {
            return Create(info);
        }

        public bool UpdateBankAccount(BankAccount holiday)
        {
            return Update(holiday);
        }

        public bool DeleteBankAccount(int id)
        {
            return Delete<BankAccount>(id);
        }

        #endregion BankAccount


        #region VenueInfo

        public VenueInfo GetVenueInfo(int id)
        {
            return Get<VenueInfo>(id);
        }

        public bool UpdateVenueInfo(VenueInfo info)
        {
            return Update(info);
        }

        #endregion

        #region AccomodationInfo
        public AccomodationInfo GetAccomodationInfo(int id)
        {
            return Get<AccomodationInfo>(id);
        }

        public bool UpdateAccomodationInfo(AccomodationInfo info)
        {
            return Update(info);
        }

        #endregion
    }
}
