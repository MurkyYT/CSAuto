
using System.Collections.Generic;

namespace CSAuto
{
    public static class AppLanguage
    {
        public struct Language
        {
            public string LanguageCode;
            public bool IsRTL;
            public bool Enabled;
        }

        public static readonly Language[] Available = new Language[]
        {
            new Language() 
            {
                LanguageCode = "en",
                IsRTL = false,
                Enabled = true 
            },
            new Language()
            {
                LanguageCode = "ru",
                IsRTL = false,
                Enabled = true
            },
            new Language()
            {
                LanguageCode = "he",
                IsRTL = true,
                Enabled = true
            },
            new Language()
            {
                LanguageCode = "ko",
                IsRTL = false,
                Enabled = false
            },
            new Language()
            {
                LanguageCode = "it",
                IsRTL = false,
                Enabled = false
            },
        };
    }
}
