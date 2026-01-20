using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Murky.Utils.CS
{
    public static class CSMap
    {
        static readonly Dictionary<string, string> MapIcons = new Dictionary<string, string>();
        static readonly Dictionary<string, string> MapDisplayNames = new Dictionary<string, string>();
        readonly static WebClient client = new WebClient();
        private static bool RemoteFileExists(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
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
            lock (client)
            {
                try
                {
                    string result = null;
                    if (MapIcons.ContainsKey(mapName))
                        return MapIcons[mapName];
                    else if (IsOfficial(mapName))
                    {
                        try
                        {
                            string url = $"https://raw.githubusercontent.com/MurkyYT/cs2-map-icons/main/images/{mapName}.png";
                            if (RemoteFileExists(url))
                                MapIcons[mapName] = url;
                            else
                                throw new Exception();

                            return MapIcons[mapName];
                        }
                        catch { Log.WriteLine($"|CSMap.cs| Couldn't load official map icon for '{mapName}'"); }
                    }

                    if (result == null)
                    {
                        string info = client.DownloadString($"https://steamcommunity.com/workshop/browse/?appid=730&searchtext={mapName}");
                        string[] splt = info.Split(new string[] { "<div class=\"workshopBrowseItems\">" }, StringSplitOptions.RemoveEmptyEntries);
                        if (splt.Length > 1)
                        {
                            splt = info.Split(new string[] { "<img class=\"workshopItemPreviewImage  aspectratio_16x9\" src=\"" }, StringSplitOptions.RemoveEmptyEntries);
                            if (splt.Length > 1)
                            {
                                result = splt[1].Split('"')[0].Split(new string[] { "/?" }, StringSplitOptions.None)[0] + "/";
                                MapIcons[mapName] = result;
                                return result;
                            }
                        }
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public static string GetDisplayName(string map)
        {
            lock (client)
            {
                if (MapDisplayNames.ContainsKey(map))
                    return MapDisplayNames[map];

                string info = client.DownloadString($"https://raw.githubusercontent.com/MurkyYT/cs2-map-icons/refs/heads/main/data/available.json");

                JObject json = (JObject)JObject.Parse(info)["maps"];

                string result = map;

                if (json.ContainsKey(map))
                    result = json[map]["display_name"].ToString();

                MapDisplayNames[map] = result;
                return result;
            }
        }
    }
}
