using System;
using System.Configuration;
using System.IO;
using System.Security.Permissions;

namespace STARS.Applications.VETS.Plugins.SystemMonitor
{
    public static class Config
    {
        public static int MaxTimer { get; private set; }   
        public static string EnumerationsPath { get; private set; }
        public static string TestTypeList { get; private set; }
        public static string VehicleTypeList { get; private set; }
        public static string VehicleManufacturerList { get; private set; }
        public static string IdTypeList { get; private set; }
        public static bool ShowForm { get; private set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        static Config()
        {
            string path = typeof(Config).Assembly.Location + ".config";
            string dir = Path.GetDirectoryName(path);
            string file = Path.GetFileName(path);

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = dir;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = file;
            watcher.Changed += new FileSystemEventHandler(OnAppConfigChanged);
            watcher.EnableRaisingEvents = true;

            UpdateFields();
        }

        private static void OnAppConfigChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                UpdateFields();
            }
            catch(Exception ex)
            {
                if (!ex.Message.Contains("being used by another process"))
                {
                    throw ex;
                }
            }
        }

        private static void UpdateFields()
        {
            MaxTimer = TypeCast.ToInt(AppConfig("MaxTimer"));
            EnumerationsPath = FormatPath(AppConfig("EnumerationsPath"));
            TestTypeList = AppConfig("TestTypeList");
            VehicleTypeList = AppConfig("VehicleTypeList");
            VehicleManufacturerList = AppConfig("VehicleManufacturerList");
            IdTypeList = AppConfig("IdTypeList");
            ShowForm = TypeCast.ToBool(AppConfig("ShowForm"));
        }

        private static string FormatPath(string path)
        {
            if(!path.EndsWith(@"\"))
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
            return string.Empty; ;
        }

    }
}
