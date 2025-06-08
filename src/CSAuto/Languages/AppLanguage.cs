
using System.Collections.Generic;

namespace CSAuto
{
    public static class AppLanguage
    {
        public static string[] Available = new string[]
        {
            "en-US",
            "ru-RU",
            "he-IL"
        };

        public static Dictionary<string, bool> IsRTL = new Dictionary<string, bool>
        {
            ["en-US"] = false,
            ["ru-RU"] = false,
            ["he-IL"] = true
        };
    }
}
