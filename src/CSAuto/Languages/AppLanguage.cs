
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
                LanguageCode = "en-US",
                IsRTL = false,
                Enabled = true 
            },
            new Language()
            {
                LanguageCode = "ru-RU",
                IsRTL = false,
                Enabled = true
            },
            new Language()
            {
                LanguageCode = "he-IL",
                IsRTL = true,
                Enabled = true
            },
            new Language()
            {
                LanguageCode = "ko-KR",
                IsRTL = false,
                Enabled = false
            },
            new Language()
            {
                LanguageCode = "it-IT",
                IsRTL = false,
                Enabled = false
            },
        };
    }
}
