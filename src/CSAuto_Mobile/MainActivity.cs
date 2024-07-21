using Android.Content;
using Android.Net.Wifi;
using Android.Preferences;
using System.Net;
using Android.OS;
using Android.Util;
using Java.Util.Prefs;
using Android.Net;
using Android.Graphics.Drawables;
using Android.Content.Res;
using System.Drawing;

namespace CSAuto_Mobile
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        static readonly string? TAG = typeof(MainActivity).FullName;
        public required Button? stopServiceButton;
        public required Button? startServiceButton;
        public required Button? logButton;
        public required TextView? outputText;
        public required EditText? ipAdressText;
        public required EditText? portText;
        public required TextView? ctScoreText;
        public required TextView? tScoreText;
        public required TextView? roundStateText;
        public required TextView? mapText;
        public required TextView? playerText;
        //public required TextView? details;
        //public required TextView? state;
        public required TextView? bombState;
        public required GridLayout? logGrid;
        public required GridLayout? inGameGrid;
        public required GridLayout? lobbyGrid;
        public required ScrollView? scrollView;
        public static MainActivity? Instance;
        public static bool Active = false;
        public required Intent startServiceIntent;
        public required Intent stopServiceIntent;
        public bool inGame = false;
        bool isStarted = false;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            isStarted = IsStarted();
            WifiManager? wifiManager = (WifiManager?)Application.Context.GetSystemService(WifiService);

            byte[]? ip = GetMyIpAddress();
            ipAdressText = FindViewById<EditText>(Resource.Id.IpAdressText);
            portText = FindViewById<EditText>(Resource.Id.PortText);
            outputText = FindViewById<TextView>(Resource.Id.OutputText);
            logGrid = FindViewById<GridLayout>(Resource.Id.LogGrid);
            inGameGrid = FindViewById<GridLayout>(Resource.Id.inGameGrid);
            lobbyGrid = FindViewById<GridLayout>(Resource.Id.inLobbyGrid);
            logButton = FindViewById<Button>(Resource.Id.logButton);
            ctScoreText = FindViewById<TextView>(Resource.Id.ctScoreText);
            tScoreText = FindViewById<TextView>(Resource.Id.tScoreText);
            roundStateText = FindViewById<TextView>(Resource.Id.roundStateText);
            mapText = FindViewById<TextView>(Resource.Id.mapText);
            playerText = FindViewById<TextView>(Resource.Id.playerStatsText);
            //details = FindViewById<TextView>(Resource.Id.details);
            //state = FindViewById<TextView>(Resource.Id.state);
            bombState = FindViewById<TextView>(Resource.Id.bombStateText);
            scrollView = FindViewById<ScrollView>(Resource.Id.outputScrollView);
            FindViewById(Resource.Id.ctScoreLayout).SetBackgroundDrawable(GetRoundedDrawable(140, 157, 165));
            FindViewById(Resource.Id.tScoreLayout).SetBackgroundDrawable(GetRoundedDrawable(203, 186, 125));
            portText.Text = AppPreferences.ServerPort;
            ipAdressText.Text = AppPreferences.ServerIp;
            Instance = this;
            startServiceIntent = new Intent(this, typeof(ServerService));
            startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);

            stopServiceIntent = new Intent(this, typeof(ServerService));
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);

            stopServiceButton = FindViewById<Button>(Resource.Id.stop_timestamp_service_button);
            startServiceButton = FindViewById<Button>(Resource.Id.start_timestamp_service_button);

            startServiceButton.Click += StartServiceButton_Click;

            stopServiceButton.Click += StopServiceButton_Click;
            logButton.Click += LogButton_Click;
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
        private Drawable GetRoundedDrawable(int r,int g,int b)
        {
            GradientDrawable gradientDrawable = new GradientDrawable();
            gradientDrawable.SetCornerRadii(new float[] { 20, 20, 20, 20, 20, 20, 20, 20 });
            gradientDrawable.SetColor(Color.FromArgb(r,g,b).ToArgb());
            return gradientDrawable;
        }
        public void UpdateLayout()
        {
            if (logGrid.Visibility == Android.Views.ViewStates.Invisible)
            {
                if (inGame)
                {
                    inGameGrid.Visibility = Android.Views.ViewStates.Visible;
                    lobbyGrid.Visibility = Android.Views.ViewStates.Invisible;
                }
                else
                {
                    lobbyGrid.Visibility = Android.Views.ViewStates.Visible;
                    inGameGrid.Visibility = Android.Views.ViewStates.Invisible;
                }
            }
        }
        private void LogButton_Click(object? sender, EventArgs e)
        {
            if(logGrid.Visibility == Android.Views.ViewStates.Invisible)
            {
                logGrid.Visibility = Android.Views.ViewStates.Visible;
                inGameGrid.Visibility = Android.Views.ViewStates.Invisible;
                lobbyGrid.Visibility = Android.Views.ViewStates.Invisible;
            }
            else
            {
                logGrid.Visibility = Android.Views.ViewStates.Invisible;
                if(inGame)
                    inGameGrid.Visibility = Android.Views.ViewStates.Visible;
                else
                    lobbyGrid.Visibility = Android.Views.ViewStates.Visible;
            }
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
            Globals.IP = ipAdressText.Text.Replace(" ","");
            Globals.PORT = portText.Text;
            Intent intent = new Intent();
            string? packageName = PackageName;
            PowerManager? pm = (PowerManager?)GetSystemService(PowerService);
            AppPreferences.ServerIp = ipAdressText.Text;
            AppPreferences.ServerPort = portText.Text;
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