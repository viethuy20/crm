using System;
using System.Collections.Generic;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface ISettingRepository
    {
        IEnumerable<Setting> GetSettings();
        Setting GetSetting(int settingId);
        Setting GetSetting(string module, string name);
        Setting CreateSetting(Setting info);
        bool UpdateSetting(Setting info);

        #region Holiday
        IEnumerable<Holiday> GetAllHolidays();
        IEnumerable<Holiday> GetAllHolidays(int[] year);
        Holiday CreateHoliday(Holiday holiday);
        bool DeleteHoliday(int id);
        int TotalHolidays(DateTime start, DateTime end, int? locationID);
        #endregion Holiday
    }
}
