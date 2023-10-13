using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace CSAuto
{
    public class DiscordRPCButton
    {
        public string Label { get; set; }
        public string Url { get; set; }
    }
    public static class DiscordRPCButtonSerializer
    {
        public static string Path { get { return Directory.GetParent(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath).FullName; } }
        public static void Serialize(List<DiscordRPCButton> buttons)
        {
            string output = JsonConvert.SerializeObject(buttons);
            try
            {
                File.WriteAllText(Path + "\\buttons.json", output);
            }
            catch { }
            (App.Current as App).settings.Set("DiscordButtons", output);
        }
        public static List<DiscordRPCButton> DeserializeOld()
        {
            if (File.Exists(Path + "\\buttons.json"))
                return JsonConvert.DeserializeObject<List<DiscordRPCButton>>(File.ReadAllText(Path + "\\buttons.json"));
            else
                return new List<DiscordRPCButton>();
        }
        public static List<DiscordRPCButton> Deserialize()
        {
            if ((App.Current as App).settings["DiscordButtons"] != null)
                return JsonConvert.DeserializeObject<List<DiscordRPCButton>>((App.Current as App).settings["DiscordButtons"]);
            else
                return new List<DiscordRPCButton>();
        }
    }
}
