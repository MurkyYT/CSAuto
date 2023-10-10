using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace Murky.Utils.CSGO
{
    public static class CSGOMap
    {
        public static Dictionary<string, string> MapIcons = new Dictionary<string, string>();
        readonly static WebClient client = new WebClient();
        static CSGOMap()
        {
            client.Proxy = null;
            client.Headers.Add(HttpRequestHeader.Host, "developer.valvesoftware.com");
            client.Headers.Add(HttpRequestHeader.Accept, "*/*");
            client.Headers.Add(HttpRequestHeader.Cookie, "AkamaiEdge=true");
        }

        public static void LoadMapIcons()
        {
            try
            {
                string info = client.DownloadString("https://developer.valvesoftware.com/wiki/Counter-Strike_2/Maps#/media");
                string[] splt = info.Split(new string[] { "src=\"/w/images/thumb/" }, StringSplitOptions.None);
                for (int i = 1; i < splt.Length; i++)
                {
                    string link = splt[i].Split('"')[0];
                    try
                    {
                        string[] imageInfo = link.Split('/');
                        string mapName = imageInfo[2].Split('.')[0];
                        if (IsOfficial(mapName))
                        {
                            string finalLink = $"https://developer.valvesoftware.com/w/images/{imageInfo[0]}/{imageInfo[1]}/{imageInfo[2]}";
                            MapIcons[mapName.ToLower()] = finalLink;
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static bool IsOfficial(string mapName)
        {
            string mapExtention = mapName.ToLower().Substring(0, 3);
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
