using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Nager.TcpClient
{
    /// <summary>
    /// A simple TcpClient
    /// </summary>
    public class TcpClient : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationTokenRegistration _streamCancellationTokenRegistration;
        private readonly Task _dataReceiverTask;
        private readonly TcpClientKeepAliveConfig _keepAliveConfig;
        private readonly object _connectSyncLock = new object();
        private readonly object _switchStateSyncLock = new object();

        private readonly byte[] _receiveBuffer;

        private System.Net.Sockets.TcpClient _tcpClient;
        private bool _tcpClientInitialized;
        private Stream _stream;
        private bool _isConnected;

        /// <summary>
        /// Is client connected
        /// </summary>
        public bool IsConnected { get { return _isConnected; } }

        /// <summary>
        /// Event to call when the connection is established.
        /// </summary>
        public event Action Connected;

        /// <summary>
        /// Event to call when the connection is destroyed.
        /// </summary>
        public event Action Disconnected;

        /// <summary>
        /// Event to call when byte data has become available from the server.
        /// </summary>
        public event Action<byte[]> DataReceived;

        /// <summary>
        /// TcpClient
        /// </summary>
        /// <param name="clientConfig"></param>
        /// <param name="keepAliveConfig"></param>
        /// <param name="logger"></param>
        public TcpClient(
            TcpClientConfig clientConfig = default,
            TcpClientKeepAliveConfig keepAliveConfig = default)
        {
            this._cancellationTokenSource = new CancellationTokenSource();
            this._keepAliveConfig = keepAliveConfig;

            if (clientConfig == default)
            {
                clientConfig = new TcpClientConfig();
            }

            this._receiveBuffer = new byte[clientConfig.ReceiveBufferSize];

            this._streamCancellationTokenRegistration = this._cancellationTokenSource.Token.Register(() =>
            {
                if (this._stream == null)
                {
                    return;
                }

                this._stream.Close();
            });

            this._dataReceiverTask = Task.Run(async () => await this.DataReceiverAsync(this._cancellationTokenSource.Token), this._cancellationTokenSource.Token);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._cancellationTokenSource != null)
                {
                    if (!this._cancellationTokenSource.IsCancellationRequested)
                    {
                        this._cancellationTokenSource.Cancel();
                    }

                    this._cancellationTokenSource.Dispose();
                }
                if (this._dataReceiverTask != null)
                {
                    if (this._dataReceiverTask.Status == TaskStatus.Running)
                    {
                        this._dataReceiverTask.Wait(50);
                    }
                }
                this._streamCancellationTokenRegistration.Dispose();

                this.DisposeTcpClientAndStream();
            }
        }
        private void DisposeTcpClientAndStream()
        {
            if (this._stream != null)
            {
                if (this._stream.CanWrite || this._stream.CanRead || this._stream.CanSeek)
                {
                    this._stream?.Close();
                }
                this._stream?.Dispose();
            }

            if (this._tcpClientInitialized)
            {
                if (this._tcpClient != null)
                {
                    if (this._tcpClient.Connected)
                    {
                        this._tcpClient.Close();
                    }

                    this._tcpClient.Dispose();
                }

                this._tcpClientInitialized = false;
            }
        }

        private void PrepareStream()
        {
            if (this._tcpClient == null)
            {
                return;
            }

            this._stream = this._tcpClient.GetStream();

            if (this._keepAliveConfig != null)
            {
                if (this._tcpClient.SetKeepAlive(this._keepAliveConfig.KeepAliveTime, this._keepAliveConfig.KeepAliveInterval, this._keepAliveConfig.KeepAliveRetryCount))
                {
                }
                else
                {
                }
            }
        }

        private bool SwitchToConnected()
        {
            lock (this._switchStateSyncLock)
            {
                if (this._isConnected)
                {
                    return false;
                }

                this._isConnected = true;
                this.Connected?.Invoke();

                return true;
            }
        }

        private bool SwitchToDisconnected()
        {
            lock (this._switchStateSyncLock)
            {
                if (!this._isConnected)
                {
                    return false;
                }

                if (this._tcpClient != null && this._tcpClient.Connected)
                {
                    this._tcpClient.Close();
                }

                this._isConnected = false;                
                this.Disconnected?.Invoke();

                return true;
            }
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="ipAddressOrHostname"></param>
        /// <param name="port"></param>
        /// <param name="connectTimeoutInMilliseconds">default: 2s</param>
        public bool Connect(
            string ipAddressOrHostname,
            int port,
            int connectTimeoutInMilliseconds = 2000)
        {
            ipAddressOrHostname = ipAddressOrHostname ?? throw new ArgumentNullException(nameof(ipAddressOrHostname));

            if (this._isConnected)
            {
                return false;
            }

            if (this._tcpClientInitialized)
            {
                return false;
            }

            lock (this._connectSyncLock)
            {
                if (this._tcpClientInitialized)
                {
                    return false;
                }

                this._tcpClientInitialized = true;

                try
                {
                    this._tcpClient = new System.Net.Sockets.TcpClient();

                    IAsyncResult asyncResult = this._tcpClient.BeginConnect(ipAddressOrHostname, port, null, null);
                    var waitHandle = asyncResult.AsyncWaitHandle;

                    //Try connect with timeout
                    if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(connectTimeoutInMilliseconds), exitContext: false))
                    {
                        /*
                         * INFO
                         * Do not include a dispose for the waitHandle here this will cause an exception
                        */

                        this._tcpClient.Close();

                        this._tcpClientInitialized = false;
                        this._tcpClient.Dispose();

                        return false;
                    }

                    this._tcpClient.EndConnect(asyncResult);

                    waitHandle.Close();
                    waitHandle.Dispose();

                    this.PrepareStream();

                    this.SwitchToConnected();

                    return true;
                }
                catch
                {
                    this._tcpClientInitialized = false;
                    this._tcpClient?.Dispose();
                }
            }

            return false;
        }

