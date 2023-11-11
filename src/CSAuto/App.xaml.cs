using ControlzEx.Theming;
using CSAuto.Properties;
using Murky.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

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
        public string Args = "";
        public RegistrySettings settings = new RegistrySettings();

        private bool crashed;
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            string languageName = null;
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
            if (Settings.Default.darkTheme)
                // Set the application theme to Dark + selected color
                ThemeManager.Current.ChangeTheme(this, $"Dark.{Settings.Default.currentColor}");
            else
                // Set the application theme to Light + selected color
                ThemeManager.Current.ChangeTheme(this, $"Light.{Settings.Default.currentColor}");
            foreach (string arg in e.Args)
            {
                if(arg != "--show" && arg != "--restart")
                    Args += arg + " ";
                if (arg == "--maximized")
                    AlwaysMaximized = true;
                if (arg == "--show")
                    StartWidnow = true;
                if (arg == "--restart")
                    Restarted = true;
                if (arg == "--language" && e.Args.ToList().IndexOf(arg) + 1 < e.Args.Length)
                    languageName = e.Args[e.Args.ToList().IndexOf(arg) + 1];
            }
            //Clear error log
            if(File.Exists("Error_Log.txt"))
                File.Delete("Error_Log.txt");
            LoadLanguage(languageName?.ToLower());
            new MainApp().Show();
        }

        private void LoadLanguage(string languageName)
        {
            string[] englishFile = File.ReadAllLines($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\resource\\language_english.txt");
            string[] file = null;
            try { file = File.ReadAllLines($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\resource\\{(languageName == null ? Settings.Default.currentLanguage : "language_"+languageName)}.txt"); }
            catch (FileNotFoundException) 
            { 
                MessageBox.Show($"Couldn't load {(languageName == null ? Settings.Default.currentLanguage : "language_" + languageName)}, the app will fallback to english", 
                    "Warning", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning); 
            }
            if(file!= null)
            {
                for (int i = 0; i < file.Length; i++)
                {
                    string[] values = GetValues(file[i]);
                    if (values != null && values[0] != null && values[1] != null)
                        Languages._Language.translation.Add(values[0], values[1]);
                }
            }
            for (int i = 0; i < englishFile.Length; i++)
            {
                string[] values = GetValues(englishFile[i]);
                if (values != null && values[0] != null && values[1] != null)
                    Languages._Language.englishTranslation.Add(values[0], values[1]);
            }
        }

        private string[] GetValues(string v)
        {
            string[] res = new string[2];
            int count = 0;
            string oneRes = "";
            foreach(char ch in v)
            {
                if (ch == '"') { count++; }
                if (count > 0 && 
                    count % 2 == 0 && 
                    res[count / 2 - 1] == null) 
                {
                    res[count / 2 - 1] = oneRes; oneRes = "";
                }
                if(count%2 == 1 && ch != '"') oneRes += ch;
            }
            return res;
        }

        private void LoadSettings()
        {
            foreach (SettingsProperty currentProperty in Settings.Default.Properties)
            {
                if (currentProperty.Name == "availableColors")
                    continue;
                string res = FirstCharToUpper(currentProperty.Name);
                if (settings[res] == null)
                    settings.Set(res, Settings.Default[currentProperty.Name]);
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
        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            CurrentDomain_UnhandledException(
                sender,
                new UnhandledExceptionEventArgs(e.Exception, e.Handled));
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (crashed)
                return;
            crashed = true;
            StackFrame frame = new StackFrame(1, false);
            Exception ex = ((Exception)e.ExceptionObject);
            Log.Error(
                $"{ex.Message}\n" +
                $"StackTrace:{ex.StackTrace}\n" +
                $"Source: {ex.Source}\n" +
                $"Inner Exception: {ex.InnerException}");
            MessageBox.Show(AppLanguage.Language["error_appcrashed"], AppLanguage.Language["title_error"] + $" ({frame.GetMethod().Name})", MessageBoxButton.OK, MessageBoxImage.Error);
            Process.Start("Error_Log.txt");
            Current.Shutdown();
        }

    }
}
