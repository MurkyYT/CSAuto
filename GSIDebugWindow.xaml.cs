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
            Log.debugWind = null;
        }
    }
}
