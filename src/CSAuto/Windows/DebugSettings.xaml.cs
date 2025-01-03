using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSAuto
{
    /// <summary>
    /// Interaction logic for DebugSettings.xaml
    /// </summary>
    public partial class DebugSettings : MetroWindow
    {
        private MainApp main;

        public DebugSettings(MainApp main)
        {
            this.main = main;
            
            InitializeComponent();

            if (main.current.IsWindows11)
            {
                IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
                var attribute = NativeMethods.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
                var preference = NativeMethods.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                NativeMethods.DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
            }

            Color regularColor = main.BUTTON_COLORS[0];
            RegularColorRed.Text = regularColor.R.ToString();
            RegularColorGreen.Text = regularColor.G.ToString();
            RegularColorBlue.Text = regularColor.B.ToString();
            OriginalRegular.Text = $"Original Color: {regularColor}";
            Color activeColor = main.BUTTON_COLORS[1];
            ActiveColorRed.Text = activeColor.R.ToString();
            ActiveColorGreen.Text = activeColor.G.ToString();
            ActiveColorBlue.Text = activeColor.B.ToString();
            OriginalActive.Text = $"Original Color: {activeColor}";
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            main.BUTTON_COLORS[0] = 
                Color.FromArgb(int.Parse(RegularColorRed.Text),
                                int.Parse(RegularColorGreen.Text),
                                int.Parse(RegularColorBlue.Text));
            main.BUTTON_COLORS[1] =
               Color.FromArgb(int.Parse(ActiveColorRed.Text),
                               int.Parse(ActiveColorGreen.Text),
                               int.Parse(ActiveColorBlue.Text));
            main.UpdateColors();
            Properties.DebugSettings.Default.Save();
        }
    }
}
