using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Preferences;
using Android.Systems;
using Commands = CSAuto.Shared.NetworkTypes.Commands;
using Android.Util;
using AndroidX.Core.App;
using Java.Lang;
using Java.Util.Prefs;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Exception = System.Exception;
using Thread = Java.Lang.Thread;
using static Android.Provider.Telephony.Mms;

namespace CSAuto_Mobile
{
    [Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeDataSync)]
    public class ServerService : Service
    {
        static readonly string? TAG = typeof(ServerService).FullName;

        bool isStarted;
        IPAddress? myIpAddress;
        TcpClient? client;
        Thread? thread;
        int lastRound = -1;
        public override void OnCreate()
        {
            base.OnCreate();
            Log.Info(TAG, "OnCreate: the service is initializing.");
            byte[]? addr = GetMyIpAddress();
            if(addr != null) { myIpAddress = new IPAddress(addr); }
        }
        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            if (intent == null)
                return StartCommandResult.Sticky;
            if (intent.Action.Equals(Constants.ACTION_START_SERVICE))
            {
                if (isStarted)
                {
                    Log.Info(TAG, "OnStartCommand: The service is already running.");
                }
                else
                {
                    Log.Info(TAG, "OnStartCommand: The service is starting.");
                    CreateNotificationChannel();
                    
                    thread = new Thread(StartServer);
                    thread.Start();
                    isStarted = true;
                }
                MainActivity.Instance.stopServiceButton.Enabled = true;
                MainActivity.Instance.startServiceButton.Enabled = false;
            }
            else if (intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
            {
                Log.Info(TAG, "OnStartCommand: The service is stopping.");
                StopClient();
                StopForeground(StopForegroundFlags.Remove);
                StopSelf();
                thread?.Join();
                MainActivity.Instance.stopServiceButton.Enabled = false;
                MainActivity.Instance.startServiceButton.Enabled = true;
                isStarted = false;
            }
            else if (intent.Action.Equals(Constants.ACTION_RESTART_TIMER))
            {
                Log.Info(TAG, "OnStartCommand: Restarting the timer.");
            }

            SaveState();

            return StartCommandResult.Sticky;
        }

        private void SaveState()
        {
            Preferences? prefs = Preferences.UserRoot();
            prefs.PutBoolean(Constants.SERVICE_STARTED_KEY, isStarted);
        }

        private static string CleanMessage(byte[] bytes)
        {
            string message = Encoding.UTF8.GetString(bytes,1,bytes.Length-1);
            return message;
        }
        public long CastIp(byte[] ip)
        {
            // This restriction is implicit in your existing code, but
            // it would currently just lose data...
            if (ip.Length != 4)
            {
                throw new ArgumentException("Must be an IPv4 address");
            }
            int networkOrder = BitConverter.ToInt32(ip, 0);
            networkOrder = BinaryPrimitives.ReverseEndianness(networkOrder);
            return (uint)IPAddress.HostToNetworkOrder(networkOrder);
        }
        private void StartServer()
        {
            try
            {
                if (myIpAddress != null)
                {
                    client = new TcpClient();
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(Globals.IP.Trim()), int.Parse(Globals.PORT));
                    client.Connect(endPoint);
                    RegisterForegroundService();
                    while (true)
                    {
                        if (isStarted && MainActivity.Instance.stopServiceButton != null && !MainActivity.Instance.stopServiceButton.Enabled)
                        {
                            MainActivity.Instance.stopServiceButton.Enabled = true;
                            MainActivity.Instance.startServiceButton.Enabled = false;
                        }
                        // Receive message.
                        Stream stream = client.GetStream();
                        while (!stream.IsDataAvailable()) { }
                        byte[] buf = new byte[4];
                        stream.ReadExactly(buf, 0, 4);
                        uint length = BitConverter.ToUInt32(buf, 0);
                        buf = new byte[length];
                        stream.ReadExactly(buf, 0, (int)length);
                        string message = CleanMessage(buf);
                        Commands command = (Commands)buf[0];
                        Log.Debug(TAG, $"Received message: '{message}', command: '{command}'");
                        //if (MainActivity.Active)
                        //{
                        //    MainActivity.Instance.RunOnUiThread(() =>
                        //    {
                        //        new Runnable(() =>
                        //        {
                        //            MainActivity.Instance.outputText.Text += $"[{DateTime.Now:HH:mm:ss}] Received message: '{message}', command: '{command}'\n";
                        //        }).Run();
                        //    });
                        //}
                        switch (command)
                        {
                            case Commands.AcceptedMatch:
                                ShowNotification(Resources.GetString(Resource.String.app_name), message, Constants.ACCEPTED_MATCH_NOTIFICATION_ID, Constants.ACCEPTED_MATCH_CHANNEL);
                                break;
                            case Commands.LoadedOnMap:
                                ShowNotification(Resources.GetString(Resource.String.app_name), message, Constants.LOADED_ON_MAP_NOTIFICATION_ID, Constants.LOADED_ON_MAP_CHANNEL_ID);
                                break;
                            case Commands.LoadedInLobby:
                                ShowNotification(Resources.GetString(Resource.String.app_name), message, Constants.IN_LOBBY_NOTIFICATION_ID, Constants.LOADED_TO_LOBBY_CHANNEL);
                                break;
                            case Commands.Connected:
                                ShowNotification(Resources.GetString(Resource.String.app_name), message, Constants.CONNECTED_NOTIFICATION_ID, Constants.SERVICE_CHANNEL_ID);
                                break;
                            case Commands.Crashed:
                                ShowNotification(Resources.GetString(Resource.String.app_name), message, Constants.CRASHED_NOTIFICATION_ID, Constants.CRASHED_CHANNEL);
                                break;
                            case Commands.Bomb:
                                ShowNotification(Resources.GetString(Resource.String.app_name), message, Constants.BOMB_NOTIFICATION_ID, Constants.BOMB_CHANNEL, NotificationPriority.Default);
                                if (MainActivity.Active)
                                {
                                    MainActivity.Instance.RunOnUiThread(() =>
                                    {
                                        new Runnable(() =>
                                        {
                                            MainActivity.Instance.bombState.Text = message;
                                        }).Run();
                                    });
                                }
                                break;
                            case Commands.Clear:
                                //if (MainActivity.Active)
                                //{
                                //    MainActivity.Instance.RunOnUiThread(() =>
                                //    {
                                //        new Runnable(() =>
                                //        {
                                //            MainActivity.Instance.state.Text = "";
                                //            MainActivity.Instance.details.Text = "";
                                //            MainActivity.Instance.bombState.Text = "";
                                //        }).Run();
                                //    });
                                //}
                                break;
                            case Commands.GameState:
                                if (MainActivity.Active)
                                {
                                    Murky.Utils.CSGO.GameState gameState = new Murky.Utils.CSGO.GameState(message);
                                    MainActivity.Instance.RunOnUiThread(() =>
                                    {
                                        new Runnable(() =>
                                        {
                                            if(lastRound != gameState.Round.CurrentRound)
                                            {
                                                MainActivity.Instance.bombState.Text = "";
                                                lastRound = gameState.Round.CurrentRound;
                                            }
                                            MainActivity.Instance.inGame = gameState.Match.Map != null;
                                            MainActivity.Instance.UpdateLayout();
                                            MainActivity.Instance.roundStateText.Text = gameState.Round.Phase.ToString();
                                            MainActivity.Instance.ctScoreText.Text = gameState.Match.CTScore.ToString();
                                            MainActivity.Instance.tScoreText.Text = gameState.Match.TScore.ToString();
                                            MainActivity.Instance.mapText.Text = $"Map: {gameState.Match.Map}";
                                            if(gameState.Player != null)
                                                MainActivity.Instance.playerText.Text = 
                                                    $"{gameState.Player.Name}\nKills: {gameState.Player.Kills}\nDeaths: {gameState.Player.Deaths}\n K\\D: {System.Math.Round((float)gameState.Player.Kills / (gameState.Player.Deaths == 0? 1: gameState.Player.Deaths), 3)}\n MVP's: {gameState.Player.MVPS}";
                                        }).Run();
                                    });
                                }
                                break;
                            default:
                                break;
                        }
                        //MainActivity.Instance.RunOnUiThread(() => {
                        //    MainActivity.Instance.outputText.Text = clearResponse;
                        //});
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    Toast.MakeText(Application.Context, "Couldn't start client, ip is null",ToastLength.Short).Show();
                }
            }
            catch (SocketException ex)
            {
                ShowNotification("Socket exception", $"{ex.Message}", Constants.SOCKET_EXCEPTION_NOTIFICATION_ID, Constants.SERVICE_CHANNEL_ID);
                client.Close();
            }
            catch (ObjectDisposedException ex)
            {
                ShowNotification("Disposed exception", $"{ex.Message}", Constants.DISPOSED_EXCEPTION_NOTIFICATION_ID, Constants.SERVICE_CHANNEL_ID);
            }
            catch (Exception ex) { ShowNotification("Error ocurred", $"{ex.GetType()},{ex.StackTrace} - {ex.Message}", Constants.ERROR_NOTIFICATION_ID, Constants.SERVICE_CHANNEL_ID); }
            var stopServiceIntent = new Intent(this, GetType());
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
            MainActivity.Instance.StartService(stopServiceIntent);
            SaveState();
        }
        private static byte[]? GetMyIpAddress()
        {
            ConnectivityManager? connectivityManager = (ConnectivityManager?)Application.Context.GetSystemService(ConnectivityService);
            if (connectivityManager != null && connectivityManager is ConnectivityManager)
            {
                IList<LinkAddress> addresses = connectivityManager.GetLinkProperties(connectivityManager.ActiveNetwork).LinkAddresses;
                LinkAddress address = addresses.Where(x => x.Address.HostAddress.Contains('.')).ElementAt(0);
                return address.Address.GetAddress();
            }
            return null;
        }

        void ShowNotification(string title,string description,int id,string channel_id, NotificationPriority priority= NotificationPriority.High)
        {
            var builder = new NotificationCompat.Builder(this, channel_id)
                          .SetAutoCancel(true) // Dismiss the notification from the notification area when the user clicks on it
                          .SetContentTitle(title) // Set the title
                          .SetNumber(id) // Display the count in the Content Info
                          .SetSmallIcon(Resource.Mipmap.ic_launcher) // This is the icon to display
                          .SetContentText(description); // the message to display.
            builder.SetPriority((int)priority);
            Notification? notification = builder.Build();
            // Turn on sound if the sound switch is on:                 
            notification.Visibility = NotificationVisibility.Public;
            // Turn on vibrate if the sound switch is on:                 
            notification.Vibrate = Array.Empty<long>();
            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(id, notification);
        }
        public override IBinder? OnBind(Intent? intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder that would
            // allow access to the GetFormattedStamp() method.
            return null;
        }
        public override void OnDestroy()
        {
            // We need to shut things down.
            Log.Info(TAG, "OnDestroy: The started service is shutting down.");

            // Stop the handler.
            StopClient();

            // Remove the notification from the status bar.
            NotificationManager? notificationManager = (NotificationManager?)GetSystemService(NotificationService);
            notificationManager.Cancel(Constants.SERVICE_RUNNING_NOTIFICATION_ID);
            isStarted = false;
            SaveState();
            base.OnDestroy();
        }

        private void StopClient()
        {
            try
            {
                client?.Close();
                thread = null;
            }
            catch { }
        }
#pragma warning disable CA1416 // Validate platform compatibility
        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification 
                // channel on older versions of Android.
                return;
            }

            var name = "Service Status";
            var description = "No description";

            var channel = new NotificationChannel(Constants.SERVICE_CHANNEL_ID, name, NotificationImportance.Default)
            {
                Description = description
            };
            channel.SetSound(null, null);
            NotificationManager? notificationManager = (NotificationManager?)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
            name = "Loaded on map";
            description = "No description";
            channel = new NotificationChannel(Constants.LOADED_ON_MAP_CHANNEL_ID, name, NotificationImportance.High)
            {
                Description = description
            };
            channel.SetSound(null, null);
            notificationManager.CreateNotificationChannel(channel);
            name = "Accepted match";
            description = "No description";
            channel = new NotificationChannel(Constants.ACCEPTED_MATCH_CHANNEL, name, NotificationImportance.High)
            {
                Description = description
            };
            channel.SetSound(null, null);
            notificationManager.CreateNotificationChannel(channel);
            name = "Back in lobby";
            description = "No description";
            channel = new NotificationChannel(Constants.LOADED_TO_LOBBY_CHANNEL, name, NotificationImportance.High)
            {
                Description = description
            };
            channel.SetSound(null, null);
            notificationManager.CreateNotificationChannel(channel);

            name = "Game crahsed";
            description = "No description";
            channel = new NotificationChannel(Constants.CRASHED_CHANNEL, name, NotificationImportance.High)
            {
                Description = description
            };
            channel.SetSound(null, null);
            notificationManager.CreateNotificationChannel(channel);

            name = "Bomb information";
            description = "No description";
            channel = new NotificationChannel(Constants.BOMB_CHANNEL, name, NotificationImportance.Default)
            {
                Description = description
            };
            channel.SetSound(null, null);
            notificationManager.CreateNotificationChannel(channel);
        }
