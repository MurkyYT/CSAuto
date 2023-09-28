using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using AndroidX.AppCompat.App;
using System.Net;
#pragma warning disable CS0618 // Type or member is obsolete
namespace CSAuto_Mobile
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        static readonly string TAG = typeof(MainActivity).FullName;
        public Button stopServiceButton;
        public Button startServiceButton;
        public TextView outputText;
        TextView ipAdressText;
        public TextView details;
        public TextView state;
        public TextView bombState;
        public static MainActivity Instance;
        public static bool Active = false;
        Intent startServiceIntent;
        Intent stopServiceIntent;
        bool isStarted = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            //if (savedInstanceState != null)
            //{
            //    isStarted = savedInstanceState.GetBoolean(Constants.SERVICE_STARTED_KEY, false);
            //}
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            isStarted = prefs.GetBoolean(Constants.SERVICE_STARTED_KEY,false);
            WifiManager wifiManager = (WifiManager)Application.Context.GetSystemService(Service.WifiService);

            int ip = wifiManager.ConnectionInfo.IpAddress;
            ipAdressText = FindViewById<TextView>(Resource.Id.IpAdressText);
            outputText = FindViewById<TextView>(Resource.Id.OutputText);
            details = FindViewById<TextView>(Resource.Id.details);
            state = FindViewById<TextView>(Resource.Id.state);
            bombState = FindViewById<TextView>(Resource.Id.bomb_state);
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
        protected override void OnNewIntent(Intent intent)
        {
            if (intent == null)
            {
                return;
            }

            var bundle = intent.Extras;
            if (bundle != null)
            {
                if (bundle.ContainsKey(Constants.SERVICE_STARTED_KEY))
                {
                    isStarted = true;
                }
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean(Constants.SERVICE_STARTED_KEY, isStarted);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnDestroy()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean(Constants.SERVICE_STARTED_KEY, isStarted);
            // editor.Commit();    // applies changes synchronously on older APIs
            editor.Apply();        // applies changes asynchronously on newer APIs
            Active = false;
            base.OnDestroy();
        }
        void StopServiceButton_Click(object sender, System.EventArgs e)
        {
            stopServiceButton.Enabled = false;
            Log.Info(TAG, "User requested that the service be stopped.");
            StopService(stopServiceIntent);
            isStarted = false;
            startServiceButton.Enabled = true;
        }

        void StartServiceButton_Click(object sender, System.EventArgs e)
        {
            startServiceButton.Enabled = false;
            StartService(startServiceIntent);
            Log.Info(TAG, "User requested that the service be started.");
            isStarted = true;
            stopServiceButton.Enabled = true;
            Intent intent = new Intent();
            string packageName = PackageName;
            PowerManager pm = (PowerManager)GetSystemService(PowerService);
            if (pm.IsIgnoringBatteryOptimizations(packageName))
                Log.Info(TAG, "Already ignoring battery optimizations");
            else
            {
                intent.SetAction(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
                intent.SetData(Android.Net.Uri.Parse("package:" + packageName));
                StartActivity(intent);
            }
            
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}