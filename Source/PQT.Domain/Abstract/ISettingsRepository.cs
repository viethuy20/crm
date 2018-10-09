using System;
using System.Collections.Generic;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Abstract
{
    public interface ISettingRepository
    {
        IEnumerable<Setting> GetSettings();
        Setting GetSetting(int settingId);
        Setting GetSetting(string module, string name);
        Setting CreateSetting(Setting info);
        bool UpdateSetting(Setting info);

        IEnumerable<Counter> GetAllCounter(string name);
        int GetCounter(string name, int defaultValue = 1);
        int UpdateCounter(string name, int defaultValue = 1);
        bool UpdateCounter(Counter counter);



        #region NotifySetting
        IEnumerable<NotifySetting> GetAllNotifySettings();
        NotifySetting GetNotifySetting(int id);
        NotifySetting GetNotifySetting(NotifyType type, NotifyAction action);
        NotifySetting CreateNotifySetting(NotifySetting info);
        bool UpdateNotifySetting(NotifySetting info);
        bool DeleteNotifySetting(int id);
        #endregion NotifySetting
        #region Holiday
        IEnumerable<Holiday> GetAllHolidays();
        IEnumerable<Holiday> GetAllHolidays(int[] year);
        Holiday CreateHoliday(Holiday holiday);
        bool DeleteHoliday(int id);
        int TotalHolidays(DateTime start, DateTime end, int? locationID);
        #endregion Holiday
    }
}
