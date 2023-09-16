using CSAuto.Languages;
using Murky.Utils;
using System;
using System.Reflection;

namespace CSAuto
{
    public class AppLanguage
    {
        public static AppLanguage Language { get { return _instance; } }
        public static string[] Available = new string[]
        {
            "language_english",
            "language_russian"
        };
        AppLanguage()
        {
            languageType = Type.GetType("CSAuto.Languages." + char.ToUpper(Properties.Settings.Default.currentLanguage[9]) + Properties.Settings.Default.currentLanguage.Substring(10));
            getMethod = languageType.GetMethod("Get");
        }
        //singleton instance
        private static AppLanguage _instance = new AppLanguage();
        private Type languageType;
        private MethodInfo getMethod;
        public string this[string category]
        {
            get { return _instance.Get(category); }
        }
        private string Get(string category)
        {
            if (category == null)
                return "";
            return (string)getMethod.Invoke(null, new object[] { category });
        }
    }
}
