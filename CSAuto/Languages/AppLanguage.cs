using CSAuto.Languages;
namespace CSAuto
{
    public static class AppLanguage
    {
        public static string Get(string category)
        {
            if (category == null)
                return "";
            switch (Properties.Settings.Default.currentLanguage)
            {
                case "language_english":
                    return English.Get(category);
                case "language_russian":
                    return Russian.Get(category);
                default:
                    return English.Get(category);
            }
        }
    }
}
