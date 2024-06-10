using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSAuto_Mobile
{
    public static class Constants
    {
        public const int DELAY_BETWEEN_LOG_MESSAGES = 5000; // milliseconds
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
        public const int ERROR_NOTIFICATION_ID = 0;
        public const int CONNECTED_NOTIFICATION_ID = 1;
        public const int IN_LOBBY_NOTIFICATION_ID = 2;
        public const int LOADED_ON_MAP_NOTIFICATION_ID = 3;
        public const int ACCEPTED_MATCH_NOTIFICATION_ID = 4;
        public const int SOCKET_EXCEPTION_NOTIFICATION_ID = 5;
        public const int DISPOSED_EXCEPTION_NOTIFICATION_ID = 6;
        public const int BOMB_NOTIFICATION_ID = 7;
        public const int CRASHED_NOTIFICATION_ID = 8;
        public const string SERVICE_STARTED_KEY = "has_service_been_started";
        public const string BROADCAST_MESSAGE_KEY = "broadcast_message";
        public const string NOTIFICATION_BROADCAST_ACTION = "CSAuto.Notification.Action";
        public const string SERVICE_CHANNEL_ID = "SERVICECHANNELID";
        public const string LOADED_ON_MAP_CHANNEL_ID = "LOADEDCHANNELID";
        public const string ACCEPTED_MATCH_CHANNEL = "ACCEPTEDCHANNELID";
        public const string LOADED_TO_LOBBY_CHANNEL = "LOBBYDCHANNELID";
        public const string CRASHED_CHANNEL = "CRASHEDCHANNELID";
        public const string BOMB_CHANNEL = "BOMBCHANNELID";

        public const string ACTION_START_SERVICE = "CSAuto.action.START_SERVICE";
        public const string ACTION_STOP_SERVICE = "CSAuto.action.STOP_SERVICE";
        public const string ACTION_RESTART_TIMER = "CSAuto.action.RESTART_TIMER";
        public const string ACTION_MAIN_ACTIVITY = "CSAuto.action.MAIN_ACTIVITY";
    }
}