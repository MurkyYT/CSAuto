﻿using CSAuto.Properties;
using Microsoft.Win32;
using Murky.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace CSAuto
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public bool StartWindow;
        public bool AlwaysMaximized;
        public bool Restarted;
        public bool IsWindows11;
        public bool IsPortable;
        public bool LogArg;
        public string Args = "";
        public AutoBuyMenu buyMenu;
        public RegistrySettings settings = new RegistrySettings();

        private bool crashed;
        private string languageName = null;
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.AppendPrivatePath("bin");
            RealStartup(e); 
        }
        void RealStartup(StartupEventArgs e)
        {
            string[] files = Directory.GetFiles(Log.WorkPath, "*.dll");
            bool didCleanOld = files.Length > 0;
            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml", UriKind.RelativeOrAbsolute)
            });
            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml", UriKind.RelativeOrAbsolute)
            });
            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Themes/Light.Blue.xaml", UriKind.RelativeOrAbsolute)
            });
            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Resources/VectorImages.xaml", UriKind.RelativeOrAbsolute)
            });

            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            if (File.Exists(Log.WorkPath + "\\resource\\.portable"))
                IsPortable = true;
            ParseArgs(e);

            if (didCleanOld)
            {
                Process.Start(Log.WorkPath + "\\bin\\updater.exe", "--cleanup \"" + Args + " --restart\"");
                App.Current.Shutdown();
            }

            if (IsPortable)
            {
                bool oldSettingsExist = settings.Exists();
                if (oldSettingsExist)
                    File.WriteAllText(Log.WorkPath + "\\.tmp", settings.ToString(), Encoding.UTF8);
                settings = new RegistrySettings("Murky", "CSAuto-Portable");
                if (File.Exists(Log.WorkPath + "\\.conf"))
                    settings.Import(Log.WorkPath + "\\.conf");
                else if (oldSettingsExist)
                {
                    Log.WriteLine("|App.cs| Loading original settings to portable, first run of portable, but has old settings");
                    settings.Import(Log.WorkPath + "\\.tmp");
                }
                if (oldSettingsExist)
                    File.Delete(Log.WorkPath + "\\.tmp");
            }

            buyMenu = new AutoBuyMenu();
            ImportSettings();
            ImportAutoBuy();

            if (Settings.Default.currentLanguage.Contains("language"))
            {
                switch (Settings.Default.currentLanguage)
                {
                    case "language_english":
                        Settings.Default.currentLanguage = "en-US";
                        break;
                    case "language_russian":
                        Settings.Default.currentLanguage = "ru-RU";
                        break;
                }
            }
            else
            {
                switch (Settings.Default.currentLanguage)
                {
                    case "en-EN":
                        Settings.Default.currentLanguage = "en-US";
                        break;
                }
            }
            try
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(
                    languageName ?? Settings.Default.currentLanguage);
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(
                   languageName ?? Settings.Default.currentLanguage);
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo(
                   languageName ?? Settings.Default.currentLanguage);
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(
                  languageName ?? Settings.Default.currentLanguage);
                if (languageName != null && !AppLanguage.Available.Contains(languageName))
                    throw new Exception();
            }
            catch 
            { 
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US"); 
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");  
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                Settings.Default.currentLanguage = "en-US";
                MessageBox.Show(Languages.Strings.warning_language, Languages.Strings.title_warning, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (AppLanguage.IsRTL[languageName ?? Settings.Default.currentLanguage])
            {
                Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/Resources/RTLResource.xaml", UriKind.RelativeOrAbsolute)
                });
            }

            Log.WriteLine($"|App.cs| Selected culture is: {CultureInfo.CurrentUICulture}");

            base.OnStartup(e);

            //Clear error log
            if (File.Exists("Error_Log.txt"))
                File.Delete("Error_Log.txt");

            WinVersion.GetVersion(out VersionInfo ver);
            if (ver.BuildNum >= (uint)BuildNumber.Windows_11_21H2)
                IsWindows11 = true;

            if (IsWindows11)
                Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/Resources/RoundedResources.xaml", UriKind.RelativeOrAbsolute)
                });
            if(!Settings.Default.oldScreenCaptureWay && Native.DisplayInfo.HasHDREnabledDisplay() && 
                (!settings.KeyExists("OptimizeForHDR") || !settings["OptimizeForHDR"] == false))
            {
                Log.WriteLine("|App.cs| CSAuto not optimized for HDR, asking user to optimize");
                if (MessageBox.Show(Languages.Strings.msgbox_hdroptimization, 
                    Languages.Strings.title_optimization, MessageBoxButton.YesNo, 
                    MessageBoxImage.Question) == MessageBoxResult.Yes) 
                {
                    Settings.Default.oldScreenCaptureWay = true;
                    Settings.Default.Save();
                    MoveSettings();
                }
                else
                {
                    settings.Set("OptimizeForHDR", false);
                }
            }

            new MainApp().Show();
            NativeMethods.OptimizeMemory();
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {

            string probingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bin";
            AssemblyName assyName = new AssemblyName(args.Name);

            string newPath = Path.Combine(probingPath, assyName.Name);
            if (!newPath.EndsWith(".dll"))
            {
                newPath += ".dll";
            }
            if (File.Exists(newPath))
            {
                Assembly assy = Assembly.LoadFile(newPath);
                return assy;
            }
            return null;
        }
        private void ParseArgs(StartupEventArgs e)
        {
            string[] forbiden = new string[]
            {
                "--show",
                "--restart",
                "--cs"
            };
            foreach (string arg in e.Args)
            {
                if (!forbiden.Contains(arg))
                    Args += arg + " ";
                if (arg == "--maximized")
                    AlwaysMaximized = true;
                if (arg == "--show")
                    StartWindow = true;
                if (arg == "--restart")
                    Restarted = true;
                if (arg == "--language" && e.Args.ToList().IndexOf(arg) + 1 < e.Args.Length)
                    languageName = e.Args[e.Args.ToList().IndexOf(arg) + 1];
                if (arg == "--portable")
                    IsPortable = true;
                if (arg == "--log")
                    LogArg = true;
                if (arg == "--cs")
                {
                    Process.Start("steam://rungameid/730");
                    Log.WriteLine("|App.cs| Launching cs on start");
                }
            }
        }

        private void ImportSettings()
        {
            if (settings["FirstRun"] == null || settings["FirstRun"])
            {
                Log.WriteLine("|App.cs| First run of new settings, moving old ones to registry");
                if (WindowsDarkMode())
                    Settings.Default.darkTheme = true;
                else
                    Settings.Default.darkTheme = false;
                MoveSettings();
                List<DiscordRPCButton> discordRPCButtonsOld = DiscordRPCButtonSerializer.DeserializeOld();
                DiscordRPCButtonSerializer.Serialize(discordRPCButtonsOld);
                settings.Set("FirstRun", false);
            }
            else
            {
                Log.WriteLine("|App.cs| Loading registry settings to properties");
                LoadSettings();
            }
        }

        private void ImportAutoBuy()
        {
            buyMenu.Load(settings);
            if (settings.KeyExists("AutoBuyArmor"))
            {
                if (settings["AutoBuyArmor"])
                {
                    buyMenu.GetItem(AutoBuyMenu.NAMES.KevlarVest, true).SetEnabled(true);
                    buyMenu.GetItem(AutoBuyMenu.NAMES.KevlarAndHelmet, true).SetEnabled(true);
                    buyMenu.GetItem(AutoBuyMenu.NAMES.KevlarVest, false).SetEnabled(true);
                    buyMenu.GetItem(AutoBuyMenu.NAMES.KevlarAndHelmet, false).SetEnabled(true);
                }
                settings.Delete("AutoBuyArmor");
            }
            if (settings.KeyExists("AutoBuyDefuseKit"))
            {
                if(settings["AutoBuyDefuseKit"])
                    buyMenu.GetItem(AutoBuyMenu.NAMES.DefuseKit, true).SetEnabled(true);
                settings.Delete("AutoBuyDefuseKit");
            }
            if (settings.KeyExists("PreferArmor"))
            {
                if (settings["PreferArmor"])
                {
                    buyMenu.GetItem(AutoBuyMenu.NAMES.KevlarVest, true).SetPriority(-2);
                    buyMenu.GetItem(AutoBuyMenu.NAMES.KevlarAndHelmet, true).SetPriority(-1);
                    buyMenu.GetItem(AutoBuyMenu.NAMES.KevlarVest, false).SetPriority(-2);
                    buyMenu.GetItem(AutoBuyMenu.NAMES.KevlarAndHelmet, false).SetPriority(-1);
                }
                settings.Delete("PreferArmor");
            }

            if (settings.KeyExists("OldAutoBuy"))
                settings.Delete("OldAutoBuy");
        }

        private bool WindowsDarkMode()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    object registryValueObject = key?.GetValue("AppsUseLightTheme");
                    if (registryValueObject == null)
                    {
                        return false;
                    }

                    int registryValue = (int)registryValueObject;

                    return registryValue <= 0;
                }
            }
            catch { return false; }
        }

        static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
        public static string Unzip(byte[] bytes)
        {
            using (MemoryStream msi = new MemoryStream(bytes))
            using (MemoryStream mso = new MemoryStream())
            {
                using (GZipStream gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        public void LoadSettings()
        {
            foreach (SettingsProperty currentProperty in Settings.Default.Properties)
            {
                if (currentProperty.Name == "availableColors")
                    continue;
                string res = FirstCharToUpper(currentProperty.Name);
                if (!settings.KeyExists(res))
                    settings.Set(res, Settings.Default[currentProperty.Name]);
                Settings.Default[currentProperty.Name] = settings[res].GetValue();
            }
            Settings.Default.Save();
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
                $"{ex.GetType()}: {ex.Message}\n" +
                $"StackTrace:{ex.StackTrace}\n" +
                $"Source: {ex.Source}\n" +
                $"Inner Exception: {ex.InnerException}");

            if(MessageBox.Show(Languages.Strings.ResourceManager.GetString("error_appcrashed"), Languages.Strings.ResourceManager.GetString("title_error") + $" ({frame.GetMethod().Name})", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                Telegram.SendMessage(Telegram.EscapeMarkdown($"CSAuto ({MainApp.FULL_VER} - {CompileInfo.Date} - {CompileInfo.Time}) crash report:\n")+"```csharp" +
                    Telegram.EscapeMarkdown(
                    $"\r\n{ex.GetType()}: {ex.Message}\n" +
                    $"StackTrace:{ex.StackTrace}\n" +
                    $"Source: {ex.Source}\n" +
                    $"Inner Exception: {ex.InnerException}\r\n") + "```", Encoding.UTF8.GetString(Convert.FromBase64String(APIKeys.REPORT_CHAT_ID + "==")), Encoding.UTF8.GetString(Convert.FromBase64String(APIKeys.REPORT_BOT_TOKEN + "==")),true);
        }

    }
}
;