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
    /// The settings are saved in HKEY_CURRENT_USER\SOFTWARE\Company\App, they are saved in binary form
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
                byte[] value = (byte[])settingsKey.GetValue(name, null);
                if (value == null)
                    return new Setting() { name = null, type = null, data = null };
                Type type = null;
                byte[] data = value.Skip(1).ToArray();
                switch (value[0])
                {
                    case 0:
                        type = typeof(string);
                        break;
                    case 1:
                        type = typeof(int);
                        break;
                    case 2:
                        type = typeof(long);
                        break;
                    case 3:
                        type = typeof(bool);
                        break;
                }
                return new Setting() { name = name, type = type, data = data };
            }
        }
        public void Set(string name,object value)
        {
            switch (value)
            {
                case string str:
                    settingsKey.SetValue(name,new byte[] { 0 }.Concat(Encoding.UTF8.GetBytes(str)).ToArray(), RegistryValueKind.Binary);
                    break;
                case int num:
                    settingsKey.SetValue(name, new byte[] { 1 }.Concat(BitConverter.GetBytes(num)).ToArray(), RegistryValueKind.Binary);
                    break;
                case long num:
                    settingsKey.SetValue(name, new byte[] { 2 }.Concat(BitConverter.GetBytes(num)).ToArray(), RegistryValueKind.Binary);
                    break;
                case bool boolean:
                    settingsKey.SetValue(name, new byte[] { 3 }.Concat(BitConverter.GetBytes(boolean)).ToArray(), RegistryValueKind.Binary);
                    break;
            }
        }
    }
    public struct Setting
    {
        public string name;
        public Type type;
        public byte[] data;
        public object GetValue()
        {
            object value;
            switch (type.Name)
            {
                case "String":
                    value = Encoding.UTF8.GetString(data);
                    break;
                case "Int32":
                    value = BitConverter.ToInt32(data, 0);
                    break;
                case "Int64":
                    value = BitConverter.ToInt64(data, 0);
                    break;
                case "Boolean":
                    value = BitConverter.ToBoolean(data, 0);
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
                    value = Encoding.UTF8.GetString(obj.data);
                    break;
                case "Int32":
                    value = BitConverter.ToInt32(obj.data, 0);
                    break;
                case "Int64":
                    value = BitConverter.ToInt64(obj.data, 0);
                    break;
                case "Boolean":
                    value = BitConverter.ToBoolean(obj.data, 0);
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
            object value;
            switch (obj)
            {
                case string str:
                    value = Encoding.UTF8.GetString(data);
                    return (string)value == str;
                case int num:
                    value = BitConverter.ToInt32(data,0);
                    return (int)value == num;
                case long num:
                    value = BitConverter.ToInt64(data, 0);
                    return (long)value == num;
                case bool boolean:
                    value = BitConverter.ToBoolean(data, 0);
                    return (bool)value == boolean;
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            int hashCode = 1330260032;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(data);
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
