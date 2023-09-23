using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace CSAuto
{
    public static class DiscordRPCButtonSerializer
    {
        public static string Path { get { return Directory.GetParent(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath).FullName; } }
        public static void Serialize(List<DiscordRPCButton> buttons)
        {
            string output = JsonConvert.SerializeObject(buttons);
            File.WriteAllText(Path + "\\buttons.json", output);
        }
        public static List<DiscordRPCButton> Deserialize()
        {
            if (File.Exists(Path + "\\buttons.json"))
                return JsonConvert.DeserializeObject<List<DiscordRPCButton>>(File.ReadAllText(Path + "\\buttons.json"));
            else
                return new List<DiscordRPCButton>();
        }
    }
}
