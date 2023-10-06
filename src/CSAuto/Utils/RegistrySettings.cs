using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Murky.Utils
{
    public class RegistrySettings
    {
        private RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
        private RegistryKey settingsKey;
        public RegistrySettings()
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            settingsKey = softwareKey.CreateSubKey($"{versionInfo.CompanyName}\\{versionInfo.ProductName}", true);
        }
        public RegistrySettings(string companyName,string productName)
        {
            settingsKey = softwareKey.CreateSubKey($"{companyName}\\{productName}", true);
        }
        public object this[string name] 
        {
            get { return settingsKey.GetValue(name, null) ; }
            set
            {
                switch (value)
                {
                    case string str:
                        settingsKey.SetValue(name, str,RegistryValueKind.String);
                        break;
                    case int num:
                        settingsKey.SetValue(name, num, RegistryValueKind.DWord);
                        break;
                    case long num:
                        settingsKey.SetValue(name, num, RegistryValueKind.QWord);
                        break;
                    case bool boolean:
                        settingsKey.SetValue(name, boolean ? 1 : 0, RegistryValueKind.DWord);
                        break;
                }
            }
        } 
    }
}
