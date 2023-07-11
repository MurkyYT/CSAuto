using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.LocalBroadcastManager.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Encoding = System.Text.Encoding;
using Android.Net.Wifi;
using System.Runtime.Remoting.Contexts;
using Context = Android.Content.Context;
using ProtocolType = System.Net.Sockets.ProtocolType;
using Activity = Android.App.Activity;
using Xamarin.Essentials;
using AndroidX.Core.App;
using Android.Preferences;

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
        TextView outputText;
        TextView ipAdressText;
        Button stopServiceButton;
        Button startServiceButton;
        Socket listener;
        bool stopServer = false;
        public override void OnCreate()
        {
            base.OnCreate();
            Log.Info(TAG, "OnCreate: the service is initializing.");
            if (currentContext != null)
            {
                ipAdressText = ((Activity)currentContext).FindViewById<TextView>(Resource.Id.IpAdressText);
                outputText = ((Activity)currentContext).FindViewById<TextView>(Resource.Id.OutputText);
                stopServiceButton = ((Activity)currentContext).FindViewById<Button>(Resource.Id.stop_timestamp_service_button);
                startServiceButton = ((Activity)currentContext).FindViewById<Button>(Resource.Id.start_timestamp_service_button);
            }
            WifiManager wifiManager = (WifiManager)Application.Context.GetSystemService(Service.WifiService);
#pragma warning disable CS0618 // Type or member is obsolete
            int ip = wifiManager.ConnectionInfo.IpAddress;
#pragma warning restore CS0618 // Type or member is obsolete
            if (ipAdressText != null)
            {
                myIpAddress = new IPAddress(ip);
                ipAdressText.Text = $"Your ip address : {myIpAddress}";
            }
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
                    StartServerAsync();
                    isStarted = true;
                }
            }
            else if (intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
            {
                Log.Info(TAG, "OnStartCommand: The service is stopping.");
                StopServer();
                StopForeground(true);
                StopSelf();
                isStarted = false;
            }
            else if (intent.Action.Equals(Constants.ACTION_RESTART_TIMER))
            {
                Log.Info(TAG, "OnStartCommand: Restarting the timer.");
            }

            // This tells Android not to restart the service if it is killed to reclaim resources.
            return StartCommandResult.Sticky;
        }
        private async Task StartServerAsync()
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(GetMyIpAddress(), 11_000);
                listener = new Socket(
                    ipEndPoint.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);
                listener.Bind(ipEndPoint);
                listener.Listen(100);
                var handler = await listener.AcceptAsync();
                stopServer = false;
                while (true)
                {
                    if (stopServer)
                    {
                        handler.Dispose();
                        break;
                    }
                    if(isStarted && stopServiceButton != null && !stopServiceButton.Enabled)
                    {
                        stopServiceButton.Enabled = true;
                        startServiceButton.Enabled = false;
                    }
                    // Receive message.
                    var buffer = new byte[1_024 * 1_024];
                    var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                    var response = Encoding.UTF8.GetString(buffer, 0, received);
                    var eom = "<|EOM|>";
                    if (response.IndexOf(eom) > -1 /* is end of message */)
                    {
                        var ackMessage = "<|ACK|>";
                        var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                        await handler.SendAsync(echoBytes, 0);
                        string clearResponse = response.Replace("<CNT>", "").Replace("<ACP>", "").Replace("<|EOM|>", "").Replace("<MAP>", "").Replace("<LBY>", "");
                        switch (response.Substring(0, "<XXX>".Length))
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
                        }
                        //if (response.Substring(0, "<GSI>".Length) == "<GSI>")
                        //{
                        //    gameState = new GameState(outputText.Text);
                        //    outputText.Text = gameState.MySteamID;
                        //}
                        outputText.Text = clearResponse;
                        //ShowNotification(Resources.GetString(Resource.String.app_name), response, id++);
                    }
                    // Sample output:
                    //    Socket server received message: "Hi friends 👋!"
                    //    Socket server sent acknowledgment: "<|ACK|>"
                }
                if (stopServer)
                {
                    stopServer = false;
                    return;
                }
                listener.EndConnect(null);
            }
            catch (SocketException) { listener.Dispose(); }
            catch (ObjectDisposedException) { }
            catch (Exception ex){ ShowNotification("Error acurred", $"{ex.GetType()},{ex.StackTrace} - {ex.Message}", Constants.ERROR_NOTIFICATION_ID, Constants.SERVICE_CHANNEL_ID); }
            await StartServerAsync();
        }

        private void LoadMainWindowItems()
        {
            if(currentContext == null)
                currentContext = Platform.CurrentActivity;
            if (currentContext != null)
            {
                ipAdressText = ((Activity)currentContext).FindViewById<TextView>(Resource.Id.IpAdressText);
                outputText = ((Activity)currentContext).FindViewById<TextView>(Resource.Id.OutputText);
                stopServiceButton = ((Activity)currentContext).FindViewById<Button>(Resource.Id.stop_timestamp_service_button);
                startServiceButton = ((Activity)currentContext).FindViewById<Button>(Resource.Id.start_timestamp_service_button);
            }
        }

        private long GetMyIpAddress()
        {
            WifiManager wifiManager = (WifiManager)Application.Context.GetSystemService(Service.WifiService);
#pragma warning disable CS0618 // Type or member is obsolete
            return wifiManager.ConnectionInfo.IpAddress;
#pragma warning restore CS0618 // Type or member is obsolete
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
                stopServer = true;
                if (listener != null)
                    listener.Dispose();
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
            var notification = new Notification.Builder(this, Constants.SERVICE_CHANNEL_ID)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText("CSAuto service running")
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
        Notification.Action BuildStopServiceAction()
        {
            var stopServiceIntent = new Intent(this, GetType());
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
            var stopServicePendingIntent = PendingIntent.GetService(this, 0, stopServiceIntent, PendingIntentFlags.Mutable);

            var builder = new Notification.Action.Builder(Android.Resource.Drawable.IcMediaPause,
                                                          GetText(Resource.String.stop_service),
                                                          stopServicePendingIntent);
            return builder.Build();

        }
    }
}