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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (CSAuto.Properties.Settings.Default.darkTheme)
                // Set the application theme to Dark.Green
                ThemeManager.Current.ChangeTheme(this, "Dark.Green");
            else
                // Set the application theme to Light.Green
                ThemeManager.Current.ChangeTheme(this, "Light.Green");


        }
    }
}
