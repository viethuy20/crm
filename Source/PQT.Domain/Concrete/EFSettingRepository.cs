using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;
using PQT.Domain.Enum;

namespace PQT.Domain.Concrete
{
    public class EFSettingRepository : Repository, ISettingRepository
    {
        public EFSettingRepository(DbContext db) : base(db)
        {
        }

        public IEnumerable<Setting> GetSettings()
        {
            return GetAll<Setting>().AsEnumerable();
        }

        public Setting GetSetting(int settingId)
        {
            return Get<Setting>(i => i.ID == settingId);
        }

        public Setting GetSetting(string module, string name)
        {
            return Get<Setting>(i => i.Module == module && i.Name == name);
        }

        public Setting CreateSetting(Setting info)
        {
            return Create(info);
        }

        public bool UpdateSetting(Setting info)
        {
            return Update(info);
        }

        public bool DeleteSetting(int settingId)
        {
            return Delete<Setting>(settingId);
        }


        #region NotifySetting


        public IEnumerable<NotifySetting> GetAllNotifySettings()
        {
            return GetAll<NotifySetting>().AsEnumerable();
        }

        public NotifySetting GetNotifySetting(int id)
        {
            return Get<NotifySetting>(id);
        }
        public NotifySetting GetNotifySetting(NotifyType type, NotifyAction action)
        {
            return Get<NotifySetting>(m=>m.NotifyType == type && m.NotifyAction == action);
        }
        public NotifySetting CreateNotifySetting(NotifySetting info)
        {
            return Create(info);
        }

        public bool UpdateNotifySetting(NotifySetting holiday)
        {
            return Update(holiday);
        }

        public bool DeleteNotifySetting(int id)
        {
            return Delete<NotifySetting>(id);
        }

        #endregion NotifySetting

    }
}
