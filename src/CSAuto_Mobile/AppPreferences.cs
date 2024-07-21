using Plugin.Settings.Abstractions;
using Plugin.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAuto_Mobile
{
    public class AppPreferences
    {
        static ISettings AppSettings => CrossSettings.Current;

        public static string ServerPort
        {
            get { return AppSettings.GetValueOrDefault(Constants.PORT_KEY, "Port"); }
            set { AppSettings.AddOrUpdateValue(Constants.PORT_KEY, value); }
        }
        public static string ServerIp
        {
            get { return AppSettings.GetValueOrDefault(Constants.IP_KEY, "IP"); }
            set { AppSettings.AddOrUpdateValue(Constants.IP_KEY, value); }
        }
    }
}
