using System;
using System.Configuration;

namespace PushToElastic.StaticTools
{
    public static class Config
    {
        public static string WebAddr { get; private set; }
        public static string SystemActive { get; private set; }
        public static string SystemInactive { get; private set; }
        public static string TestRunning { get; private set; }
        public static string TestCompleted { get; private set; }
        public static string TestAborted { get; private set; }
        public static string TestTotal { get; private set; }
        public static bool IsDebug { get; private set; }
        public static string UserName { get; private set; }
        public static string Password { get; private set; }
        public static string LogFilePath { get; private set; }
        public static string ConfigPath { get; private set; }
        public static string TestTypeList { get; private set; }
        public static string VehicleTypeList { get; private set; }
        public static string IdTypeList { get; private set; }
        public static string SystemState { get; private set; }
        public static string TestState { get; private set; }
        public static string TestType { get; private set; }
        public static string VehicleType { get; private set; }
        public static string DriverID { get; private set; }
        public static string Date { get; private set; }
        public static string Time { get; private set; }
        public static int ConnectionRefreshTime { get; private set; }


        static Config()
        {
            UpdateFields();
        }

        private static void UpdateFields()
        {
            WebAddr = AppConfig("WebAddr");
            SystemActive = AppConfig("SystemActive");
            SystemInactive = AppConfig("SystemInactive");
            TestRunning = AppConfig("TestRunning");
            TestCompleted = AppConfig("TestCompleted");
            TestAborted = AppConfig("TestAborted");
            TestTotal = AppConfig("TestTotal");
            IsDebug = TypeCast.ToBool(AppConfig("IsDebug"));
            UserName = AppConfig("UserName");
            Password = AppConfig("Password");
            LogFilePath = AppConfig("LogFilePath");
            ConfigPath = FormatPath(AppConfig("ConfigPath"));
            TestTypeList = AppConfig("TestTypeList");
            VehicleTypeList = AppConfig("VehicleTypeList");
            IdTypeList = AppConfig("IdTypeList");
            SystemState = AppConfig("SystemState");
            TestState = AppConfig("TestState");
            TestType = AppConfig("TestType");
            VehicleType = AppConfig("VehicleType");
            DriverID = AppConfig("DriverID");
            Date = AppConfig("Date");
            Time = AppConfig("Time");
            ConnectionRefreshTime = TypeCast.ToInt(AppConfig("ConnectionRefreshTime")) * 1000;
        }

        private static string FormatPath(string path)
        {
            if (!path.EndsWith(@"\"))
            {
                return path + @"\";
            }
            return path;
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
