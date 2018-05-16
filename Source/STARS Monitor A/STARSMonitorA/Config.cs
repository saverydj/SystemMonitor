using System;
using System.Configuration;

namespace STARSMonitorA
{
    public static class Config
    {
        private static string hookPtr1;
        public static string HookPtr1
        {
            get { return hookPtr1; }
            set { SetField("HookPtr1", value.ToString()); }
        }

        private static string hookPtr2;
        public static string HookPtr2
        {
            get { return hookPtr2; }
            set { SetField("HookPtr2", value.ToString()); }
        }

        private static void UpdateFields()
        {
            hookPtr1 = AppConfig("HookPtr1");
            hookPtr2 = AppConfig("HookPtr2");
        }

        static Config()
        {
            UpdateFields();
        }

        private static string AppConfig(string key)
        {
            Configuration config = null;
            string exeConfigPath = typeof(Config).Assembly.Location;
            config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
            if (config == null || config.AppSettings.Settings.Count == 0)
            {
                throw new Exception(String.Format("Config file {0}.config is missing or could not be loaded.", exeConfigPath));
            }

            KeyValueConfigurationElement element = config.AppSettings.Settings[key];
            if (element != null)
            {
                string value = element.Value;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return string.Empty;
        }

        private static void SetField(string key, string value)
        {
            Configuration config = null;
            string exeConfigPath = typeof(Config).Assembly.Location;
            config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
            if (config == null || config.AppSettings.Settings.Count == 0)
            {
                throw new Exception(String.Format("Config file {0}.config is missing or could not be loaded.", exeConfigPath));
            }

            KeyValueConfigurationElement element = config.AppSettings.Settings[key];
            if (element == null) return;
            element.Value = value;
            config.Save();

            UpdateFields();
        }

    }
}
