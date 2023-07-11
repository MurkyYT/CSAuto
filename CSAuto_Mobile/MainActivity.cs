using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using AndroidX.AppCompat.App;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtocolType = System.Net.Sockets.ProtocolType;

namespace CSAuto_Mobile
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        static readonly string TAG = typeof(MainActivity).FullName;
        Button stopServiceButton;
        Button startServiceButton;
        Intent startServiceIntent;
        Intent stopServiceIntent;
        bool isStarted = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
           
            if (savedInstanceState != null)
            {
                isStarted = savedInstanceState.GetBoolean(Constants.SERVICE_STARTED_KEY, false);
            }
            WifiManager wifiManager = (WifiManager)Application.Context.GetSystemService(Service.WifiService);
#pragma warning disable CS0618 // Type or member is obsolete
            int ip = wifiManager.ConnectionInfo.IpAddress;
#pragma warning restore CS0618 // Type or member is obsolete

            FindViewById<TextView>(Resource.Id.IpAdressText).Text = $"Your ip address : {new IPAddress(ip)}";

            startServiceIntent = new Intent(this, typeof(ServerService));
            startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);

            stopServiceIntent = new Intent(this, typeof(ServerService));
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);


            stopServiceButton = FindViewById<Button>(Resource.Id.stop_timestamp_service_button);
            startServiceButton = FindViewById<Button>(Resource.Id.start_timestamp_service_button);
            if (isStarted)
            {
                stopServiceButton.Click += StopServiceButton_Click;
                stopServiceButton.Enabled = true;
                startServiceButton.Enabled = false;
            }
            else
            {
                startServiceButton.Click += StartServiceButton_Click;
                startServiceButton.Enabled = true;
                stopServiceButton.Enabled = false;
            }
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
            //Log.Info(TAG, "Activity is being destroyed; stop the service.");

            //StopService(startServiceIntent);
            base.OnDestroy();
        }
        void StopServiceButton_Click(object sender, System.EventArgs e)
        {
            stopServiceButton.Click -= StopServiceButton_Click;
            stopServiceButton.Enabled = false;

            Log.Info(TAG, "User requested that the service be stopped.");
            StopService(stopServiceIntent);
            isStarted = false;

            startServiceButton.Click += StartServiceButton_Click;
            startServiceButton.Enabled = true;
        }

        void StartServiceButton_Click(object sender, System.EventArgs e)
        {
            startServiceButton.Enabled = false;
            startServiceButton.Click -= StartServiceButton_Click;

            StartService(startServiceIntent);
            Log.Info(TAG, "User requested that the service be started.");

            isStarted = true;
            stopServiceButton.Click += StopServiceButton_Click;

            stopServiceButton.Enabled = true;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}