#if (NET5_0_OR_GREATER)

        /// <summary>
        /// ConnectAsync
        /// </summary>
        /// <param name="ipAddressOrHostname"></param>
        /// <param name="port"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(
            string ipAddressOrHostname,
            int port,
            CancellationToken cancellationToken = default)
        {
            ipAddressOrHostname = ipAddressOrHostname ?? throw new ArgumentNullException(nameof(ipAddressOrHostname));

            if (this._isConnected)
            {
                return false;
            }

            this._tcpClient = new System.Net.Sockets.TcpClient();

            this._logger.LogDebug($"{nameof(ConnectAsync)} - Connecting");

            try
            {
                await this._tcpClient.ConnectAsync(ipAddressOrHostname, port, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this._logger.LogError(exception, $"{nameof(ConnectAsync)} - Cannot connect");
                return false;
            }

            this.PrepareStream();

            this._logger.LogInformation($"{nameof(ConnectAsync)} - Connected");
            this.SwitchToConnected();
            
            return true;
        }

#else

        /// <summary>
        /// ConnectAsync
        /// </summary>
        /// <param name="ipAddressOrHostname"></param>
        /// <param name="port"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(
            string ipAddressOrHostname,
            int port,
            CancellationToken cancellationToken = default
            )
        {
            ipAddressOrHostname = ipAddressOrHostname ?? throw new ArgumentNullException(nameof(ipAddressOrHostname));

            if (this._isConnected)
            {
                return false;
            }

            this._tcpClient = new System.Net.Sockets.TcpClient();

            try
            {
                var cancellationCompletionSource = new TaskCompletionSource<bool>();

                var task = this._tcpClient.ConnectAsync(ipAddressOrHostname, port);
                using (cancellationToken.Register(() => cancellationCompletionSource.TrySetResult(true)))
                {
                    if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                    {
                        return false;
                    }
                }

                if (task.Status == TaskStatus.Faulted)
                {
                    var exception = new Exception("Task faulted", task.Exception);
                    return false;
                }

            }
            catch
            {
                return false;
            }
            this.PrepareStream();
            this.SwitchToConnected();

            return true;
        }

