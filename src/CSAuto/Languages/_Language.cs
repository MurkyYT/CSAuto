using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAuto.Languages
{
    public class _Language
    {
        public static Dictionary<string, string> translation = new Dictionary<string, string>()
        {
            
        };
        public static Dictionary<string, string> englishTranslation = new Dictionary<string, string>()
        {

        };
        public static string Get(string category)
        {
            if (translation.ContainsKey(category)) return translation[category]; 
            else if (englishTranslation.ContainsKey(category)) return englishTranslation[category]; 
            else return category;
        }
    }
}
