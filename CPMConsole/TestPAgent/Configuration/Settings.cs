using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPMPaymentsService.Configuration
{
    public class Settings
    {
        private static readonly System.Configuration.Configuration Config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ExecutableFilePath);

        private static string _connStr;
        private static Nullable<bool> _saveLog;
        private static Nullable<bool> _saveXML;
        private static Nullable<int> _interval;
        private static string _services;
        private static List<string> _technicalOverdraftBranches;

        private static string ExecutableFilePath
        {
            get
            {
                return System.Reflection.Assembly.GetAssembly(typeof(Settings)).Location;
            }
        }

        public static bool SaveLog
        {
            get
            {
                if (!_saveLog.HasValue)
                    _saveLog = Config.AppSettings.Settings["SaveLog"].Value.ToLower().Equals("true");
                return _saveLog.Value;
            }
        }

        public static bool SaveXML
        {
            get
            {
                if (!_saveXML.HasValue)
                    _saveXML = Config.AppSettings.Settings["SaveXML"].Value.ToLower().Equals("true");
                return _saveXML.Value;
            }
        }

        public static int Port
        {
            get
            {
                if (!_interval.HasValue)
                    _interval = Int32.Parse(Config.AppSettings.Settings["port"].Value);
                return _interval.Value;
            }
        }

        public static string IP
        {
            get
            {
                if (string.IsNullOrEmpty(_services))
                    _services = Config.AppSettings.Settings["ip"].Value;
                return _services;
            }
        }

    }
}
