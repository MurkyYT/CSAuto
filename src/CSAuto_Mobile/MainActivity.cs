using Android.Content;
using Android.Net.Wifi;
using Android.Preferences;
using System.Net;
using Android.OS;
using Android.Util;
using Java.Util.Prefs;
using Android.Net;

namespace CSAuto_Mobile
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {

        static readonly string? TAG = typeof(MainActivity).FullName;
        public required Button? stopServiceButton;
        public required Button? startServiceButton;
        public required TextView? outputText;
        public required TextView? ipAdressText;
        public required TextView? details;
        public required TextView? state;
        public required TextView? bombState;
        public static MainActivity? Instance;
        public static bool Active = false;
        public required Intent startServiceIntent;
        public required Intent stopServiceIntent;
        bool isStarted = false;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            isStarted = IsStarted();
            WifiManager? wifiManager = (WifiManager?)Application.Context.GetSystemService(WifiService);


            byte[]? ip = GetMyIpAddress();
            ipAdressText = FindViewById<TextView>(Resource.Id.IpAdressText);
            outputText = FindViewById<TextView>(Resource.Id.OutputText);
            details = FindViewById<TextView>(Resource.Id.details);
            state = FindViewById<TextView>(Resource.Id.state);
            bombState = FindViewById<TextView>(Resource.Id.bomb_state);
            if(ip != null)
                ipAdressText.Text = $"Your IP Address: {new IPAddress(ip)}";
            Instance = this;
            startServiceIntent = new Intent(this, typeof(ServerService));
            startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);

            stopServiceIntent = new Intent(this, typeof(ServerService));
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);


            stopServiceButton = FindViewById<Button>(Resource.Id.stop_timestamp_service_button);
            startServiceButton = FindViewById<Button>(Resource.Id.start_timestamp_service_button);

            startServiceButton.Click += StartServiceButton_Click;

            stopServiceButton.Click += StopServiceButton_Click;
            if (isStarted)
            {
                stopServiceButton.Enabled = true;
                startServiceButton.Enabled = false;
            }
            else
            {
                startServiceButton.Enabled = true;
                stopServiceButton.Enabled = false;
            }
            Active = true;
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
        private bool IsStarted()
        {
            Preferences? prefs = Preferences.UserRoot();
            return prefs.GetBoolean(Constants.SERVICE_STARTED_KEY,false);
        }
        void StopServiceButton_Click(object? sender, System.EventArgs e)
        {
            stopServiceButton.Enabled = false;
            Log.Info(TAG, "User requested that the service be stopped.");
            StopService(stopServiceIntent);
            isStarted = false;
            startServiceButton.Enabled = true;
        }

        void StartServiceButton_Click(object? sender, System.EventArgs e)
        {
            startServiceButton.Enabled = false;
            StartService(startServiceIntent);
            Log.Info(TAG, "User requested that the service be started.");
            isStarted = true;
            stopServiceButton.Enabled = true;
            Intent intent = new Intent();
            string? packageName = PackageName;
            PowerManager? pm = (PowerManager?)GetSystemService(PowerService);
            if (pm.IsIgnoringBatteryOptimizations(packageName))
                Log.Info(TAG, "Already ignoring battery optimizations");
            else
            {
                intent.SetAction(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
                intent.SetData(Android.Net.Uri.Parse("package:" + packageName));
                StartActivity(intent);
            }

        }
    }
}
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning restore CA1422 // Validate platform compatibility