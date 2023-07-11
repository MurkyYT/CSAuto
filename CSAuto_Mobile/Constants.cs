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
        public const string SERVICE_STARTED_KEY = "has_service_been_started";
        public const string BROADCAST_MESSAGE_KEY = "broadcast_message";
        public const string NOTIFICATION_BROADCAST_ACTION = "ServicesDemo3.Notification.Action";
        public const string SERVICE_CHANNEL_ID = "SERVICECHANNELID";
        public const string LOADED_ON_MAP_CHANNEL_ID = "LOADEDCHANNELID";
        public const string ACCEPTED_MATCH_CHANNEL = "ACCEPTEDCHANNELID";
        public const string LOADED_TO_LOBBY_CHANNEL = "LOBBYDCHANNELID";

        public const string ACTION_START_SERVICE = "ServicesDemo3.action.START_SERVICE";
        public const string ACTION_STOP_SERVICE = "ServicesDemo3.action.STOP_SERVICE";
        public const string ACTION_RESTART_TIMER = "ServicesDemo3.action.RESTART_TIMER";
        public const string ACTION_MAIN_ACTIVITY = "ServicesDemo3.action.MAIN_ACTIVITY";
    }
}