using System;
using System.IO.Pipes;
using System.IO;
using System.Text;
using System.Threading;
using Steamworks;
using CSAuto;

namespace SteamAPIServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            InitializeSteamAPI();
            new Thread(ServerThread).Start();
        }
        static void InitializeSteamAPI()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string path = currentDir + "\\steam_appid.txt";
            if (!File.Exists(path))
            {
                using (FileStream fs = File.Create(path))
                {
                    Byte[] title = new UTF8Encoding(true).GetBytes("730");
                    fs.Write(title, 0, title.Length);
                }
            }
            SteamAPI.Init();
            File.Delete(path);
        }
        private static void ServerThread(object data)
        {
            while (true)
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("csautopipe", PipeDirection.InOut))
                {
                    pipeServer.WaitForConnection();
                    try
                    {
                        StreamString ss = new StreamString(pipeServer);
                        ss.WriteString("I am the one true server!");
                        ulong lobbysteamid = ulong.Parse(ss.ReadString());
                        CSteamID lobbyid = new CSteamID(lobbysteamid);
                        string res = $"{SteamMatchmaking.GetNumLobbyMembers(lobbyid)}/{SteamMatchmaking.GetLobbyMemberLimit(lobbyid)}({lobbyid})";
                        ss.WriteString(res);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ERROR: {0}", e.Message);
                    }
                }
            }
        }
    }
}

