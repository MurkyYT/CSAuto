using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Updater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                MessageBox.Show("Not enough arguments supplied", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }
            if (e.Args[0] == "--cleanup")
            {
                string baseDir = Path.GetFullPath(Path.Combine(Log.WorkPath, ".."));

                string[] files = Directory.GetFiles(baseDir, "*.dll");
                bool didCleanOld = files.Length > 0;

                foreach (var file in files)
                {
                    int attempts = 0;
                    while (true)
                    {
                        if (attempts > 5)
                            break;

                        try
                        {
                            attempts++;
                            File.Delete(file);
                            break;
                        }
                        catch { Thread.Sleep(100); }
                    }
                }


                string binPath = Path.Combine(baseDir, "bin");

                string[] keep =
                {
                    "DXGICapture.dll",
                    "steam_api.dll",
                    "Steamworks.NET.dll"
                };

                HashSet<string> keepSet = new HashSet<string>(keep, StringComparer.OrdinalIgnoreCase);

                if (Directory.Exists(binPath))
                {
                    foreach (var dll in Directory.GetFiles(binPath, "*.dll"))
                    {
                        if (keepSet.Contains(Path.GetFileName(dll)))
                            continue;

                        int attempts = 0;
                        while (true)
                        {
                            if (attempts > 5)
                                break;

                            try
                            {
                                attempts++;
                                File.Delete(dll);
                                break;
                            }
                            catch { Thread.Sleep(100); }
                        }
                    }
                }

                if (didCleanOld)
                {
                    int attempts = 0;
                    while (true)
                    {
                        if (attempts > 5)
                            break;

                        try
                        {
                            attempts++;
                            Directory.Delete(Path.Combine(baseDir, "ru"));
                            break;
                        }
                        catch { Thread.Sleep(100); }
                    }

                    try
                    {
                        File.Delete(Path.Combine(baseDir, "steamapi.exe"));
                        File.Delete(Path.Combine(baseDir, "updater.exe"));
                    }
                    catch { }
                }


                Process.Start(Path.Combine(baseDir, "CSAuto.exe"), e.Args[1]);
                App.Current.Shutdown();
            }
            else
                new MainWindow(e.Args).Show();
        }
    }
}
