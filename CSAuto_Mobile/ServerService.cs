using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using System;
using System.Net.Sockets;
using System.Net;
using Encoding = System.Text.Encoding;
using Android.Net.Wifi;
using Context = Android.Content.Context;
using Xamarin.Essentials;
using AndroidX.Core.App;
using Android.Preferences;
using CSAuto;
using Murky.Utils.CSGO;
using System.Threading;
using System.Threading.Tasks;

namespace CSAuto_Mobile
{
    /// <summary>
	/// This is a sample started service. When the service is started, it will log a string that details how long 
	/// the service has been running (using Android.Util.Log). This service displays a notification in the notification
	/// tray while the service is active.
	/// </summary>
	[Service]
    public class ServerService : Service
    {
        static readonly string TAG = typeof(ServerService).FullName;

        bool isStarted;
        Context currentContext = Platform.CurrentActivity;
        IPAddress myIpAddress;
        TcpListener listener;
        public override void OnCreate()
        {
            base.OnCreate();
            Log.Info(TAG, "OnCreate: the service is initializing.");
            myIpAddress = new IPAddress(GetMyIpAddress());
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
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
                    RegisterForegroundService();
                    new Thread(StartServer).Start();
                    isStarted = true;
                }
                MainActivity.Instance.stopServiceButton.Enabled = true;
                MainActivity.Instance.startServiceButton.Enabled = false;
            }
            else if (intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
            {
                Log.Info(TAG, "OnStartCommand: The service is stopping.");
                StopServer();
                StopForeground(true);
                StopSelf();
                MainActivity.Instance.stopServiceButton.Enabled = false;
                MainActivity.Instance.startServiceButton.Enabled = true;
                isStarted = false;
            }
            else if (intent.Action.Equals(Constants.ACTION_RESTART_TIMER))
            {
                Log.Info(TAG, "OnStartCommand: Restarting the timer.");
            }

            // This tells Android not to restart the service if it is killed to reclaim resources.
            return StartCommandResult.Sticky;
        }
        private static string cleanMessage(byte[] bytes)
        {
            string message = Encoding.UTF8.GetString(bytes);
            return message;
        }
        private void StartServer()
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(GetMyIpAddress(), 11_000);
                listener = new TcpListener(ipEndPoint);
                listener.Start();
                while (true)
                {
                    if(isStarted && MainActivity.Instance.stopServiceButton != null && !MainActivity.Instance.stopServiceButton.Enabled)
                    {
                        MainActivity.Instance.stopServiceButton.Enabled = true;
                        MainActivity.Instance.startServiceButton.Enabled = false;
                    }
                    // Receive message.
                    const int bytesize = 1024 * 1024;

                    string message = null;
                    byte[] buffer = new byte[bytesize];

                    var sender = listener.AcceptTcpClient();
                    sender.GetStream().Read(buffer, 0, bytesize);
                    message = cleanMessage(buffer);
                    var eom = "<|EOM|>";
                    if (message.IndexOf(eom) > -1 /* is end of message */)
                    {
                        string clearResponse = message.Replace("<GSI>","").Replace("<CNT>", "").Replace("<ACP>", "").Replace("<|EOM|>", "").Replace("<MAP>", "").Replace("<LBY>", "").Replace("�","");
                        switch (message.Substring(0, "<XXX>".Length))
                        {
                            case "<ACP>":
                                ShowNotification(Resources.GetString(Resource.String.app_name), clearResponse, Constants.ACCEPTED_MATCH_NOTIFICATION_ID, Constants.ACCEPTED_MATCH_CHANNEL);
                                break;
                            case "<MAP>":
                                ShowNotification(Resources.GetString(Resource.String.app_name), clearResponse, Constants.LOADED_ON_MAP_NOTIFICATION_ID, Constants.LOADED_ON_MAP_CHANNEL_ID);
                                break;
                            case "<LBY>":
                                ShowNotification(Resources.GetString(Resource.String.app_name), clearResponse, Constants.IN_LOBBY_NOTIFICATION_ID, Constants.LOADED_TO_LOBBY_CHANNEL);
                                break;
                            case "<CNT>":
                                ShowNotification(Resources.GetString(Resource.String.app_name), clearResponse, Constants.CONNECTED_NOTIFICATION_ID, Constants.SERVICE_CHANNEL_ID);
                                break;
                            case "<GSI>":
                                if (MainActivity.Active)
                                    ParseGameState(clearResponse);
                                break;
                            case "<CLS>":
                                if (MainActivity.Active)
                                {
                                    MainActivity.Instance.RunOnUiThread(() =>
                                    {
                                        MainActivity.Instance.state.Text = "";
                                        MainActivity.Instance.details.Text = "";
                                    });
                                }
                                break;
                        }
                        //MainActivity.Instance.RunOnUiThread(() => {
                        //    MainActivity.Instance.outputText.Text = clearResponse;
                        //});
                    
                    }
                }
            }
            catch (SocketException ex)
            {
                ShowNotification("Socket exception", $"{ex.Message}", Constants.SOCKET_EXCEPTION_NOTIFICATION_ID, Constants.SERVICE_CHANNEL_ID);
                listener.Stop();
            }
            catch (ObjectDisposedException ex)
            {
                ShowNotification("Disposed exception", $"{ex.Message}", Constants.DISPOSED_EXCEPTION_NOTIFICATION_ID, Constants.SERVICE_CHANNEL_ID);
            }
            catch (Exception ex) { ShowNotification("Error acurred", $"{ex.GetType()},{ex.StackTrace} - {ex.Message}", Constants.ERROR_NOTIFICATION_ID, Constants.SERVICE_CHANNEL_ID); }
            var stopServiceIntent = new Intent(this, GetType());
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
            currentContext.StartService(stopServiceIntent);
        }

        private void ParseGameState(string clearResponse)
        {
            string[] splt = clearResponse.Split('}');
            bool inGame = splt[splt.Length-1][..4] == "True";
            GameState gs = new GameState(clearResponse);  
            if (!inGame)
            {
                MainActivity.Instance.RunOnUiThread(() => {
                    MainActivity.Instance.state.Text = $"FriendCode: {CSGOFriendCode.Encode(gs.MySteamID)}";
                    MainActivity.Instance.details.Text = $"Chilling in lobby";
                });
            }
            else
            {
                MainActivity.Instance.RunOnUiThread(() => {
                    MainActivity.Instance.state.Text = $"{gs.Match.Mode} - {gs.Match.Map}";
                    string phase = gs.Match.Phase == Phase.Warmup ? "Warmup" : gs.Round.Phase.ToString();
                    MainActivity.Instance.details.Text = gs.Player.Team == Team.T ?
                        $"{gs.Match.TScore} [T] ({phase}) {gs.Match.CTScore} [CT]" :
                        $"{gs.Match.CTScore} [CT] ({phase}) {gs.Match.TScore} [T]";
                });
            }
        }
        private long GetMyIpAddress()
        {
            WifiManager wifiManager = (WifiManager)Application.Context.GetSystemService(Service.WifiService);
            return wifiManager.ConnectionInfo.IpAddress;
        }

        void ShowNotification(string title,string description,int id,string channel_id)
        {
            var builder = new NotificationCompat.Builder(this, channel_id)
                          .SetAutoCancel(true) // Dismiss the notification from the notification area when the user clicks on it
                          .SetContentTitle(title) // Set the title
                          .SetNumber(id) // Display the count in the Content Info
                          .SetSmallIcon(Resource.Mipmap.ic_launcher) // This is the icon to display
                          .SetContentText(description); // the message to display.
            builder.SetPriority((int)NotificationPriority.High);
            Notification notification = builder.Build();
            // Turn on sound if the sound switch is on:                 
            notification.Defaults |= NotificationDefaults.Sound;
            notification.Visibility = NotificationVisibility.Public;
            // Turn on vibrate if the sound switch is on:                 
            notification.Defaults |= NotificationDefaults.Vibrate;
            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(id, notification);
        }

        public override IBinder OnBind(Intent intent)
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
            StopServer();

            // Remove the notification from the status bar.
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Cancel(Constants.SERVICE_RUNNING_NOTIFICATION_ID);
            isStarted = false;
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean(Constants.SERVICE_STARTED_KEY, isStarted);
            // editor.Commit();    // applies changes synchronously on older APIs
            editor.Apply();        // applies changes asynchronously on newer APIs
            base.OnDestroy();
        }

        private void StopServer()
        {
            try
            {
                if (listener != null)
                    listener.Stop();
            }
            catch { }
        }

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

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
            name = "Loaded on map";
            description = "No description";
            channel = new NotificationChannel(Constants.LOADED_ON_MAP_CHANNEL_ID, name, NotificationImportance.High)
            {
                Description = description
            };

            notificationManager.CreateNotificationChannel(channel);
            name = "Accepted match";
            description = "No description";
            channel = new NotificationChannel(Constants.ACCEPTED_MATCH_CHANNEL, name, NotificationImportance.High)
            {
                Description = description
            };

            notificationManager.CreateNotificationChannel(channel);
            name = "Back in lobby";
            description = "No description";
            channel = new NotificationChannel(Constants.LOADED_TO_LOBBY_CHANNEL, name, NotificationImportance.High)
            {
                Description = description
            };

            notificationManager.CreateNotificationChannel(channel);
        }
        void RegisterForegroundService()
        {
            var notification = new NotificationCompat.Builder(this, Constants.SERVICE_CHANNEL_ID)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText($"Server running on ip {myIpAddress}")
                .SetSmallIcon(Resource.Mipmap.ic_launcher)
                .SetContentIntent(BuildIntentToShowMainActivity())
                .SetOngoing(true)
                .AddAction(BuildStopServiceAction())
                .Build();
            // Turn on sound if the sound switch is on:                 
            notification.Defaults = ~NotificationDefaults.Sound;

            // Turn on vibrate if the sound switch is on:                 
            notification.Defaults = ~NotificationDefaults.Vibrate;

            // Enlist this instance of the service as a foreground service
            StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);
        }

        /// <summary>
        /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
        /// user taps on the notification; it will take them to the main activity of the app.
        /// </summary>
        /// <returns>The content intent.</returns>
        PendingIntent BuildIntentToShowMainActivity()
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