using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
        string company;
        string product;
        private RegistryKey settingsKey;
        public RegistrySettings()
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            settingsKey = softwareKey.CreateSubKey($"{versionInfo.CompanyName}\\{versionInfo.ProductName}", true);
            company = versionInfo.CompanyName;
            product = versionInfo.ProductName;
        }
        public RegistrySettings(string companyName,string productName)
        {
            settingsKey = softwareKey.CreateSubKey($"{companyName}\\{productName}", true);
            company = companyName;
            product = productName;
        }
        public bool Exists()
        {
            return softwareKey.OpenSubKey(company,false).GetSubKeyNames().Contains(product);
        }
        public void DeleteSettings()
        {
            softwareKey.DeleteSubKey($"{company}\\{product}");
        }
        public override string ToString()
        {
            string res = $"{company} - {product} - RegSet\n\n";
            foreach (var item in settingsKey.GetValueNames())
            {
                Setting obj = this[item];
                switch (settingsKey.GetValueKind(item))
                {
                    case RegistryValueKind.String:
                        res += (char)1;
                        break;
                    case RegistryValueKind.DWord:
                        res += (char)2;
                        break;
                    case RegistryValueKind.QWord:
                        res += (char)3;
                        break;
                    case RegistryValueKind.Binary:
                        res += (char)4;
                        break;
                }
                res += $"\"{item}\"\t\"{obj}\"\r\n";
            }
            return res;
        }
        public bool Import(string path)
        {
            char[] oldValues = new char[]
            {
                '0','1','2','3'
            };
            try
            {
                string file = File.ReadAllText(path, Encoding.Default);
                file = file.Split(new string[] { "RegSet" }, StringSplitOptions.None)[1];
                string[] lines = file.Split(new string[] { "\"\r\n" },StringSplitOptions.None);
                for (int i =0; i< lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    line += '"';
                    string[] values = GetValues(line);
                    if (values[0] == null || values[1] == null)
                        continue;
                    if (oldValues.Contains(line[0]))
                        OldLoadValues(line, values);
                    else
                        NewLoadValues(line, values);
                }
                return true;
            }
            catch { }
            return false;
        }

        private void NewLoadValues(string line, string[] values)
        {
            switch (line[0])
            {
                case (char)1:
                    Set(values[0], values[1]); break;
                case (char)2:
                    Set(values[0], int.Parse(values[1])); break;
                case (char)3:
                    Set(values[0], long.Parse(values[1])); break;
                case (char)4:
                    Set(values[0], bool.Parse(values[1])); break;
            }
        }

        private void OldLoadValues(string line, string[] values)
        {
            switch (line[0])
            {
                case '0':
                    Set(values[0], values[1]); break;
                case '1':
                    Set(values[0], int.Parse(values[1])); break;
                case '2':
                    Set(values[0], long.Parse(values[1])); break;
                case '3':
                    Set(values[0], bool.Parse(values[1])); break;
            }
        }

        private string[] GetValues(string v)
        {
            string[] res = new string[2];
            string firstVal = null;
            string[] splt = v.Split('"');
            if(splt.Length > 1)
            {
                splt = splt[1].Split('\t');
                firstVal = splt[0].Trim(new char[] { '"' });
            }
            string secVal = null;
            splt = v.Split(new char[] { '"' }, 3);
            if(splt.Length > 2)
            {
                splt = splt[2].Split('\t');
                if(splt.Length > 1)
                    secVal = splt[1].Trim(new char[] { '"' });
            }
            res[0] = firstVal;
            res[1] = secVal;
            return res;
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

        public bool KeyExists(string res)
        {
            return settingsKey.GetValue(res) != null;
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
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = (hashCode * -1521134295) + EqualityComparer<Type>.Default.GetHashCode(type);
            hashCode = (hashCode * -1521134295) + EqualityComparer<object>.Default.GetHashCode(data);
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
