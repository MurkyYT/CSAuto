using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Murky.Utils.CS
{
    public class GameStateListener
    {
        private readonly GameState _gameState;
        private TcpListener _listener;
        private readonly string GAMESTATE_PORT = "";
        private volatile bool _serverRunning = false;
        private Thread _listenerThread;

        public bool ServerRunning { get { return _serverRunning; } }
        public event EventHandler OnReceive;

        public GameStateListener(ref GameState gameState, string port)
        {
            _gameState = gameState;
            GAMESTATE_PORT = port;
        }

        public bool StartGSIServer()
        {
            if (_serverRunning)
                return false;

            try
            {
                int port = int.Parse(GAMESTATE_PORT);
                _listener = new TcpListener(IPAddress.Loopback, port);
                _listener.Start();

                _serverRunning = true;
                _listenerThread = new Thread(new ThreadStart(Run));
                _listenerThread.Start();

                Log.WriteLine($"TCP Listener started on port {port}");
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Failed to start TCP listener: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stops listening for TCP connections
        /// </summary>
        public void StopGSIServer()
        {
            if (!_serverRunning)
                return;

            _serverRunning = false;

            try
            {
                _listener?.Stop();
            }
            catch { }

            if (_listenerThread != null && _listenerThread.IsAlive)
            {
                _listenerThread.Join(TimeSpan.FromSeconds(5));
            }
        }

        private void Run()
        {
            while (_serverRunning)
            {
                try
                {
                    if (_listener.Pending())
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        Thread clientThread = new Thread(() => HandleClient(client));
                        clientThread.IsBackground = true;
                        clientThread.Start();
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (SocketException ex)
                {
                    if (_serverRunning)
                    {
                        Log.WriteLine($"Socket error: {ex.Message}");
                    }
                    break;
                }
                catch (Exception ex)
                {
                    if (_serverRunning)
                    {
                        Log.WriteLine($"Error in listener loop: {ex.Message}");
                    }
                    break;
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] chunk = new byte[1024];
                        int bytesRead;

                        stream.ReadTimeout = 5000;

                        while (stream.DataAvailable)
                        {
                            bytesRead = stream.Read(chunk, 0, chunk.Length);
                            if (bytesRead > 0)
                            {
                                ms.Write(chunk, 0, bytesRead);
                            }
                        }

                        string receivedData = Encoding.UTF8.GetString(ms.ToArray());

                        if (!string.IsNullOrEmpty(receivedData))
                        {
                            string json = ExtractJson(receivedData);

                            if (!string.IsNullOrEmpty(json))
                            {
                                _gameState.UpdateJson(json);
                                OnReceive?.Invoke(this, new EventArgs());
                            }

                            string response = "HTTP/1.1 200 OK\r\nContent-Length: 0\r\n\r\n";
                            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                            stream.Write(responseBytes, 0, responseBytes.Length);
                            stream.Flush();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error handling GSI client: {ex.Message}");
            }
        }

        private string ExtractJson(string rawData)
        {
            try
            {
                int jsonStart = rawData.IndexOf("\r\n\r\n");
                if (jsonStart >= 0)
                {
                    return rawData.Substring(jsonStart + 4).Trim();
                }

                if (rawData.TrimStart().StartsWith("{"))
                {
                    return rawData.Trim();
                }

                return rawData;
            }
            catch
            {
                return rawData;
            }
        }
    }
}