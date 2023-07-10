using Android.App;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using CSAuto;
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
        TextView outputText;
        TextView ipAdressText;
        IPAddress myIpAddress;
        GameState gameState = new GameState(null);
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            ipAdressText = FindViewById<TextView>(Resource.Id.IpAdressText);
            outputText = FindViewById<TextView>(Resource.Id.OutputText);
            WifiManager wifiManager = (WifiManager)Application.Context.GetSystemService(Service.WifiService);
#pragma warning disable CS0618 // Type or member is obsolete
            int ip = wifiManager.ConnectionInfo.IpAddress;
#pragma warning restore CS0618 // Type or member is obsolete

            myIpAddress = new IPAddress(ip);
            ipAdressText.Text = $"Your ip address : {myIpAddress}";
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            StartServerAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task StartServerAsync()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(myIpAddress, 11_000);
            using Socket listener = new Socket(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(ipEndPoint);
            listener.Listen(100);

            var handler = await listener.AcceptAsync();
            try
            {
                while (true)
                {
                    // Receive message.
                    var buffer = new byte[1_024*1_024];
                    var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                    var response = Encoding.UTF8.GetString(buffer, 0, received);
                    var eom = "<|EOM|>";
                    if (response.IndexOf(eom) > -1 /* is end of message */)
                    {
                        var ackMessage = "<|ACK|>";
                        var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                        await handler.SendAsync(echoBytes, 0);
                        if (response.Substring(0, "<GSI>".Length) == "<GSI>")
                        {
                            gameState = new GameState(outputText.Text);
                            outputText.Text = gameState.MySteamID;
                        }
                        if (response.Substring(0, "<ALV>".Length) == "<ALV>")
                            continue;
                        outputText.Text = response.Replace(eom, "").Replace("<CLS>", "").Replace("<GSI>", "");
                    }
                    // Sample output:
                    //    Socket server received message: "Hi friends 👋!"
                    //    Socket server sent acknowledgment: "<|ACK|>"
                }
            }
            catch {}
            listener.Close();
            await StartServerAsync();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}