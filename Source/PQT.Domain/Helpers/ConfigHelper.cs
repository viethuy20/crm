using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace PQT.Domain.Helpers
{
    internal class ConfigHelper
    {
        public static string GetAppSetting(string key)
        {
            string exeConfigPath = Assembly.GetExecutingAssembly().Location;
            Configuration config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);

            KeyValueConfigurationElement element = config.AppSettings.Settings[key];
            if (element != null)
            {
                string value = element.Value;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            throw new KeyNotFoundException("`" + key + "` is not found");
        }
    }
}
