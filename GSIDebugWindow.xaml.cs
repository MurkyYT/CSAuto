using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSAuto
{
    /// <summary>
    /// Interaction logic for GSIDebugWindow.xaml
    /// </summary>
    public partial class GSIDebugWindow : Window
    {
        MainWindow main;
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
            Utils.Log.debugWind = null;
        }
    }
}
