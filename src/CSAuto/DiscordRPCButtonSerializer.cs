using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace CSAuto
{
    internal static class DiscordRPCButtonSerializer
    {
        public static string Path { get { return Directory.GetParent(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath).FullName; } }
        public static void Serialize(List<DiscordRPCButton> buttons)
        {
            string output = JsonConvert.SerializeObject(buttons);
            File.WriteAllText(Path + "\\buttons.json", output);
        }
        public static List<DiscordRPCButton> Deserialize()
        {
            if (File.Exists(Path))
                return JsonConvert.DeserializeObject<List<DiscordRPCButton>>(File.ReadAllText(Path));
            else
                return new List<DiscordRPCButton>();
        }
    }
}
