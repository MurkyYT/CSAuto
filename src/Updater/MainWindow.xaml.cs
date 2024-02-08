using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using static Updater.Unzip;

namespace Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string basePath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        static string zipPath = basePath + "\\temp.zip";
        static string tempFolderPath = basePath + "\\temp";
        static string targetPath = tempFolderPath + " - Copy";
        static string downloadLink;
        static string mainExePath = targetPath+"\\CSAuto.exe";
        static string exeArgs = " --restart --maximized";
        static string[] filesToSkip = new string[] { ".portable" };
        public MainWindow(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            InitializeComponent();
            if (args.Length < 5) { Application.Current.Shutdown(); return; }
            string arg = "";
            for (int i = 0; i < args.Length; i++)
                arg += args[i] + ",";
            Log.WriteLine($"Args are {arg}");
            targetPath = args[0];
            downloadLink = args[1];
            mainExePath = targetPath + "\\" + args[2];
            exeArgs = args[3];
            filesToSkip = args[4].Split(',');
            StartDownload(downloadLink, zipPath);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Log.WriteLine(
                $"{ex.Message}\n" +
                $"StackTrace:{ex.StackTrace}\n" +
                $"Source: {ex.Source}\n" +
                $"Inner Exception: {ex.InnerException}");
            MessageBox.Show("Update has failed, try to download latest version from https://github.com/MurkyYT/CSAuto/releases", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }

        private void StartDownload(string url,string toWhere)
        {
            ProgressTextBox.Text = "Downloading zip... (0%)";
            Log.WriteLine($"Started downloading '{url}' to '{toWhere}'");
            new Thread(() =>
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
                client.DownloadFileAsync(new Uri(url), toWhere);
            }
            ).Start();
        }
        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.InvokeAsync(() => 
            { 
                ProgressBar.Value = e.ProgressPercentage;
                ProgressTextBox.Text = $"Downloading zip... ({e.ProgressPercentage}%)";
            });
            Log.WriteLine($"Percentage: {e.ProgressPercentage}");
        }
        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Unzip();
        }

        private void Unzip()
        {
            using (var unzip = new Unzip(zipPath))
            {
                if (Directory.Exists(tempFolderPath))
                    Directory.Delete(tempFolderPath, true);
                Directory.CreateDirectory(tempFolderPath);
                Entry[] entries = unzip.Entries;
                for (int i = 0; i < entries.Length; i++)
                {
                    Entry entry = entries[i];
                    Dispatcher.InvokeAsync(() =>
                    {
                        ProgressTextBox.Text = $"Extracting zip... ({i}/{entries.Length})";
                        ProgressBar.Value = ((double)i / entries.Length) * 100;
                    });
                    if (entry.IsFile)
                    {
                        string extractPath = $"{tempFolderPath}\\{entry.Name}";
                        unzip.Extract(entry.Name, extractPath);
                    }
                    else if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory($"{tempFolderPath}\\{entry.Name}");
                    }
                    Log.WriteLine($"Extracted: {entry.Name}");
                }
                Dispatcher.InvokeAsync(() =>
                {
                    ProgressTextBox.Text = $"Extracting zip... ({entries.Length}/{entries.Length})";
                    ProgressBar.Value = 100;
                });
            }
            File.Delete(zipPath);
            CopyToDestination(tempFolderPath, targetPath);
        }

        private void CopyToDestination(string source,string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(s => Path.GetDirectoryName(s));
            int amountOfFiles = 0;
            foreach (var folder in files)
                foreach (var file in folder)
                    if(!filesToSkip.Contains(file))
                        amountOfFiles++;
            int currentFile = 0;
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    if (filesToSkip.Contains(file.Split('\\').Last()))
                    {
                        Log.WriteLine("Skipping "+file);
                        continue;
                    }
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                    Dispatcher.InvokeAsync(() =>
                    {
                        ProgressTextBox.Text = $"Moving files... ({currentFile}/{amountOfFiles})";
                        ProgressBar.Value = ((double)currentFile / amountOfFiles) * 100;
                    });
                    currentFile++;
                    Log.WriteLine($"Copied: {file}");
                }
            }
            Directory.Delete(source, true);
            Dispatcher.InvokeAsync(() =>
            {
                ProgressTextBox.Text = $"Openning main exe";
            });
            Process.Start(mainExePath, exeArgs);
            Dispatcher.InvokeAsync(() =>
            {
                Application.Current.Shutdown();
            });
            if (File.Exists(MainWindow.targetPath + "\\Update_Log.txt"))
                File.Delete(MainWindow.targetPath + "\\Update_Log.txt");
            Log.WriteLine("Succesfully updated!");
            File.Move(basePath + "\\Update_Log.txt", MainWindow.targetPath + "\\Update_Log.txt");
        }
    }
}
