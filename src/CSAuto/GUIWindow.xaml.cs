using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MdXaml;
using Microsoft.Win32;
using Murky.Utils;
using Murky.Utils.CSGO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;

namespace CSAuto
{
    /// <summary>
    /// Interaction logic for GSIDebugWindow.xaml
    /// </summary>
    public partial class GUIWindow : MetroWindow
    {
        readonly MainApp main = (MainApp)Application.Current.MainWindow;
        readonly StringCollection Colors = Properties.Settings.Default.availableColors;
        readonly GameState GameState = new GameState(Properties.Resources.GAMESTATE_EXAMPLE);
        private BuyItem selectedItem = null;
        private CustomBuyItem customSelectedItem = null;
        bool isCt = true;
        public GUIWindow()
        {
            InitializeComponent();
            if(main.current.IsWindows11)
            {
                IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
                var attribute = NativeMethods.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
                var preference = NativeMethods.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                NativeMethods.DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
            }
            Dispatcher.InvokeAsync(() => { AutoBuyImage.Source = main.current.buyMenu.GetImage(isCt); });
            PortableModeCheck.IsChecked = File.Exists(Log.WorkPath + "\\resource\\.portable");
            IsPortableText.Text = $"Portable: {PortableModeCheck.IsChecked}";
            Title += $" {MainApp.FULL_VER}";
        }
        private async Task RestartMessageBox()
        {
            var restart = await ShowMessage("title_restartneeded", "msgbox_restartneeded", MessageDialogStyle.AffirmativeAndNegative);
            if (restart == MessageDialogResult.Affirmative)
            {
                Process.Start(Assembly.GetExecutingAssembly().Location,"--restart --show " + main.current.Args);
                Application.Current.Shutdown();
            }
        }
        private async Task<string> CallInputDialogAsync(string title,string message)
        {
            return await this.ShowInputAsync(Languages.Strings.ResourceManager.GetString(title), 
                Languages.Strings.ResourceManager.GetString(message));
        }
        public void UpdateText(string data)
        {
            Dispatcher.InvokeAsync(() =>
            {
                outputBox.Text = data;
                lastRecieveTime.Text = $"Last recieved data from GSI: {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}";
            });
        }
        public void UpdateDebug(string data)
        {
            Dispatcher.InvokeAsync(() =>
            {
                debugBox.Text += data+'\n';
            });
        }

        private void GUIWindow_Closed(object sender, EventArgs e)
        {
            main.guiWindow = null;
            Log.debugWind = null;
            Properties.Settings.Default.Save();
            main.current.MoveSettings();
            GameState.Dispose();
            Close();
            main.current.buyMenu.Save(main.current.settings);
            NativeMethods.OptimizeMemory();
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
                        case "Murky.Utils.CSGO.Player":
                            ParsePlayerGSI(gs, pi);
                            break;
                        case "Murky.Utils.CSGO.Match":
                            ParseMatchGSI(gs, pi);
                            break;
                        case "Murky.Utils.CSGO.Round":
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
                    case "Murky.Utils.CSGO.Weapon[]":
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

        //private static void ParseWeaponGSI(Player player, PropertyInfo pi2)
        //{
        //    foreach (PropertyInfo pi3 in pi2.PropertyType.GetProperties())
        //    {
        //        Log.WriteLine(
        //            string.Format("Name: {0} | Value: {1}",
        //                    pi3.Name,
        //                    pi3.GetValue(TypeConvertor.ConvertPropertyInfoToOriginalType<Weapon>(pi2, player), null)
        //                )
        //        );
        //    }
        //    Log.WriteLine("-----------------------------");
        //}

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
            string fileName = $"{DateTime.Now.Day}.{DateTime.Now.Month}.{DateTime.Now.Year} - {DateTime.Now:HH;mm;ss}.txt";
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
            new Thread(() =>
            {
                Steam.GetLaunchOptions(730, out string launchOpt);
                Dispatcher.InvokeAsync(() =>
                {
                    steamInfo.Text = $"Steam Path: \"{Steam.GetSteamPath()}\"\n" +
                        $"SteamID3: {Steam.GetCurrentSteamID3()}\n" +
                        $"SteamID64: {Steam.GetSteamID64()}\n" +
                        $"CS:GO FriendCode: {CSGOFriendCode.Encode(Steam.GetSteamID64().ToString())}\n" +
                        $"CS:GO Path: \"{Steam.GetGameDir("Counter-Strike Global Offensive")}\"\n" +
                        $"CS:GO LaunchOptions: \"{launchOpt}\"";
                    GenerateLanguages();
                    string finalPath = Log.Path + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + "_Log.txt";
                    if (File.Exists(finalPath))
                        debugBox.Text = File.ReadAllText(finalPath);
                    if (main.current.AlwaysMaximized)
                        WindowState = WindowState.Maximized;
                    //LoadLanguages(this);
                    //VersionText.Text = $"ver {MainApp.FULL_VER}";
                    UpdateDiscordRPCResult(true);
                    LoadDiscordButtons();
                });
            }).Start();
        }

