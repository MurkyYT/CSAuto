using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CSAuto.Utils
{
    public static class Steam
    {
        /// <summary>
        /// Tries to get the game folder.
        /// </summary>
        /// <param name="game"></param>
        /// <returns>Game folder if found,  <see langword="null"/> if wasn't</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        // from - https://gist.github.com/moritzuehling/7f1c512871e193c0222f
        public static string GetGameDir(string game)
        {
            string steamPath = Steam.GetSteamPath();
            if (steamPath == null)
                throw new DirectoryNotFoundException("Couldn't find Steam path");
            string pathsFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

            if (!File.Exists(pathsFile))
                return null;

            List<string> libraries = new List<string>
            {
                Path.Combine(steamPath)
            };

            var pathVDF = File.ReadAllLines(pathsFile);
            // Okay, this is not a full vdf-parser, but it seems to work pretty much, since the 
            // vdf-grammar is pretty easy. Hopefully it never breaks. I'm too lazy to write a full vdf-parser though. 
            Regex pathRegex = new Regex(@"\""(([^\""]*):\\([^\""]*))\""");
            foreach (var line in pathVDF)
            {
                if (pathRegex.IsMatch(line))
                {
                    string match = pathRegex.Matches(line)[0].Groups[1].Value;

                    // De-Escape vdf. 
                    libraries.Add(match.Replace("\\\\", "\\"));
                }
            }

            foreach (var library in libraries)
            {
                string gamePatj = Path.Combine(library, $"steamapps\\common\\{game}");
                if (Directory.Exists(gamePatj))
                {
                    return gamePatj;
                }
            }

            return null;
        }
        /// <summary>
        /// Tries to get steam path.
        /// </summary>
        /// <returns>Steam path if found,  <see langword="null"/> if didnt</returns>
        public static string GetSteamPath()
        {
            string X86 = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "InstallPath", null);
            string X64 = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", null);
            return X86 ?? X64;
        }
        /// <summary>
        /// Tries to get the active SteamID3.
        /// </summary>
        /// <returns>0 if not found, active SteamID3 if found</returns>
        public static int GetCurrentSteamID3()
        {
            int steamID3 = 0;
            try
            {
                string connLogPath = GetSteamPath() + "\\logs\\connection_log.txt";
                string[] log = File.ReadAllLines(connLogPath);
                for (int i = log.Length - 1; i >= 0 && steamID3 == 0; i--)
                {
                    string line = log[i];
                    string[] split = line.Split(new string[] { "[U:1:" }, StringSplitOptions.None);
                    if (split.Length > 1)
                        steamID3 = int.Parse(split[1].Split(']')[0]);
                }
            }
            catch (IOException)
            {
                Log.WriteLine("Couldn't read SteamID3 from log, reading from Registry");
                steamID3 = (int)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam\\ActiveProcess", "ActiveUser", 0);
            }
            return steamID3;
        }
        /// <summary>
        /// Gets the launch options of the app with the id you entered.
        /// </summary>
        /// <param name="appID">The id of the app</param>
        /// <param name="result">Out's the launch options if found any</param>
        /// <returns>index of the line the options were found,-1 if werent found</returns>
        public static int GetLaunchOptions(int appID,out string result)
        {
            result = null;
            int? steamID3 = GetCurrentSteamID3();
            string steamPath = GetSteamPath();
            if (steamPath == null)
                return -1;
            if (steamID3 == 0)
                return -1;
            string localconfPath = $"{steamPath}\\userdata\\{steamID3}\\config\\localconfig.vdf";
            string[] localconfFile = File.ReadAllLines(localconfPath);
            int index = GetAppIDIndex(appID, localconfFile);
            if(index == -1)
                return -1;
            int length = GetAppOptLength(index, localconfFile);
            // skip app id number
            index++;
            string[] appOpts = GetAppOpts(index, localconfFile, length);
            int laucnOptIndex = GetLaunchOptionsIndex(length, appOpts, out bool foundLaunchOptions);
            index += laucnOptIndex;
            if (foundLaunchOptions)
            {
                result =
                    appOpts[laucnOptIndex].Split(new string[] { "\"LaunchOptions\"" }, StringSplitOptions.None)[1].Split('"')[1].Split('"')[0];
                return index;
            }
            return -1;
        }
        /// <summary>
        /// Sets the launch options even if there weren't any.
        /// </summary>
        /// <param name="appID">The id of the app</param>
        /// <param name="value">The value to set in the launch options</param>
        /// <returns>returns <see langword="true"/> if succeeded <see langword="false"/> if didn't</returns>
        public static bool SetLaunchOptions(int appID, string value)
        {
            int steamID3 = GetCurrentSteamID3();
            string steamPath = GetSteamPath();
            if (steamPath == null)
                return false;
            if (steamID3 == 0)
                return false;
            string localconfPath = $"{steamPath}\\userdata\\{steamID3}\\config\\localconfig.vdf";
            string[] localconfFile = File.ReadAllLines(localconfPath);
            int appIDIndex = GetAppIDIndex(730, localconfFile);
            if (appIDIndex == -1)
                return false;
            int launchOptionsIndex = GetLaunchOptions(appID, out string launchOptions);
            if (launchOptionsIndex != -1)
                localconfFile[launchOptionsIndex] = $"\t\t\t\t\t\t\"LaunchOptions\"\t\t\"{value}\"";
            else
            {
                string[] temp = new string[localconfFile.Length];
                Array.Copy(localconfFile, temp, localconfFile.Length);
                localconfFile = new string[localconfFile.Length + 1];
                bool addedOptions = false;
                for (int i = 0; i < localconfFile.Length; i++)
                {
                    if (i == appIDIndex + 2)
                    {
                        localconfFile[i] = $"\t\t\t\t\t\t\"LaunchOptions\"\t\t\"{value}\"";
                        addedOptions = true;
                    }
                    else if(!addedOptions)
                        localconfFile[i] = temp[i];
                    else if (addedOptions)
                        localconfFile[i] = temp[i-1];
                }
                Log.WriteLine($"No launch options found, adding \'\"LaunchOptions\" \t\t\"{value}\"\' at index {appIDIndex+2}");
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < localconfFile.Length; i++)
            {
                builder.AppendLine(localconfFile[i]);
            }
            using (FileStream fs = File.Create(localconfPath))
            {
                Byte[] title = new UTF8Encoding(true).GetBytes(builder.ToString());
                fs.Write(title, 0, title.Length);
            }
            Log.WriteLine($"Successfuly set LaunchOptions to \'{value}\'");
            return true;
        }
        private static int GetLaunchOptionsIndex (int length, string[] appOpts,out bool succes)
        {
            succes = false;
            for (int i = 0; i < length; i++)
            {
                string line = appOpts[i].Trim();
                if(line.Length > "\"LaunchOptions\"".Length)
                if (line.Substring(0, "\"LaunchOptions\"".Length) == "\"LaunchOptions\"")
                {
                    succes = true;
                    return i;
                }
            }
            return 0;
        }

        private static int GetAppIDIndex(int appID, string[] localconfFile)
        {
            int index = -1;
            for (int i = 0; i < localconfFile.Length && index == -1; i++)
            {
                if (localconfFile[i] == $"\t\t\t\t\t\"{appID}\"")
                    index = i;
            }
            return index;
        }

        private static string[] GetAppOpts(int index, string[] localconfFile, int length)
        {
            string[] res = new string[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = localconfFile[index + i];
            }
            return res;
        }

        private static int GetAppOptLength(int index, string[] localconfFile)
        {
            int length = 0;
            int findEndInd = index;
            while (localconfFile[findEndInd] != "\t\t\t\t\t}")
            {
                findEndInd++;
                length++;
            }
            return length;
        }
    }
}
