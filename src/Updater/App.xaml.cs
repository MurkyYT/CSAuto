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
            if(e.Args.Length == 0)
            {
                MessageBox.Show("Not enough arguments supplied","Error",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            if (e.Args[0] == "--cleanup")
            {
                string[] files = Directory.GetFiles(Log.WorkPath + "\\..", "*.dll");
                bool didCleanOld = files.Length > 0;
                foreach (var file in files)
                {
                    while (true)
                    {
                        try
                        {
                            File.Delete(file);
                            break;
                        }
                        catch { }
                    }
                }
                if (didCleanOld)
                {
                    while (true)
                    {
                        try
                        {
                            Directory.Delete(Log.WorkPath + "\\..\\ru");
                            break;
                        }
                        catch { }
                    }
                    try
                    {
                        File.Delete(Log.WorkPath + "\\..\\steamapi.exe");
                        File.Delete(Log.WorkPath + "\\..\\updater.exe");
                    }
                    catch { }
                }
                Process.Start(Log.WorkPath + "\\..\\CSAuto.exe", e.Args[1]);
                App.Current.Shutdown();
            }
            else
                new MainWindow(e.Args).Show();
        }
    }
}
