using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Murky.Utils.CSGO
{
    public static class CSGOMap
    {
        readonly static WebClient client = new WebClient();
        static CSGOMap()
        {
            client.Proxy = null;
        }
        public static bool IsOfficial(string mapName) 
        {
            string mapExtention = mapName.Substring(0, 3);
            return
                mapExtention == "de_" ||
                mapExtention == "dz_" ||
                mapExtention == "gd_" ||
                mapExtention == "cs_" ||
                mapExtention == "ar_";
        }
        public static string GetMapIcon(string mapName)
        {
            try
            {
                string info = client.DownloadString($"https://developer.valvesoftware.com/wiki/File:{mapName}.png");
                string result = $"https://developer.valvesoftware.com/w/images/{info.Split(new string[] { "a href=\"/w/images/" }, StringSplitOptions.None)[1].Split('"')[0]}";
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
