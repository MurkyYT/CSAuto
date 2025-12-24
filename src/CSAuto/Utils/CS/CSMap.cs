using System;
using System.Collections.Generic;
using System.Net;

namespace Murky.Utils.CS
{
    public static class CSMap
    {
        static readonly Dictionary<string, string> MapIcons = new Dictionary<string, string>();
        readonly static WebClient client = new WebClient();
        static CSMap()
        {
            client.Proxy = null;
            client.Headers.Add(HttpRequestHeader.Host, "developer.valvesoftware.com");
            client.Headers.Add(HttpRequestHeader.Accept, "*/*");
            client.Headers.Add(HttpRequestHeader.Cookie, "AkamaiEdge=true");
        }

        //public static void LoadMapIcons()
        //{
        //    lock (client)
        //    {
        //        try
        //        {
        //            string info = client.DownloadString("https://developer.valvesoftware.com/wiki/Counter-Strike_2/Maps#/media");
        //            string[] splt = info.Split(new string[] { "src=\"/w/images/thumb/" }, StringSplitOptions.None);
        //            for (int i = 1; i < splt.Length; i++)
        //            {
        //                string link = splt[i].Split('"')[0];
        //                try
        //                {
        //                    bool isNew = false;
        //                    string[] imageInfo = link.Split('/');
        //                    string mapName = imageInfo[2].Split('.')[0];
        //                    if (mapName.StartsWith("Map_icon_"))
        //                    {
        //                        mapName = mapName.Substring("Map_icon_".Length);
        //                        isNew = true;
        //                    }
        //                    mapName = mapName.ToLower();
        //                    if (IsOfficial(mapName))
        //                    {
        //                        if (!isNew && MapIcons.ContainsKey(mapName))
        //                            continue;
        //                        string finalLink = $"https://developer.valvesoftware.com/w/images/{imageInfo[0]}/{imageInfo[1]}/{imageInfo[2]}";
        //                        MapIcons[mapName] = finalLink;
        //                    }
        //                }
        //                catch { }
        //            }
        //        }
        //        catch { }
        //    }
        //}

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
                            if(RemoteFileExists(url))
                                MapIcons[mapName] = url;
                            else
                                throw new Exception();

                            return MapIcons[mapName];
                        }
                        catch { Log.WriteLine($"|CSMap.cs| Couldn't load official map icon for '{mapName}'"); }
                    }

                    if(result == null) 
                    { 
                        string info = client.DownloadString($"https://steamcommunity.com/workshop/browse/?appid=730&searchtext={mapName}");
                        string[] splt = info.Split(new string[] { "<div class=\"workshopBrowseItems\">" }, StringSplitOptions.RemoveEmptyEntries);
                        if (splt.Length > 1)
                        {
                            splt = info.Split(new string[] { "<img class=\"workshopItemPreviewImage  aspectratio_16x9\" src=\"" }, StringSplitOptions.RemoveEmptyEntries);
                            if (splt.Length > 1)
                            {
                                result = splt[1].Split('"')[0];
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
    }
}
