using System;
using System.Net.Sockets;
using System.Text;
using TcpClient = Nager.TcpClient.TcpClient;

namespace Murky.Utils.CSGO
{
    public class NetCon
    {
        private string _ip;
        private int _port;
        private TcpClient _client;
        public string IP { get { return _ip; } }
        public int Port { get { return _port; } }
        public event EventHandler MatchFound;
        public NetCon(string ip, int port)
        {
            try
            {
                _ip = ip;
                _port = port;
                _client = new TcpClient();
                _client.DataReceived += OnDataReceived;
                _client.Connect(_ip, _port);
                Log.WriteLine($"[NetCon] : Successfully connected to netCon");
            }
            catch (SocketException ex)
            {
                Log.WriteLine($"[NetCon] : Couldn't connet to netCon\n{ex.Message}\n{ex.StackTrace}\n{ex.SocketErrorCode}");
            }
        }
        void OnDataReceived(byte[] receivedData)
        {
            string data = Encoding.ASCII.GetString(receivedData);
            if (data.Contains("vport 0] connected") && data.Contains("SDR server steamid"))
                MatchFound?.Invoke(this, new EventArgs());
        }
        public void SendCommand(string command)
        {
            Byte[] data = Encoding.ASCII.GetBytes(command + "\n");
            _ = _client.SendAsync(data);
            Log.WriteLine("[NetCon] Sent: '" + command +"'");
        }

        public void Close()
        {
            try
            {
                _client.Disconnect();
                Log.WriteLine($"[NetCon] : Successfully closed netCon");
            }
            catch (SocketException ex)
            {
                Log.WriteLine($"[NetCon] : Couldn't close netCon\n{ex.Message}\n{ex.StackTrace}\n{ex.SocketErrorCode}");
            }
        }
    }
}
