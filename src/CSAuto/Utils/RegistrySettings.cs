using ControlzEx.Standard;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Murky.Utils
{
    /// <summary>
    /// Class to save settings into the registry, supported types are <see langword="bool" />,<see langword="string" />,<see langword="int" /> and <see langword="long" />
    /// <code></code>
    /// The settings are saved in HKEY_CURRENT_USER\SOFTWARE\Company\App
    /// </summary>
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
        public Setting this[string name] 
        {
            get 
            { 
                object value = settingsKey.GetValue(name, null);
                if (value == null)
                    return new Setting() { name = null, type = null, data = null };
                Type type = null;
                switch (settingsKey.GetValueKind(name))
                {
                    case RegistryValueKind.String:
                        type = typeof(string);
                        break;
                    case RegistryValueKind.DWord:
                        type = typeof(int);
                        break;
                    case RegistryValueKind.QWord:
                        type = typeof(long);
                        break;
                    case RegistryValueKind.Binary:
                        type = typeof(bool);
                        break;
                }
                return new Setting() { name = name, type = type, data = value };
            }
        }
        public void Delete(string name)
        {
            settingsKey.DeleteValue(name);
        }
        public void Set(string name,object value)
        {
            switch (value)
            {
                case string _:
                    settingsKey.SetValue(name,value, RegistryValueKind.String);
                    break;
                case int _:
                    settingsKey.SetValue(name, value, RegistryValueKind.DWord);
                    break;
                case long _:
                    settingsKey.SetValue(name, value, RegistryValueKind.QWord);
                    break;
                case bool boolean:
                    settingsKey.SetValue(name, BitConverter.GetBytes(boolean), RegistryValueKind.Binary);
                    break;
            }
        }
    }
    public struct Setting
    {
        public string name;
        public Type type;
        public object data;
        public object GetValue()
        {
            object value;
            switch (type.Name)
            {
                case "String":
                    value = (string)data;
                    break;
                case "Int32":
                    value = (int)data;
                    break;
                case "Int64":
                    value = (long)data;
                    break;
                case "Boolean":
                    value = BitConverter.ToBoolean((byte[])data, 0);
                    break;
                default:
                    return null;
            }
            return value;
        }
        private static object GetValue(Setting obj)
        {
            object value;
            switch (obj.type.Name)
            {
                case "String":
                    value = (string)obj.data;
                    break;
                case "Int32":
                    value = (int)obj.data;
                    break;
                case "Int64":
                    value = (long)obj.data;
                    break;
                case "Boolean":
                    value = BitConverter.ToBoolean((byte[])obj.data, 0);
                    break;
                default:
                    return null;
            }
            return value;
        }
        public override string ToString()
        {
            return GetValue(this).ToString();
        }
        public override bool Equals(object obj)
        {
            if (name == null && type == null && data == null && obj == null)
                return true;
            if (obj == null || type != obj.GetType())
                return false;
            switch (obj)
            {
                case string str:
                    return (string)data == str;
                case int num:
                    return (int)data == num;
                case long num:
                    return (long)data == num;
                case bool boolean:
                    return (bool)GetValue() == boolean;
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            int hashCode = 1330260032;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(data);
            return hashCode;
        }

        public static bool operator ==(Setting lhs, object rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(Setting lhs, object rhs)
        {
            return !(lhs == rhs);
        }
        public static bool operator !(Setting lhs)
        {
            return lhs == false;
        }
        public static implicit operator bool(Setting obj)
        {
            return obj == true;
        }
        public static implicit operator string(Setting obj)
        {
            return GetValue(obj).ToString();
        }
        public static implicit operator int(Setting obj)
        {
            return (int)GetValue(obj);
        }
        public static implicit operator long(Setting obj)
        {
            return (long)GetValue(obj);
        }
    }
}
