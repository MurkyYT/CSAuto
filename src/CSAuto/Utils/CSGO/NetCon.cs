using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Murky.Utils.CSGO
{
    public class NetCon
    {
        const int BUFFER_SIZE = 1024 * 1024;
        private string _ip;
        private int _port;
        private TcpClient _client;
        private Thread _receiveThread;
        private byte[] _buffer = new byte[BUFFER_SIZE];
        public string IP { get { return _ip; } }
        public int Port { get { return _port; } }
        public event EventHandler MatchFound;
        public NetCon(string ip, int port,bool listenToConsole = false)
        {
            try
            {
                _ip = ip;
                _port = port;
                _client = new TcpClient(ip, port);
                Log.WriteLine($"[NetCon] : Successfully connected to netCon");
            }
            catch (SocketException ex)
            {
                Log.WriteLine($"[NetCon] : Couldn't connet to netCon\n{ex.Message}\n{ex.StackTrace}\n{ex.SocketErrorCode}");
            }
            if (listenToConsole)
            {
                _receiveThread = new Thread(ReceiveNetCon);
                _receiveThread.Start();
            }
        }
        public void ReceiveNetCon()
        {
            if (_client.Connected)
            {
                NetworkStream serverStream = _client.GetStream();
                while (true)
                {
                    if (serverStream.DataAvailable)
                    {
                        serverStream.Read(_buffer, 0, BUFFER_SIZE);
                        string data = Encoding.ASCII.GetString(_buffer);
                        if (data.Contains("vport 0] connected") && data.Contains("SDR server steamid"))
                            MatchFound?.Invoke(this, new EventArgs());
                    }
                }
            }
        }
        public void SendCommand(string command)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(command + "\n");
            _client.GetStream().Write(data, 0, data.Length);
            Log.WriteLine("[NetCon] Sent: '" + command +"'");
        }

        public void Close()
        {
            try
            {
                _client.Close();
                Log.WriteLine($"[NetCon] : Successfully closed netCon");
            }
            catch (SocketException ex)
            {
                Log.WriteLine($"[NetCon] : Couldn't close netCon\n{ex.Message}\n{ex.StackTrace}\n{ex.SocketErrorCode}");
            }
        }
    }
}