        private void LoadDiscordButtons()
        {
            DiscordRPCButtonsListView.Items.Clear();
            foreach (DiscordRPCButton button in main.discordRPCButtons)
            {
                ListViewItem item = new ListViewItem
                {
                    Content = button
                };
                DiscordRPCButtonsListView.Items.Add(item);
            }
        }

        private void LoadChangelog()
        {
            string res = Github.GetWebInfo($"https://raw.githubusercontent.com/MurkyYT/CSAuto/{MainApp.ONLINE_BRANCH_NAME}/Docs/FullChangelog.MD");
            if (res.Length == 0)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    _ = ShowMessage(Languages.Strings.ResourceManager.GetString("title_error"), Languages.Strings.ResourceManager.GetString("error_loadchangelog"), MessageDialogStyle.Affirmative);
                });
                return;
            }
            // Well MdXaml cant use only 2 spaces to add indetation so replace "  " with "    "
            res = res.Replace("  ", "    ");
            Dispatcher.InvokeAsync(() => {
                TextToFlowDocumentConverter converter = new TextToFlowDocumentConverter();
                FlowDocument document = (FlowDocument)converter.Convert(res, null, null, null);
                ChangeLogFlowDocument.Document = document;
                });
        }

        //private void LoadLanguages(DependencyObject obj)
        //{
        //    Dispatcher.InvokeAsync(() =>
        //    {
        //        foreach (CheckBox ch in FindVisualChildren<CheckBox>(obj))
        //            ch.Content = Languages.Strings.ResourceManager.GetString((string)ch.Content];
        //        foreach (MetroTabItem ch in FindVisualChildren<MetroTabItem>(obj))
        //            ch.Header = Languages.Strings.ResourceManager.GetString((string)ch.Header];
        //        foreach (TextBlock ch in FindVisualChildren<TextBlock>(obj))
        //        {
        //            if (Colors.Contains(ch.Text) ||
        //                ch.Text == "" ||
        //                ch.Text == "Steam" ||
        //                ch.Text == "Faceit" ||
        //                ch.Text == "CSGOStats")
        //                continue;
        //            ch.Text = Languages.Strings.ResourceManager.GetString(ch.Text];
        //        }
        //        foreach (Button ch in FindVisualChildren<Button>(obj))
        //            if (ch.Content != null && ch.Content.GetType().Name == "String")
        //                ch.Content = Languages.Strings.ResourceManager.GetString((string)ch.Content];
        //        foreach (DropDownButton ch in FindVisualChildren<DropDownButton>(obj))
        //            ch.Content = Languages.Strings.ResourceManager.GetString((string)ch.Content];
        //        foreach (TextBox tb in FindVisualChildren<TextBox>(obj))
        //            tb.ToolTip = tb.ToolTip != null ? tb.ToolTip = Languages.Strings.ResourceManager.GetString((string)tb.ToolTip] : null;
        //    });
        //}

        private void LaunchGitHubSite(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/MurkyYT/CSAuto");
        }
        private void GenerateLanguages()
        {
            foreach (string language in AppLanguage.Available)
            {
                RadioButton rb = new RadioButton() { Content = Languages.Strings.ResourceManager.GetString("language_"+language), IsChecked = language == Properties.Settings.Default.currentLanguage };
                rb.Checked += async (sender, args) =>
                {
                    Properties.Settings.Default.currentLanguage = (sender as RadioButton).Tag.ToString();
                    Properties.Settings.Default.Save();
                    CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo((sender as RadioButton).Tag.ToString());
                    await RestartMessageBox();
                };
                rb.Tag = language;
                languagesStackPanel.Children.Add(rb);
            }
        }
        private void StartUpCheck_Click(object sender, RoutedEventArgs e)
        {
            string appname = Assembly.GetEntryAssembly().GetName().Name;
            string executablePath = Process.GetCurrentProcess().MainModule.FileName;
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (Properties.Settings.Default.runAtStartUp)
                {
                    rk.SetValue(appname, executablePath + " "+main.current.Args);
                }
                else
                {
                    rk.DeleteValue(appname, false);
                }
            }
        }
        public async Task<MessageDialogResult> ShowMessage(string title,
           string message, MessageDialogStyle dialogStyle)
        {
            return await this.ShowMessageAsync(
                Languages.Strings.ResourceManager.GetString(title), Languages.Strings.ResourceManager.GetString(message), dialogStyle);
        }
        private void DarkThemeCheck_Click(object sender, RoutedEventArgs e)
        {
            //await RestartMessageBox();
            UpdateTheme();
        }

        private void BotButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://t.me/csautonotification_bot");
        }

        private void ColorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Properties.Settings.Default.currentColor == Colors[ColorsComboBox.SelectedIndex])
                return;
            Properties.Settings.Default.currentColor = Colors[ColorsComboBox.SelectedIndex];
            UpdateTheme();
        }

        private void UpdateTheme()
        {
            if (Properties.Settings.Default.darkTheme)
                // Set the application theme to Dark + selected color
                ThemeManager.Current.ChangeTheme(Application.Current, $"Dark.{Properties.Settings.Default.currentColor}");
            else
                // Set the application theme to Light + selected color
                ThemeManager.Current.ChangeTheme(Application.Current, $"Light.{Properties.Settings.Default.currentColor}");
            main.InitializeContextMenu();
        }

        private void CategoriesTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriesTabControl.SelectedItem != null &&
                CategoriesTabControl.SelectedIndex == 1
                && ChangeLogFlowDocument.Document.Blocks.LastBlock.ContentStart.Paragraph != null)
                new Thread(() => { LoadChangelog(); }).Start();
            else if (CategoriesTabControl.SelectedItem != null && CategoriesTabControl.SelectedIndex == 2)
            {
                OldCaptureText.Text = Properties.Settings.Default.oldScreenCaptureWay ? "Old capture" : "New capture";
                DebugButtonColor.Text = $"Regular: {main.BUTTON_COLORS[0]}, Active: {main.BUTTON_COLORS[1]}";
            }
            else if (CategoriesTabControl.SelectedItem != null && CategoriesTabControl.SelectedIndex == 4)
            {
                UpdateImage();
            }
        }

        private void InGameDetailsText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.inGameDetails = InGameDetailsText.Text;
            UpdateDiscordRPCResult(false);
        }
        private void InGameStateText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.inGameState = InGameStateText.Text;
            UpdateDiscordRPCResult(false);
        }
        private void LobbyDetailsText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.lobbyDetails = LobbyDetailsText.Text;
            UpdateDiscordRPCResult(true);
        }

        private void LobbyStateText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.lobbyState = LobbyStateText.Text;
            UpdateDiscordRPCResult(true);
        }
        private void UpdateDiscordRPCResult(bool lobby)
        {
            if (DiscordRpcDetails == null || DiscordRpcState == null)
                return;
            if (lobby) 
            {
                DiscordRpcDetails.Text = main.LimitLength(main.FormatString(Properties.Settings.Default.lobbyDetails, GameState),128);
                DiscordRpcState.Text = main.LimitLength(main.FormatString(Properties.Settings.Default.lobbyState, GameState),128);
            }
            else
            {
                DiscordRpcDetails.Text = main.LimitLength(main.FormatString(Properties.Settings.Default.inGameDetails, GameState),128);
                DiscordRpcState.Text = main.LimitLength(main.FormatString(Properties.Settings.Default.inGameState, GameState),128);
            }
        }

        private void DiscordTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDiscordRPCResult(DiscordTabControl.SelectedIndex == 0);
        }

        private void TelegramTestMessage_Click(object sender, RoutedEventArgs e)
        {
            Telegram.SendMessage("Test Message!",Properties.Settings.Default.telegramChatId, 
                Telegram.CheckToken(Properties.Settings.Default.customTelegramToken) ? 
                Properties.Settings.Default.customTelegramToken : APIKeys.TELEGRAM_BOT_TOKEN);
        }

        private void RemoveDiscordButton_Click(object sender, RoutedEventArgs e)
        {
            if(DiscordRPCButtonsListView.SelectedIndex != -1)
            {
                main.discordRPCButtons.Remove(main.discordRPCButtons[DiscordRPCButtonsListView.SelectedIndex]);
                LoadDiscordButtons();
                DiscordRPCButtonSerializer.Serialize(main.discordRPCButtons);
            }
        }

        private async void AddDiscordButton_Click(object sender, RoutedEventArgs e)
        {
            if(main.discordRPCButtons.Count == 1)
            {
                await ShowMessage("title_error", "error_max1discord", MessageDialogStyle.Affirmative);
                return;
            }
            string label = await CallInputDialogAsync(Languages.Strings.ResourceManager.GetString("inputtext_label"),
                Languages.Strings.ResourceManager.GetString("inputtext_enterlabel"));
            string url = await CallInputDialogAsync(Languages.Strings.ResourceManager.GetString("inputtext_url")
                , Languages.Strings.ResourceManager.GetString("inputtext_enterurl"));
            if(label == null || url == null || label.Trim() == "" || url.Trim() == "" || !Uri.IsWellFormedUriString(url,UriKind.Absolute))
            {
                await ShowMessage("title_error", "error_entervalid", MessageDialogStyle.Affirmative);
                return;
            }
            DiscordRPCButton res = new DiscordRPCButton() { Label = label, Url = url };
            main.discordRPCButtons.Add(res);
            LoadDiscordButtons();
            DiscordRPCButtonSerializer.Serialize(main.discordRPCButtons);
        }

        private async void DiscordTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            if(DiscordTemplateComboBox.SelectedIndex == 0)
            {
                await ShowMessage("title_error", "error_discordselecttemplate", MessageDialogStyle.Affirmative);
                return;
            }
            if (main.discordRPCButtons.Count == 1)
            {
                await ShowMessage("title_error", "error_max1discord", MessageDialogStyle.Affirmative);
                return;
            }
            string label = await CallInputDialogAsync(Languages.Strings.ResourceManager.GetString("inputtext_label")
                , Languages.Strings.ResourceManager.GetString("inputtext_enterlabel"));
            if (label == null || label.Trim() == "")
            {
                await ShowMessage("title_error", "error_entervalid", MessageDialogStyle.Affirmative);
                return;
            }
            string url = "";
            switch (DiscordTemplateComboBox.SelectedIndex)
            {
                case 1:
                    url = "https://steamcommunity.com/profiles/{SteamID}";
                    break;
                case 2:
                    url = "https://faceitfinder.com/profile/{SteamID}";
                    break;
                case 3:
                    url = "https://csgostats.gg/player/{SteamID}";
                    break;
            }
            DiscordRPCButton res = new DiscordRPCButton() { Label = label, Url = url };
            main.discordRPCButtons.Add(res);
            LoadDiscordButtons();
            DiscordRPCButtonSerializer.Serialize(main.discordRPCButtons);
        }

        private void OpenDiscordServer(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/57ZEVZgm5W");
        }

        private void AutoBuyImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Point pos = e.GetPosition(AutoBuyImage);
                Size size = AutoBuyImage.RenderSize;
                double x_ratio = size.Width / main.current.buyMenu.size.Width;
                double y_ratio = size.Height / main.current.buyMenu.size.Height;
                BuyItem item = main.current.buyMenu.GetItem(new Point(pos.X / x_ratio, pos.Y / y_ratio), isCt);
                if (item != null)
                {
                    selectedItem = item;
                    SelectedCustomItemPropery.Visibility = Visibility.Hidden;
                    BuyItemProperties.Visibility = Visibility.Visible;
                    AutoBuyImage.Visibility = Visibility.Hidden;
                    AutoBuyTab.Visibility = Visibility.Hidden;
                    AutoBuyEnabledCheckBox.Visibility = Visibility.Hidden;
                    BuyItemName.Text = Languages.Strings.ResourceManager.GetString($"buyitem_{item.Name.ToString().ToLower()}");
                    BuyItemPriority.Value = item.GetPriority();
                    BuyItemEnabledCheckBox.IsChecked = item.IsEnabled();
                    CheckIsCustom(item);
                }
                //else
                //    await ShowMessage("title_error", "error_notimplemented", MessageDialogStyle.Affirmative);         
            }
        }

        private void CheckIsCustom(BuyItem item)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (item is CustomBuyItem)
                {
                    CustomBuyItem customItem = item as CustomBuyItem;
                    SelectedCustomItemPropery.Visibility = Visibility.Visible;
                    StackPanel box = SelectedCustomItemPropery.Children[1] as StackPanel;
                    box.Children.Clear();
                    if (isCt)
                    {
                        AutoBuyMenu.NAMES[] options = customItem.GetCTOptions();
                        foreach (AutoBuyMenu.NAMES option in options)
                        {
                            RadioButton rb = new RadioButton
                            {
                                Content = Languages.Strings.ResourceManager.GetString($"buyitem_{option.ToString().ToLower()}"),
                                IsChecked = option == customItem.GetName(),
                                Tag = option
                            };
                            box.Children.Add(rb);
                        }
                    }
                    else
                    {
                        AutoBuyMenu.NAMES[] options = customItem.GetTOptions();
                        foreach (AutoBuyMenu.NAMES option in options)
                        {
                            RadioButton rb = new RadioButton
                            {
                                Content = Languages.Strings.ResourceManager.GetString($"buyitem_{option.ToString().ToLower()}"),
                                IsChecked = option == customItem.GetName(),
                                Tag = option
                            };
                            box.Children.Add(rb);
                        }
                    }
                    customSelectedItem = customItem;
                }
            });
        }

        private async void ApplyBuyItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null)
            {
                selectedItem.SetEnabled((bool)BuyItemEnabledCheckBox.IsChecked);
                selectedItem.SetPriority((int)BuyItemPriority.Value);
                selectedItem = null;
                BuyItemProperties.Visibility = Visibility.Hidden;
                AutoBuyImage.Visibility = Visibility.Visible;
                AutoBuyTab.Visibility = Visibility.Visible;
                AutoBuyEnabledCheckBox.Visibility = Visibility.Visible;
                if (customSelectedItem != null)
                {
                    CustomBuyItem item = customSelectedItem;
                    int selectedIndex = 0;
                    StackPanel box = SelectedCustomItemPropery.Children[1] as StackPanel;
                    for (int i = 0;i<box.Children.Count;i++)
                    {
                        object child = box.Children[i];
                        if ((bool)((RadioButton)child).IsChecked)
                        {
                            selectedIndex = i;
                            break;
                        }
                    }
                    AutoBuyMenu.NAMES[] options = isCt ? item.GetCTOptions() : item.GetTOptions();
                    if (options[selectedIndex] == AutoBuyMenu.NAMES.None || !main.current.buyMenu.ContainsCustom(isCt, options[selectedIndex]))
                        item.SetName(options[selectedIndex]);
                    else if(item.GetName() != options[selectedIndex])
                        await ShowMessage("title_error", "error_alreadycontainscustom", MessageDialogStyle.Affirmative);
                    customSelectedItem = null;
                }
                UpdateImage();
            }
            main.current.buyMenu.Save(main.current.settings);
        }

        private void UpdateImage()
        {
            //BitmapSource src = main.current.buyMenu.GetImage(isCt);
            //src.Freeze();
            //await Dispatcher.InvokeAsync(() => { AutoBuyImage.Source = src; });
            AutoBuyImage.Source = main.current.buyMenu.GetImage(isCt);
            NativeMethods.OptimizeMemory();
        }

        private void AutoBuyTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;
            isCt = control.SelectedIndex == 0;
        }

        private async void PortableModeCheck_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)PortableModeCheck.IsChecked)
            {
                if(File.Exists(Log.WorkPath + "\\resource\\.portable"))
                {
                    File.Delete(Log.WorkPath + "\\resource\\.portable");
                    await RestartMessageBox();
                }
            }
            else
            {
                File.Create(Log.WorkPath + "\\resource\\.portable");
                await RestartMessageBox();
            }
        }

        private void OpenWebSite_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://csauto.vercel.app/changelog");
        }

        private void OpenGithubChangelog_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/MurkyYT/CSAuto/blob/master/Docs/FullChangelog.MD");
        }

        private void debugBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            debugBox.ScrollToEnd();
        }
        private void debugBox_Loaded(object sender, RoutedEventArgs e)
        {
            debugBox_TextChanged(sender, null);
        }

        private void DebugSettings_Click(object sender, RoutedEventArgs e)
        {
            new DebugSettings(this.main).Show();
        }
    }
}
