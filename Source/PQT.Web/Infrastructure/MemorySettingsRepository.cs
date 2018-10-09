﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Concrete;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Web.Infrastructure
{
    public class MemorySettingsRepository : EFSettingRepository
    {
        private List<Setting> _settings = new List<Setting>();
        private List<Holiday> _holidays = new List<Holiday>();
        private List<NotifySetting> _notifySettings = new List<NotifySetting>();

        #region Factory

        public MemorySettingsRepository(DbContext db)
            : base(db)
        {
            RetrieveResources();
        }

        #endregion

        #region Decorator Properties

        public EFSettingRepository SettingService
        {
            get { return DependencyHelper.GetService<EFSettingRepository>(); }
        }
        #endregion

        private void RetrieveResources()
        {
            _settings.Clear();
            _settings.AddRange(SettingService.GetSettings());
            _holidays.Clear();
            _holidays.AddRange(SettingService.GetAllHolidays().Select(m => new Holiday(m)));
            _notifySettings.Clear();
            _notifySettings.AddRange(SettingService.GetAllNotifySettings());
        }

        public override IEnumerable<Setting> GetSettings()
        {
            return _settings.AsEnumerable();
        }

        public override Setting GetSetting(int settingId)
        {
            return _settings.FirstOrDefault(i => i.ID == settingId);
        }

        public override Setting GetSetting(string module, string name)
        {
            return _settings.FirstOrDefault(i => i.Module == module && i.Name == name);
        }

        public override Setting CreateSetting(Setting info)
        {
            var newValue = SettingService.CreateSetting(info);
            _settings.Add(newValue);
            return newValue;
        }

        public override bool UpdateSetting(Setting info)
        {
            var newValue = SettingService.UpdateSetting(info);
            _settings.Remove(GetSetting(info.ID));
            _settings.Add(info);
            return newValue;
        }


        #region Holiday

        public override IEnumerable<Holiday> GetAllHolidays()
        {
            return _holidays.AsEnumerable();
        }
        public override IEnumerable<Holiday> GetAllHolidays(int[] year)
        {
            return _holidays.Where(m => year.Contains(m.StartDate.Year)).ToList();
        }

        public override Holiday CreateHoliday(Holiday holiday)
        {
            var newValue = SettingService.CreateHoliday(holiday);
            _holidays.Add(new Holiday(newValue));
            return newValue;
        }

        public override bool DeleteHoliday(int id)
        {
            var newValue = SettingService.DeleteHoliday(id);
            if (newValue)
            {
                _holidays.Remove(_holidays.FirstOrDefault(i => i.ID == id));
                return newValue;
            }
            return newValue;
        }
        public override int TotalHolidays(DateTime start, DateTime end, int? countryId)
        {
            var holidays = _holidays.Where(m => m.EntityStatus == EntityStatus.Normal &&
                                                (countryId == null || m.CountryID == countryId) &&
                                                ((m.StartDate.Month == start.Month && m.StartDate.Year == start.Year) ||
                                                 (m.EndDate.Month == start.Month && m.EndDate.Year == start.Year) ||
                                                 (m.StartDate.Month == end.Month && m.StartDate.Year == end.Year) ||
                                                 (m.EndDate.Month == end.Month && m.EndDate.Year == end.Year)))
                .AsEnumerable();
            return holidays.Sum(m => m.TotalHolidays(start, end));
        }

        #endregion

        #region Notify Setting

        public override IEnumerable<NotifySetting> GetAllNotifySettings()
        {
            return _notifySettings.AsEnumerable();
        }

        public override NotifySetting GetNotifySetting(int id)
        {
            return _notifySettings.FirstOrDefault(m=>m.ID == id);
        }
        public override NotifySetting GetNotifySetting(NotifyType type, NotifyAction action)
        {
            return _notifySettings.FirstOrDefault(m => m.NotifyType == type && m.NotifyAction == action);
        }
        public override NotifySetting CreateNotifySetting(NotifySetting info)
        {
            var newValue = SettingService.CreateNotifySetting(info);
            _notifySettings.Add(info);
            return newValue;
        }

        public override bool UpdateNotifySetting(NotifySetting info)
        {
            var newValue = SettingService.UpdateNotifySetting(info);
            _notifySettings.Remove(GetNotifySetting(info.ID));
            _notifySettings.Add(info);
            return newValue;
        }

        public override bool DeleteNotifySetting(int id)
        {
            var newValue = SettingService.DeleteNotifySetting(id);
            if (newValue)
            {
                _notifySettings.Remove(GetNotifySetting(id));
                return newValue;
            }
            return newValue;
        }
        #endregion Notify Setting

    }
}
