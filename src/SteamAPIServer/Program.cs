using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;

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
    // Defines the data protocol for reading and writing strings on our stream
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}