#endif

        /// <summary>
        /// Disconnect
        /// </summary>
        public void Disconnect()
        {
            this.DisposeTcpClientAndStream();
            this.SwitchToDisconnected();
        }

        /// <summary>
        /// Send data async
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendAsync(
            byte[] data,
            CancellationToken cancellationToken = default)
        {
            if (this._stream == null)
            {
                return;
            }


#if (NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER)

            await this._stream.WriteAsync(data.AsMemory(0, data.Length), cancellationToken).ConfigureAwait(false);
            await this._stream.FlushAsync(cancellationToken).ConfigureAwait(false);

#else
            await this._stream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
            await this._stream.FlushAsync(cancellationToken).ConfigureAwait(false);
#endif
        }

        private async Task DataReceiverAsync(CancellationToken cancellationToken = default)
        {
            var defaultTimeout = TimeSpan.FromMilliseconds(100);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (this._tcpClient == null)
                {

                    await Task
                        .Delay(defaultTimeout, cancellationToken)
                        .ContinueWith(task => { }, CancellationToken.None)
                        .ConfigureAwait(false);

                    continue;
                }
                if (!this._tcpClient.Connected)
                {
                    this.SwitchToDisconnected();

                    await Task
                        .Delay(defaultTimeout, cancellationToken)
                        .ContinueWith(task => { }, CancellationToken.None)
                        .ConfigureAwait(false);

                    continue;
                }
                if (this._stream == null)
                {
                    await Task
                        .Delay(defaultTimeout, cancellationToken)
                        .ContinueWith(task => { }, CancellationToken.None)
                        .ConfigureAwait(false);

                    continue;
                }
                var readTaskSuccessful = await DataReadAsync(cancellationToken)
                    .ContinueWith(async task =>
                    {
                        if (task.IsCanceled)
                        {
                            return false;
                        }
                        if (task.IsFaulted)
                        {
                            if (this.IsKnownException(task.Exception))
                            {
                                this.SwitchToDisconnected();
                                return true;
                            }
                            this.SwitchToDisconnected();
                            return false;
                        }
                        byte[] data = task.Result;
                        if (data == null || data.Length == 0)
                        {
                            await Task
                            .Delay(defaultTimeout, cancellationToken)
                            .ContinueWith(task1 => { }, CancellationToken.None)
                            .ConfigureAwait(false);

                            //In this situation, the Docker Container tcp conncection is in a bad state
                            //infinite loop

                            this.SwitchToDisconnected();
                            return true;
                        }
                        if (this.DataReceived != null)
                        {
                            this.DataReceived?.Invoke(data);
                        }
                        else
                        {
                        }
                        return true;
                    }, cancellationToken)
                    .ContinueWith(task =>
                    {
                        if (task.IsCanceled)
                        {
                            return false;
                        }

                        return task.Result.Result;
                    }, CancellationToken.None)
                    .ConfigureAwait(false);

                if (!readTaskSuccessful)
                {
                    break;
                }
            }
        }

        private bool IsKnownException(Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                if (aggregateException.InnerException is IOException ioException)
                {
                    if (ioException.InnerException is SocketException socketException)
                    {
                        // Target device, network cable unplugged
                        if (socketException.SocketErrorCode == SocketError.TimedOut)
                        {
                            return true;
                        }

                        if (socketException.SocketErrorCode == SocketError.ConnectionReset)
                        {
                            return true;
                        }

                        // Target device is restarted
                        if (socketException.SocketErrorCode == SocketError.OperationAborted)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private async Task<byte[]> DataReadAsync(CancellationToken cancellationToken)
        {
            if (this._stream == null)
            {
                return Array.Empty<byte>();
            }

            if (!this._stream.CanRead)
            {
                return Array.Empty<byte>();
            }

            try
            {
#if (NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER)

                this._logger.LogTrace($"{nameof(DataReadAsync)} - Read data... (AsMemory)");

                var numberOfBytesToRead = await this._stream.ReadAsync(this._receiveBuffer.AsMemory(0, this._receiveBuffer.Length), cancellationToken).ConfigureAwait(false);
                if (numberOfBytesToRead == 0)
                {
                    return Array.Empty<byte>();
                }

                this._logger.LogTrace($"{nameof(DataReadAsync)} - NumberOfBytesToRead:{numberOfBytesToRead}");
                using var memoryStream = new MemoryStream();
                await memoryStream.WriteAsync(this._receiveBuffer.AsMemory(0, numberOfBytesToRead), cancellationToken).ConfigureAwait(false);
                return memoryStream.ToArray();

#else
                var numberOfBytesToRead = await this._stream.ReadAsync(this._receiveBuffer, 0, this._receiveBuffer.Length, cancellationToken).ConfigureAwait(false);
                if (numberOfBytesToRead == 0)
                {
                    return Array.Empty<byte>();
                }
                var memoryStream = new MemoryStream();
                await memoryStream.WriteAsync(this._receiveBuffer, 0, numberOfBytesToRead, cancellationToken).ConfigureAwait(false);
                return memoryStream.ToArray();
#endif
            }
            catch (ObjectDisposedException)
            {
            }
            catch (IOException)
            {
                throw;
            }
            catch
            {
                throw;
            }

            return Array.Empty<byte>();
        }
    }
}