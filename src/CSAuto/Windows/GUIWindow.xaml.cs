using ControlzEx.Theming;
using CSAuto.Properties;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MdXaml;
using Microsoft.Win32;
using Murky.Utils;
using Murky.Utils.CS;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace CSAuto
{
    /// <summary>
    /// Interaction logic for GSIDebugWindow.xaml
    /// </summary>
    public partial class GUIWindow : MetroWindow
    {
        readonly MainApp main = (MainApp)Application.Current.MainWindow;

        readonly string[] Colors =
        {
            "Red", 
            "Green", 
            "Blue", 
            "Purple", 
            "Orange", 
            "Lime", 
            "Emerald", 
            "Teal", 
            "Cyan", 
            "Cobalt", 
            "Indigo", 
            "Violet",
            "Pink", 
            "Magenta", 
            "Crimson", 
            "Amber",
            "Yellow", 
            "Brown", 
            "Olive", 
            "Steel", 
            "Mauve", 
            "Taupe", 
            "Sienna"
        };

        readonly GameState GameState = new GameState(Properties.Resources.GAMESTATE_EXAMPLE);

        private BuyItem selectedItem = null;
        private CustomBuyItem customSelectedItem = null;
        private bool isCt = true;

        public GUIWindow()
        {
            InitializeComponent();

            if (main.clients != null)
            {
                lock (main.clients)
                {
                    foreach (var client in main.clients)
                        ClientsListBox?.Items.Add(client.Client.RemoteEndPoint);
                }
            }

            if(main.current.IsWindows11)
            {
                IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
                var attribute = NativeMethods.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
                var preference = NativeMethods.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                NativeMethods.DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
            }

            if (main.current.RTLLanguage)
            {
                FlowDirection = FlowDirection.RightToLeft;
            }

            foreach (string color in Colors)
            {
                string translated = Languages.Strings.ResourceManager.GetString($"color_{color.ToLower()}");
                ColorsComboBox.Items.Add(translated ?? color);
                if(Settings.Default.currentColor == color)
                    ColorsComboBox.SelectedIndex = ColorsComboBox.Items.Count - 1;
            }

            Dispatcher.InvokeAsync(() => { AutoBuyImage.Source = main.current.buyMenu.GetImage(isCt); });
            PortableModeCheck.IsChecked = File.Exists(Log.WorkPath + "\\resource\\.portable");
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
                lastRecieveTime.Text = $"Last recieved data from GSI: {DateTime.Now:HH:mm:ss.fff}";
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
            GameState.Dispose();
            Close();
            NativeMethods.OptimizeMemory();
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            string strWorkPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fileName = $"{DateTime.Now.Day}.{DateTime.Now.Month}.{DateTime.Now.Year} - {DateTime.Now:HH;mm;ss}.txt";
            string path = strWorkPath + "\\debug\\GSI\\"+ fileName;
            if (outputBox.Text != "None")
            {
                Directory.CreateDirectory(strWorkPath + "\\debug\\GSI\\");
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
                Dispatcher.InvokeAsync(() =>
                {
                    debugBox.Text = Log.MemoryLog;
                    ServerIP.Text = $"IP: {main.GetLocalIPAddress()}";
                    GenerateLanguages();
                    if (main.current.AlwaysMaximized)
                        WindowState = WindowState.Maximized;
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
                    _ = ShowMessage("title_error", "error_loadchangelog", MessageDialogStyle.Affirmative);
                });
                return;
            }

            Dispatcher.InvokeAsync(async () =>
            {
                ChangeLogFlowDocument.Document = await MarkdownHelper.GetDocumentAsync(res);
            });
        }

        private void LaunchGitHubSite(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/MurkyYT/CSAuto");
        }
        private void GenerateLanguages()
        {
            foreach (AppLanguage.Language language in AppLanguage.Available)
            {
                if (!language.Enabled)
                    continue;

                ComboBoxItem item = new ComboBoxItem() 
                {  
                    Content = Languages.Strings.ResourceManager.GetString("language_" + language.LanguageCode),
                    Tag = language.LanguageCode
                };

                if (language.LanguageCode == Settings.Default.currentLanguage) languagesComboBox.SelectedItem = item;
                if (!string.IsNullOrEmpty(language.OriginalName)) item.Content += $" ({language.OriginalName})";

                languagesComboBox.Items.Add(item);
            }

            languagesComboBox.SelectionChanged += async (sender, args) =>
            {
                ComboBoxItem item = languagesComboBox.SelectedItem as ComboBoxItem;
                Settings.Default.currentLanguage = item.Tag.ToString();
                Settings.Default.Save();
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(item.Tag.ToString());
                languagesComboBox.SelectedItem = item;
                await RestartMessageBox();
            };
        }
        private void StartUpCheck_Click(object sender, RoutedEventArgs e)
        {
            string appname = Assembly.GetEntryAssembly().GetName().Name;
            string executablePath = Process.GetCurrentProcess().MainModule.FileName;
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (Settings.Default.runAtStartUp)
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
            var settings = new MetroDialogSettings
            {
                AffirmativeButtonText = Languages.Strings.msgbox_ok,
                NegativeButtonText = Languages.Strings.msgbox_cancel
            };

            return await this.ShowMessageAsync(
                Languages.Strings.ResourceManager.GetString(title), Languages.Strings.ResourceManager.GetString(message), dialogStyle, settings);
        }
        private void DarkThemeCheck_Click(object sender, RoutedEventArgs e)
        {
            UpdateTheme();
        }

        private void BotButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://t.me/csautonotification_bot");
        }

        private void ColorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Settings.Default.currentColor == Colors[ColorsComboBox.SelectedIndex])
                return;
            Settings.Default.currentColor = Colors[ColorsComboBox.SelectedIndex];
            UpdateTheme();
        }

        private void UpdateTheme()
        {
            if (Settings.Default.darkTheme)
                // Set the application theme to Dark + selected color
                ThemeManager.Current.ChangeTheme(Application.Current, $"Dark.{Settings.Default.currentColor}");
            else
                // Set the application theme to Light + selected color
                ThemeManager.Current.ChangeTheme(Application.Current, $"Light.{Settings.Default.currentColor}");
            main.InitializeContextMenu();

            FlowDocument doc = new FlowDocument();
            doc.Blocks.Add(new Paragraph(new Run("Loading...")));

            ChangeLogFlowDocument.Document = doc;
        }

        private void CategoriesTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriesTabControl.SelectedItem != null &&
                CategoriesTabControl.SelectedIndex == 3
                && ChangeLogFlowDocument.Document.Blocks.LastBlock.ContentStart.Paragraph != null)
                new Thread(() => { LoadChangelog(); }).Start();
            else if (CategoriesTabControl.SelectedItem != null && CategoriesTabControl.SelectedIndex == 2)
            {
                UpdateImage();
            }
        }

        private void InGameDetailsText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.inGameDetails = InGameDetailsText.Text;
            UpdateDiscordRPCResult(false);
        }
        private void InGameStateText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.inGameState = InGameStateText.Text;
            UpdateDiscordRPCResult(false);
        }
        private void LobbyDetailsText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.lobbyDetails = LobbyDetailsText.Text;
            UpdateDiscordRPCResult(true);
        }

        private void LobbyStateText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.lobbyState = LobbyStateText.Text;
            UpdateDiscordRPCResult(true);
        }
        private void UpdateDiscordRPCResult(bool lobby)
        {
            if (DiscordRpcDetails == null || DiscordRpcState == null)
                return;
            if (lobby) 
            {
                DiscordRpcDetails.Text = main.LimitLength(main.FormatString(Settings.Default.lobbyDetails, GameState),128);
                DiscordRpcState.Text = main.LimitLength(main.FormatString(Settings.Default.lobbyState, GameState),128);
            }
            else
            {
                DiscordRpcDetails.Text = main.LimitLength(main.FormatString(Settings.Default.inGameDetails, GameState),128);
                DiscordRpcState.Text = main.LimitLength(main.FormatString(Settings.Default.inGameState, GameState),128);
            }
        }

        private void DiscordTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDiscordRPCResult(DiscordTabControl.SelectedIndex == 0);
        }

        private void TelegramTestMessage_Click(object sender, RoutedEventArgs e)
        {
            Telegram.SendMessage("Test Message!",Settings.Default.telegramChatId, 
                Telegram.CheckToken(Settings.Default.customTelegramToken) ? 
                Settings.Default.customTelegramToken : Encoding.UTF8.GetString(Convert.FromBase64String(APIKeys.TELEGRAM_BOT_TOKEN + "==")));
        }

        private void RemoveDiscordButton_Click(object sender, RoutedEventArgs e)
        {
            if(DiscordRPCButtonsListView.SelectedIndex != -1)
            {
                main.discordRPCButtons.Remove(main.discordRPCButtons[DiscordRPCButtonsListView.SelectedIndex]);
                LoadDiscordButtons();
                DiscordRPCButtonSerializer.Serialize(main.discordRPCButtons);

                if (main.current.IsPortable)
                    File.WriteAllText(Log.WorkPath + "\\.conf", main.current.settings.ToString(), Encoding.UTF8);
            }
        }

        private async void AddDiscordButton_Click(object sender, RoutedEventArgs e)
        {
            if(main.discordRPCButtons.Count == 1)
            {
                await ShowMessage("title_error", "error_max1discord", MessageDialogStyle.Affirmative);
                return;
            }
            string label = await CallInputDialogAsync("inputtext_label","inputtext_enterlabel");
            string url = await CallInputDialogAsync("inputtext_url"
                , "inputtext_enterurl");
            if(label == null || url == null || label.Trim() == "" || url.Trim() == "" || !Uri.IsWellFormedUriString(url,UriKind.Absolute))
            {
                await ShowMessage("title_error", "error_entervalid", MessageDialogStyle.Affirmative);
                return;
            }
            DiscordRPCButton res = new DiscordRPCButton() { Label = label, Url = url };
            main.discordRPCButtons.Add(res);
            LoadDiscordButtons();
            DiscordRPCButtonSerializer.Serialize(main.discordRPCButtons);

            if (main.current.IsPortable)
                File.WriteAllText(Log.WorkPath + "\\.conf", main.current.settings.ToString(), Encoding.UTF8);
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
            string label = await CallInputDialogAsync("inputtext_label"
                , "inputtext_enterlabel");
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

            if (main.current.IsPortable)
                File.WriteAllText(Log.WorkPath + "\\.conf", main.current.settings.ToString(), Encoding.UTF8);
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
                double x_ratio = size.Width / main.current.buyMenu.Size.Width;
                double y_ratio = size.Height / main.current.buyMenu.Size.Height;
                BuyItem item = main.current.buyMenu.GetItem(new Point(pos.X / x_ratio, pos.Y / y_ratio), isCt);
                if (item != null)
                {
                    selectedItem = item;
                    SelectedCustomItemPropery.Visibility = Visibility.Hidden;
                    BuyItemProperties.Visibility = Visibility.Visible;
                    AutoBuyImage.Visibility = Visibility.Hidden;
                    AutoBuyTab.Visibility = Visibility.Hidden;
                    AutoBuySettingsStack.Visibility = Visibility.Hidden;
                    BuyItemName.Text = Languages.Strings.ResourceManager.GetString($"buyitem_{item.Name.ToString().ToLower()}");
                    BuyItemPriority.Value = item.GetPriority();
                    BuyItemEnabledCheckBox.IsChecked = item.IsEnabled();
                    CheckIsCustom(item);
                }  
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
                AutoBuySettingsStack.Visibility = Visibility.Visible;
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
            if (main.current.IsPortable)
                File.WriteAllText(Log.WorkPath + "\\.conf", main.current.settings.ToString(), Encoding.UTF8);
        }

        private void UpdateImage()
        {
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

        private void DebugBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            debugBox.ScrollToEnd();
        }
        private void DebugBox_Loaded(object sender, RoutedEventArgs e)
        {
            DebugBox_TextChanged(sender, null);
        }

        private void DebugSettings_Click(object sender, RoutedEventArgs e)
        {
            new DebugSettings(main).Show();
        }

        private void DebugInfo_Click(object sender, RoutedEventArgs e)
        {
            new DebugInfo(main).Show();
        }

        private void AboutLicense_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/MurkyYT/CSAuto/blob/master/LICENSE");
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            if (!main.current.IsWindows11)
                return;

            IntPtr hWnd = new WindowInteropHelper(this).EnsureHandle();
            var attribute = NativeMethods.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;

            var preference = WindowState == WindowState.Normal
                ? NativeMethods.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND
                : NativeMethods.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DONOTROUND;

            NativeMethods.DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
        }
    }
}
