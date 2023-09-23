using ControlzEx.Theming;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CSAuto
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public bool StartWidnow;
        public bool AlwaysMaximized;
        public bool Restarted;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (CSAuto.Properties.Settings.Default.darkTheme)
                // Set the application theme to Dark + selected color
                ThemeManager.Current.ChangeTheme(this, $"Dark.{CSAuto.Properties.Settings.Default.currentColor}");
            else
                // Set the application theme to Light + selected color
                ThemeManager.Current.ChangeTheme(this, $"Light.{CSAuto.Properties.Settings.Default.currentColor}");
            foreach (string arg in e.Args)
            {
                if (arg == "--maximized")
                    AlwaysMaximized = true;
                if (arg == "--show")
                    StartWidnow = true;
                if (arg == "--restart")
                    Restarted = true;
            }
            new MainApp().Show();
        }
    }
}
