using System;
using System.Web.Mvc;
using PQT.Domain.Abstract;
using PQT.Domain.Entities;

namespace PQT.Web.Infrastructure.Utility
{
    public class Settings
    {
        #region Repository

        private static ISettingRepository SettingRepository
        {
            get { return DependencyHelper.GetService<ISettingRepository>(); }
        }

        #endregion

        /// <summary>
        ///     Get value of property in system setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingModule">PQT.Domain.Entities.Setting.ModuleType.Indent or any one</param>
        /// <param name="keyGetValue">Setting.ModuleKey.*.* : the key to get it's value</param>
        /// <param name="typeofData">type of return value</param>
        /// <returns>Return a value of the key in module</returns>
        public static dynamic GetSetting<T>(Setting.ModuleType settingModule, T keyGetValue, Type typeofData)
        {
            string module = Enum.GetName(typeof(Setting.ModuleType), settingModule);
            string key = Enum.GetName(typeof(T), keyGetValue);

            Setting t = SettingRepository.GetSetting(module, key);
            dynamic tempData = null;
            if (typeofData == typeof(int)) tempData = Convert.ToInt32(t.Value);
            else if (typeofData == typeof(double)) tempData = Convert.ToDouble(t.Value);
            else if (typeofData == typeof(decimal)) tempData = Convert.ToDecimal(t.Value);
            else if (typeofData == typeof(bool)) tempData = Convert.ToBoolean(t.Value);
            else tempData = t.Value;

            return tempData;
        }

        #region Nested type: SystemConfig

        public class Lead
        {
            public static int NumberDaysExpired()
            {
                return Convert.ToInt32(GetSetting(Setting.ModuleType.Lead, Setting.ModuleKey.Lead.NumberDaysExpired, typeof(int)));
            }
            public static int MaxBlockeds()
            {
                return Convert.ToInt32(GetSetting(Setting.ModuleType.Lead, Setting.ModuleKey.Lead.MaxBlockeds, typeof(int)));
            }
            public static int MaxLOIs()
            {
                return Convert.ToInt32(GetSetting(Setting.ModuleType.Lead, Setting.ModuleKey.Lead.MaxLOIs, typeof(int)));
            }
        }
        #endregion

    }
}
