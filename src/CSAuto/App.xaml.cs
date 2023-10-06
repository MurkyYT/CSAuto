using ControlzEx.Theming;
using CSAuto.Properties;
using Murky.Utils;
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
        public RegistrySettings settings = new RegistrySettings();
        protected override void OnStartup(StartupEventArgs e)
        {
            if (settings["FirstRun"] == null || settings["FirstRun"])
            {
                Log.WriteLine("First run of new settings, moving old ones to registry");
                MoveSettings();
                List<DiscordRPCButton> discordRPCButtonsOld = DiscordRPCButtonSerializer.DeserializeOld();
                DiscordRPCButtonSerializer.Serialize(discordRPCButtonsOld);
                settings.Set("FirstRun", false);
            }
            else
            {
                Log.WriteLine("Loading registry settings to properties");
                LoadSettings();
            }
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
        private void LoadSettings()
        {
            foreach (SettingsProperty currentProperty in Settings.Default.Properties)
            {
                if (currentProperty.Name == "availableColors")
                    continue;
                string res = FirstCharToUpper(currentProperty.Name);
                Settings.Default[currentProperty.Name] = settings[res].GetValue();
                Settings.Default.Save();
            }
        }

        public void MoveSettings()
        {
            foreach (SettingsProperty currentProperty in Settings.Default.Properties)
            {
                if (currentProperty.Name == "availableColors")
                    continue;
                string res = FirstCharToUpper(currentProperty.Name);
                settings.Set(res, Settings.Default[currentProperty.Name]);
            }
        }
        private string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + String.Join("", input.Skip(1));
        }
    }
}