#pragma warning restore CA1416 // Validate platform compatibility
        void RegisterForegroundService()
        {
            var notification = new NotificationCompat.Builder(this, Constants.SERVICE_CHANNEL_ID)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText($"Client connected to {Globals.IP}:{Globals.PORT}")
                .SetSmallIcon(Resource.Mipmap.ic_launcher)
                .SetContentIntent(BuildIntentToShowMainActivity())
                .SetOngoing(true)
                .AddAction(BuildStopServiceAction())
                .Build();              

            notification.Vibrate = Array.Empty<long>();

            if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Tiramisu)
            {
                StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);
            }
            else
            {
                StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification, Android.Content.PM.ForegroundService.TypeDataSync);
            }
            
        }

        /// <summary>
        /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
        /// user taps on the notification; it will take them to the main activity of the app.
        /// </summary>
        /// <returns>The content intent.</returns>
        PendingIntent? BuildIntentToShowMainActivity()
        {
            var notificationIntent = new Intent(this, typeof(MainActivity));
            notificationIntent.SetAction(Constants.ACTION_MAIN_ACTIVITY);
            notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
            notificationIntent.PutExtra(Constants.SERVICE_STARTED_KEY, true);

            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Mutable);
            return pendingIntent;
        }
        /// <summary>
        /// Builds the Notification.Action that will allow the user to stop the service via the
        /// notification in the status bar
        /// </summary>
        /// <returns>The stop service action.</returns>
        NotificationCompat.Action BuildStopServiceAction()
        {
            var stopServiceIntent = new Intent(this, GetType());
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
            var stopServicePendingIntent = PendingIntent.GetService(this, 0, stopServiceIntent, PendingIntentFlags.Mutable);

            var builder = new NotificationCompat.Action.Builder(Android.Resource.Drawable.IcMediaPause,
                                                          GetText(Resource.String.stop_service),
                                                          stopServicePendingIntent);
            return builder.Build(); 
        }
    }
}