using System;
using System.Net;
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
                _client.Connect(_ip, _port);
                if (!_client.IsConnected)
                    throw new WebException("Connection refused");
                _client.DataReceived += OnDataReceived;
                Log.WriteLine($"|NetCon.cs| Successfully connected to netCon");
            }
            catch (Exception ex)
            {
                _client.Dispose();
                Log.WriteLine($"|NetCon.cs| Couldn't connet to netCon\n{ex.Message}\n{ex.StackTrace}");
            }
        }
        void OnDataReceived(byte[] receivedData)
        {
            string data = Encoding.UTF8.GetString(receivedData);
            if (data.Contains("vport 0] connected") && data.Contains("SDR server steamid"))
                MatchFound?.Invoke(this, new EventArgs());
            Log.debugWind?.Dispatcher.Invoke(new Action(() => { Log.debugWind.csConsoleOutput.Text += data; Log.debugWind.csConsoleOutput.ScrollToEnd(); }));
        }
        public void SendCommand(string command)
        {
            byte[] data = Encoding.ASCII.GetBytes(command + "\n");
            _ = _client.SendAsync(data);
            Log.WriteLine("|NetCon.cs| Sent: '" + command +"'");
        }

        public void Close()
        {
            try
            {
                _client.Disconnect();
                Log.WriteLine($"|NetCon.cs| Successfully closed netCon");
            }
            catch (SocketException ex)
            {
                Log.WriteLine($"|NetCon.cs| Couldn't close netCon\n{ex.Message}\n{ex.StackTrace}\n{ex.SocketErrorCode}");
            }
        }
    }
}
