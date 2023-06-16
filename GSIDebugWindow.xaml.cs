using System;
using System.Windows;
using CSAuto.Utils;
namespace CSAuto
{
    /// <summary>
    /// Interaction logic for GSIDebugWindow.xaml
    /// </summary>
    public partial class GSIDebugWindow : Window
    {
        readonly MainWindow main;
        public GSIDebugWindow(MainWindow main)
        {
            InitializeComponent();
            this.main = main;
            Steam.GetLaunchOptions(730, out string launchOpt);
            steamInfo.Text = $"Steam Path: \"{Steam.GetSteamPath()}\"\n" +
                $"SteamID3: {Steam.GetCurrentSteamID3()}\n" +
                $"CS:GO Path: \"{Steam.GetGameDir("Counter-Strike Global Offensive")}\"\n" +
                $"CS:GO LaunchOptions: \"{launchOpt}\"";
        }
        public void UpdateText(string data)
        {
            this.Dispatcher.Invoke(() =>
            {
                outputBox.Text = data;
                lastRecieveTime.Text = $"Last recieved data from GSI: {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}";
            });
           
        }
        public void UpdateDebug(string data)
        {
            this.Dispatcher.Invoke(() =>
            {
                debugBox.Text += data+'\n';
                debugBox.ScrollToEnd();
            });

        }

        private void debugWind_Closed(object sender, EventArgs e)
        {
            main.debugWind = null;
            Utils.Log.debugWind = null;
        }
    }
}
