using System.Collections.Generic;

namespace CSAuto.Languages
{
    class Template
    {
        /* change file name to the corresponding language and change the class name to the corresponding language*/
        static Dictionary<string, string> translation = new Dictionary<string, string>()
        {
            /* you can copy from the english translation */
        };
        public static string Get(string category)
        {
            if (translation.ContainsKey(category)) return translation[category]; else return category;
        }
    }
}