
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
        //singleton instance
        private static AppLanguage _instance = new AppLanguage();
        public string this[string category]
        {
            get { return _instance.Get(category); }
        }
        private string Get(string category)
        {
            if (category == null)
                return "";
            return Languages._Language.Get(category);
        }
    }
}
