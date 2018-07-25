using System;
using System.Collections.Generic;
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
            if (t != null)
            {
                dynamic tempData = null;
                if (typeofData == typeof(int)) tempData = Convert.ToInt32(t.Value);
                else if (typeofData == typeof(double)) tempData = Convert.ToDouble(t.Value);
                else if (typeofData == typeof(decimal)) tempData = Convert.ToDecimal(t.Value);
                else if (typeofData == typeof(bool)) tempData = Convert.ToBoolean(t.Value);
                else tempData = t.Value;
                return tempData;
            }
            else
            {
                var defaulValue = GetDefault(typeofData);
                var newSetting = new Setting
                {
                    Module = module,
                    Name = key,
                    Value = defaulValue,

                };
                SettingRepository.CreateSetting(newSetting);
                return defaulValue;
            }
        }
        public static dynamic GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
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

        public class System
        {
            public static int NotificationNumber()
            {
                return Convert.ToInt32(GetSetting(Setting.ModuleType.System, Setting.ModuleKey.System.NotificationNumber, typeof(int)));
            }
        }
        public class KPI
        {
            public static int VoIpBuffer()
            {
                return Convert.ToInt32(GetSetting(Setting.ModuleType.KPI, Setting.ModuleKey.KPI.VoIpBuffer, typeof(int)));
            }
            public static string[] ExceptCodes()
            {
                var value = Convert.ToString(GetSetting(Setting.ModuleType.KPI, Setting.ModuleKey.KPI.ExceptCode, typeof(string)));
                return !string.IsNullOrEmpty(value) ? value.Split(",", StringSplitOptions.RemoveEmptyEntries) : new List<string>().ToArray();
            }
        }
        #endregion

    }
}
