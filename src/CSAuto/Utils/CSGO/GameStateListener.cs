using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Murky.Utils.CSGO
{
    public class GameStateListener
    {
        private readonly GameState _gameState;
        private HttpListener _listener;
        private readonly string GAMESTATE_PORT = "";
        private bool _serverRunning = false;
        private readonly AutoResetEvent _waitForConnection = new AutoResetEvent(false);
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

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:" + GAMESTATE_PORT + "/");
            Thread ListenerThread = new Thread(new ThreadStart(Run));
            try
            {
                _listener.Start();
            }
            catch (HttpListenerException)
            {
                return false;
            }
            _serverRunning = true;
            ListenerThread.Start();
            return true;
        }

        /// <summary>
        /// Stops listening for HTTP POST requests
        /// </summary>
        public void StopGSIServer()
        {
            if (!_serverRunning)
                return;
            _serverRunning = false;
            _listener.Close();
            (_listener as IDisposable).Dispose();
        }

        private void Run()
        {
            while (_serverRunning)
            {
                _listener.BeginGetContext(ReceiveGameState, _listener);
                _waitForConnection.WaitOne();
                _waitForConnection.Reset();
            }
            try
            {
                _listener.Stop();
            }
            catch (ObjectDisposedException)
            { /* _listener was already disposed, do nothing */ }
        }
        private void ReceiveGameState(IAsyncResult result)
        {
            HttpListenerContext context;
            try
            {
                context = _listener.EndGetContext(result);
            }
            catch (ObjectDisposedException)
            {
                // Listener was Closed due to call of Stop();
                return;
            }
            catch (HttpListenerException)
            {
                return;
            }
            finally
            {
                _waitForConnection.Set();
            }
            try
            {
                HttpListenerRequest request = context.Request;
                string JSON;

                using (Stream inputStream = request.InputStream)
                {
                    using (StreamReader sr = new StreamReader(inputStream))
                    {
                        JSON = sr.ReadToEnd();
                    }
                }
                using (HttpListenerResponse response = context.Response)
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";
                    response.Close();
                }
                _gameState.UpdateJson(JSON);
                OnReceive?.Invoke(this, new EventArgs());
            }
            catch { }
        }
    }
}
