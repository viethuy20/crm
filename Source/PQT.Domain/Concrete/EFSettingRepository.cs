using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NS.Entity;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Domain.Concrete
{
    public class EFSettingRepository : Repository, ISettingRepository
    {
        public EFSettingRepository(DbContext db) : base(db)
        {
        }

        public virtual IEnumerable<Setting> GetSettings()
        {
            return GetAll<Setting>().AsEnumerable();
        }

        public virtual Setting GetSetting(int settingId)
        {
            return Get<Setting>(i => i.ID == settingId);
        }

        public virtual Setting GetSetting(string module, string name)
        {
            return Get<Setting>(i => i.Module == module && i.Name == name);
        }

        public virtual Setting CreateSetting(Setting info)
        {
            return Create(info);
        }

        public virtual bool UpdateSetting(Setting info)
        {
            return Update(info);
        }


        #region Holiday

        public virtual IEnumerable<Holiday> GetAllHolidays()
        {
            return GetAll<Holiday>(m => m.EntityStatus == EntityStatus.Normal).ToList();
        }
        public virtual IEnumerable<Holiday> GetAllHolidays(int[] year)
        {
            return GetAll<Holiday>(m => year.Contains(m.StartDate.Year)).ToList();
        }

        public virtual Holiday CreateHoliday(Holiday holiday)
        {
            holiday = Create(holiday);
            if (holiday != null && holiday.CountryID > 0)
            {
                holiday.Country = Get<Country>((int)holiday.CountryID);
            }
            return holiday;
        }

        public virtual bool DeleteHoliday(int id)
        {
            return Delete<Holiday>(id);
        }
        public virtual int TotalHolidays(DateTime start, DateTime end, int? countryId)
        {
            var holidays = GetAll<Holiday>(m => (countryId == null || m.CountryID == countryId) &&
                                                (m.StartDate.Month == start.Month && m.StartDate.Year == start.Year ||
                                                 m.EndDate.Month == start.Month && m.EndDate.Year == start.Year ||
                                                 m.StartDate.Month == end.Month && m.StartDate.Year == end.Year ||
                                                 m.EndDate.Month == end.Month && m.EndDate.Year == end.Year))
                .AsEnumerable();
            return holidays.Sum(m => m.TotalHolidays(start, end));
        }

        #endregion
    }
}
