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
        private static int numThreads = 1;
        static void Main(string[] args)
        {
            SteamAPI.Init();
            int i;
            Thread[] servers = new Thread[numThreads];

            Console.WriteLine("\n*** Named pipe server stream with impersonation example ***\n");
            Console.WriteLine("Waiting for client connect...\n");
            for (i = 0; i < numThreads; i++)
            {
                servers[i] = new Thread(ServerThread);
                servers[i]?.Start();
            }
        }
        private static void ServerThread(object data)
        {


            int threadId = Thread.CurrentThread.ManagedThreadId;
            while (true)
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("csautopipe", PipeDirection.InOut))
                {
                    pipeServer.WaitForConnection();

                    Console.WriteLine("Client connected on thread[{0}].", threadId);
                    try
                    {
                        // Read the request from the client. Once the client has
                        // written to the pipe its security token will be available.

                        StreamString ss = new StreamString(pipeServer);

                        // Verify our identity to the connected client using a
                        // string that the client anticipates.

                        ss.WriteString("I am the one true server!");
                        ulong lobbysteamid = ulong.Parse(ss.ReadString());

                        CSteamID lobbyid = new CSteamID(lobbysteamid);
                        string res = $"{SteamMatchmaking.GetNumLobbyMembers(lobbyid)}/{SteamMatchmaking.GetLobbyMemberLimit(lobbyid)}({lobbyid})";
                        ss.WriteString(res);
                    }
                    // Catch the IOException that is raised if the pipe is broken
                    // or disconnected.
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

