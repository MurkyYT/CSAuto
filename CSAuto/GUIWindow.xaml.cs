﻿using MahApps.Metro.Controls;
using Markdown.Xaml;
using Microsoft.Win32;
using Murky.Utils;
using Murky.Utils.CSGO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;

namespace CSAuto
{
    /// <summary>
    /// Interaction logic for GSIDebugWindow.xaml
    /// </summary>
    public partial class GUIWindow : MetroWindow
    {
        readonly MainWindow main;
        readonly List<string> Colors = new List<string>();
        IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield return (T)Enumerable.Empty<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
                if (ithChild == null) continue;
                if (ithChild is T t) yield return t;
                foreach (T childOfChild in FindVisualChildren<T>(ithChild)) yield return childOfChild;
            }
        }
        public GUIWindow(MainWindow main)
        {
            InitializeComponent();
            Properties.Resources.AVAILABLE_THEME_COLORS.Split(',').ToList().ForEach(x => Colors.Add(x.Replace("\"", "").Trim()));
            this.main = main;
            Steam.GetLaunchOptions(730, out string launchOpt);
            steamInfo.Text = $"Steam Path: \"{Steam.GetSteamPath()}\"\n" +
                $"SteamID3: {Steam.GetCurrentSteamID3()}\n" +
                $"StemID64: {Steam.GetSteamID64()}\n" +
                $"CS:GO FriendCode: {CSGOFriendCode.Encode(Steam.GetSteamID64().ToString())}\n" +
                $"CS:GO Path: \"{Steam.GetGameDir("Counter-Strike Global Offensive")}\"\n" +
                $"CS:GO LaunchOptions: \"{launchOpt}\"";
            //Title = AppLanguage.Get("title_debugwind");
            GenerateLanguages();
            GenerateColors();
        }

        private void GenerateColors()
        {
            int index = -1;
            foreach (string color in Colors)
            {
                if (color == Properties.Settings.Default.currentColor)
                    index = Colors.IndexOf(color);
                ComboBoxItem comboBoxItem = new ComboBoxItem()
                {
                    Content = color
                };
                ColorsComboBox.Items.Add(comboBoxItem);
            }
            ColorsComboBox.SelectedIndex = index;
            ColorsComboBox.SelectionChanged += (s, eArgs) =>
            {
                Properties.Settings.Default.currentColor = (string)(ColorsComboBox.SelectedItem as ComboBoxItem).Content;
                Properties.Settings.Default.Save();
                MessageBoxResult restart = MessageBox.Show(AppLanguage.Get("msgbox_restartneeded"), AppLanguage.Get("title_restartneeded"), MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (restart == MessageBoxResult.OK)
                {
                    Process.Start(Assembly.GetExecutingAssembly().Location);
                    Application.Current.Shutdown();
                }
            };
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

        private void GUIWindow_Closed(object sender, EventArgs e)
        {
            main.debugWind = null;
            Log.debugWind = null;
            Properties.Settings.Default.Save();
        }
        void ParseGameState(string JSON)
        {
            try
            {
                GameState gs = new GameState(JSON);
                Log.WriteLine("-----------------------------");
                foreach (PropertyInfo pi in gs.GetType().GetProperties())
                {
                    Log.WriteLine(
                        string.Format("{0} | {1}",
                                pi.Name,
                                pi.GetValue(gs, null)
                            )
                    );
                    Type type = pi.PropertyType;
                    switch (type.ToString())
                    {
                        case "CSAuto.Player":
                            ParsePlayerGSI(gs, pi);
                            break;
                        case "CSAuto.Match":
                            ParseMatchGSI(gs, pi);
                            break;
                        case "CSAuto.Round":
                            ParseRoundGSI(gs, pi);
                            break;
                    }
                    Log.WriteLine("-----------------------------");
                }
            }
            catch
            {
                Log.WriteLine("There was an error while parsing the GSI file, check if the file is correct");
            }
        }

        private static void ParseRoundGSI(GameState gs, PropertyInfo pi)
        {
            foreach (PropertyInfo pi2 in pi.PropertyType.GetProperties())
            {
                Log.WriteLine(
                    string.Format("{0} | {1}",
                            pi2.Name,
                            pi2.GetValue(TypeConvertor.ConvertPropertyInfoToOriginalType<Round>(pi, gs), null)
                        )
                );
            }
        }

        private static void ParseMatchGSI(GameState gs, PropertyInfo pi)
        {
            foreach (PropertyInfo pi2 in pi.PropertyType.GetProperties())
            {
                Log.WriteLine(
                    string.Format("{0} | {1}",
                            pi2.Name,
                            pi2.GetValue(TypeConvertor.ConvertPropertyInfoToOriginalType<Match>(pi, gs), null)
                        )
                );
            }
        }

        private static void ParsePlayerGSI(GameState gs, PropertyInfo pi)
        {
            Player player = TypeConvertor.ConvertPropertyInfoToOriginalType<Player>(pi, gs);
            foreach (PropertyInfo pi2 in pi.PropertyType.GetProperties())
            {
                Log.WriteLine(
                    string.Format("{0} | {1}",
                            pi2.Name,
                            pi2.GetValue(player, null)
                        )
                );
                Type type = pi2.PropertyType;
                switch (type.ToString())
                {
                    case "CSAuto.Weapon[]":
                        ParseWeaponsGSI(player);
                        break;
                }
            }
            
        }

        private static void ParseWeaponsGSI(Player player)
        {
            foreach (Weapon wep in player.Weapons)
            {
                foreach (PropertyInfo pi3 in wep.GetType().GetProperties())
                {
                    Log.WriteLine(
                        string.Format("{0} | {1}",
                                pi3.Name,
                                pi3.GetValue(wep, null)
                            )
                    );
                }
                Log.WriteLine("-----------------------------");
            }
        }

        private static void ParseWeaponGSI(Player player, PropertyInfo pi2)
        {
            foreach (PropertyInfo pi3 in pi2.PropertyType.GetProperties())
            {
                Log.WriteLine(
                    string.Format("Name: {0} | Value: {1}",
                            pi3.Name,
                            pi3.GetValue(TypeConvertor.ConvertPropertyInfoToOriginalType<Weapon>(pi2, player), null)
                        )
                );
            }
            Log.WriteLine("-----------------------------");
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog
            {
                InitialDirectory = @"C:\",
                Title = "Browse Text Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "txt",
                Filter = "txt files (*.txt)|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ParseGameState(File.ReadAllText(openFileDialog1.FileName));
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            string strWorkPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string fileName = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + $" - {DateTime.Now.ToString("HH;mm;ss")}.txt";
            string path = strWorkPath + "/DEBUG/GSI_OUTPUT/"+ fileName;
            if (outputBox.Text != "None")
            {
                Directory.CreateDirectory(strWorkPath + "/DEBUG/GSI_OUTPUT/");
                using (FileStream fs = File.Create(path))
                {
                    Byte[] title = new UTF8Encoding(true).GetBytes(outputBox.Text);
                    fs.Write(title, 0, title.Length);
                }
            }
        }

        private void GUIWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string finalPath = Log.Path + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + "_Log.txt";
            if (File.Exists(finalPath))
            {
                debugBox.Text = File.ReadAllText(finalPath);
                debugBox.ScrollToEnd();
            }
            LoadLanguages(this);
            string res = Github.GetReleaseDescription("murkyyt", "csauto", MainWindow.VER);
            res = res.Replace("\\r\\n", "\r\n").Split(new string[] { "🛡" },StringSplitOptions.None)[0];
            TextToFlowDocumentConverter converter = new TextToFlowDocumentConverter();
            FlowDocument document = (FlowDocument)converter.Convert(res,null,null,null);
            ChangeLogFlowDocument.Document = document;
            VersionText.Text = $"ver {MainWindow.VER}";
        }

        private void LoadLanguages(DependencyObject obj)
        {
            foreach (CheckBox ch in FindVisualChildren<CheckBox>(obj))
                ch.Content = AppLanguage.Get((string)ch.Content);
            foreach (MetroTabItem ch in FindVisualChildren<MetroTabItem>(obj))
                ch.Header = AppLanguage.Get((string)ch.Header);
            foreach (TextBlock ch in FindVisualChildren<TextBlock>(obj))
                ch.Text = AppLanguage.Get(ch.Text);
            foreach (Button ch in FindVisualChildren<Button>(obj))
                if(ch.Content != null && ch.Content.GetType().Name == "String")
                    ch.Content = AppLanguage.Get((string)ch.Content);
        }

        private void LaunchGitHubSite(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/MurkyYT/CSAuto");
        }
        private void GenerateLanguages()
        {
            foreach (string language in Properties.Settings.Default.languages)
            {
                RadioButton rb = new RadioButton() { Content = AppLanguage.Get(language), IsChecked = language == Properties.Settings.Default.currentLanguage };
                rb.Checked += (sender, args) =>
                {
                    Properties.Settings.Default.currentLanguage = (sender as RadioButton).Tag.ToString();
                    Properties.Settings.Default.Save();
                    MessageBoxResult restart = MessageBox.Show(AppLanguage.Get("msgbox_restartneeded"), AppLanguage.Get("title_restartneeded"), MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (restart == MessageBoxResult.OK)
                    {
                        Process.Start(Assembly.GetExecutingAssembly().Location);
                        Application.Current.Shutdown();
                    }
                };
                rb.Tag = language;
                languagesStackPanel.Children.Add(rb);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IEnumerable<DependencyObject> enumarble = ((sender as TabControl).SelectedItem as MetroTabItem).GetChildObjects();
            if(enumarble.Count() > 0)
                LoadLanguages((enumarble.First() as StackPanel));
        }

        private void StartUpCheck_Click(object sender, RoutedEventArgs e)
        {
            string appname = Assembly.GetEntryAssembly().GetName().Name;
            string executablePath = Process.GetCurrentProcess().MainModule.FileName;
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (Properties.Settings.Default.runAtStartUp)
                {
                    rk.SetValue(appname, executablePath);
                }
                else
                {
                    rk.DeleteValue(appname, false);
                }
            }
        }

        private void DarkThemeCheck_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult restart = MessageBox.Show(AppLanguage.Get("msgbox_restartneeded"), AppLanguage.Get("title_restartneeded"), MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (restart == MessageBoxResult.OK)
            {
                Process.Start(Assembly.GetExecutingAssembly().Location);
                Application.Current.Shutdown();
            }
        }

        private void BotButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://t.me/csautonotification_bot");
        }
    }
}
