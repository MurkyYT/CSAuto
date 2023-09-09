using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace CSAuto
{
    internal static class DiscordRPCButtonSerializer
    {
        public static void Serialize(List<DiscordRPCButton> buttons)
        {
            string output = JsonConvert.SerializeObject(buttons);
            string path = Directory.GetParent(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath).FullName;
            File.WriteAllText(path+"\\buttons.json", output);
        }
        public static List<DiscordRPCButton> Deserialize()
        {
            string path = Directory.GetParent(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath).FullName+"\\buttons.json";
            if (File.Exists(path))
                return JsonConvert.DeserializeObject<List<DiscordRPCButton>>(File.ReadAllText(path));
            else
                return new List<DiscordRPCButton>();
        }
    }
}
