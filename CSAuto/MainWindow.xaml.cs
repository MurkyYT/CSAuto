using Microsoft.Win32;
using NotifyIconLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Murky.Utils;
using Murky.Utils.CSGO;
using System.Net.Sockets;
using System.IO.Pipes;
using System.Security.Principal;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace CSAuto
{
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            var inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Constants
        /// </summary>
        const string VER = "1.1.0";
        const string DEBUG_REVISION = "1";
        const string PORT = "11523";
        const string TIMEOUT = "5.0";
        const string BUFFER = "0.1";
        const string THROTTLE = "0.0";
        const string HEARTBEAT = "10.0";
        const string INTEGRATION_FILE = "\"CSAuto Integration v" + VER + "," + DEBUG_REVISION + "\"\r\n{\r\n\"uri\" \"http://localhost:" + PORT +
            "\"\r\n\"timeout\" \"" + TIMEOUT + "\"\r\n\"" +
            "buffer\"  \"" + BUFFER + "\"\r\n\"" +
            "throttle\" \"" + THROTTLE + "\"\r\n\"" +
            "heartbeat\" \"" + HEARTBEAT + "\"\r\n\"data\"\r\n{\r\n   \"provider\"            \"1\"\r\n   \"map\"                 \"1\"\r\n   \"round\"               \"1\"\r\n   \"player_id\"           \"1\"\r\n   \"player_state\"        \"1\"\r\n   \"player_weapons\"      \"1\"\r\n   \"player_match_stats\"  \"1\"\r\n   \"bomb\" \"1\"\r\n}\r\n}";
        const string IN_LOBBY_STATE = "Chilling in lobby";
        const float ACCEPT_BUTTON_DELAY = 20;
        const int MAX_ARMOR_AMOUNT_TO_REBUY = 70;
        const int MIN_AMOUNT_OF_PIXELS_TO_ACCEPT = 5;
        const int BOMB_SECONDS_DELAY = 2;
        const int BOMB_SECONDS = 40 - BOMB_SECONDS_DELAY;
        const int BOMB_TIMER_DELAY = 950;
        readonly string[] AVAILABLE_MAP_ICONS;
        /// <summary>
        /// Publics
        /// </summary>
        public GSIDebugWindow debugWind = null;
        /// <summary>
        /// Readonly
        /// </summary>
        readonly NotifyIconWrapper notifyIcon = new NotifyIconWrapper();
        readonly ContextMenu exitcm = new ContextMenu();
        readonly System.Windows.Threading.DispatcherTimer appTimer = new System.Windows.Threading.DispatcherTimer();
        readonly Color BUTTON_COLOR = Color.FromArgb(76, 175, 80);
        readonly Color ACTIVE_BUTTON_COLOR = Color.FromArgb(90, 203, 94);
        readonly MenuItem startUpCheck = new MenuItem();
        readonly MenuItem saveFramesDebug = new MenuItem();
        readonly MenuItem autoAcceptMatchCheck = new MenuItem();
        readonly MenuItem autoReloadCheck = new MenuItem();
        readonly MenuItem autoBuyArmor = new MenuItem();
        readonly MenuItem autoBuyDefuseKit = new MenuItem();
        readonly MenuItem preferArmorCheck = new MenuItem();
        readonly MenuItem saveLogsCheck = new MenuItem();
        readonly MenuItem continueSprayingCheck = new MenuItem();
        readonly MenuItem autoCheckForUpdates = new MenuItem();
        readonly MenuItem autoPauseResumeSpotify = new MenuItem();
        readonly MenuItem enableDiscordRPC = new MenuItem();
        readonly MenuItem enableMobileApp = new MenuItem();
        readonly MenuItem acceptedNotification = new MenuItem();
        readonly MenuItem mapNotification = new MenuItem();
        readonly MenuItem lobbyNotification = new MenuItem();
        readonly MenuItem connectedNotification = new MenuItem();
        readonly MenuItem crashedNotification = new MenuItem();
        readonly MenuItem bombNotification = new MenuItem();
        readonly MenuItem enableLobbyCount = new MenuItem();
        readonly MenuItem autoBuyMenu = new MenuItem
        {
            Header = AppLanguage.Get("menu_autobuy")
        };
        readonly MenuItem discordMenu = new MenuItem
        {
            Header = AppLanguage.Get("menu_discord")
        };
        readonly MenuItem autoReloadMenu = new MenuItem
        {
            Header = AppLanguage.Get("menu_autoreload")
        };
        /// <summary>
        /// Privates
        /// </summary>
        private DiscordRpc.EventHandlers discordHandlers;
        private DiscordRpc.RichPresence discordPresence;
        private readonly AutoResetEvent _waitForConnection = new AutoResetEvent(false);
        private string integrationPath = null;
        private HttpListener _listener;
        private bool ServerRunning = false;
        /// <summary>
        /// Members
        /// </summary>
        Point csResolution = new Point();
        GameState GameState = new GameState(null);
        int frame = 0;
        bool csRunning = false;
        bool inGame = false;
        bool csActive = false;
        bool discordRPCON = false;
        Activity? lastActivity;
        Phase? matchState;
        Phase? roundState;
        BombState? bombState;
        Weapon weapon;
        bool acceptedGame = false;
        Process steamAPIServer = null;
        Process csProcess = null;
        Thread bombTimerThread = null;

        public ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                Application.Current.Exit += Current_Exit;
                AVAILABLE_MAP_ICONS = Properties.Resources.AVAILABLE_MAPS_STRING.Split(',');
                CSGOFriendCode.Encode("76561198341800115");
                InitializeDiscordRPC();
                CheckForDuplicates();
                //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                InitializeContextMenu();
                Top = -1000;
                Left = -1000;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{AppLanguage.Get("error_startup1")}\n'{ex.Message}'\n{AppLanguage.Get("error_startup2")}", AppLanguage.Get("title_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            Exited();
        }

        private void InitializeContextMenu()
        {
            MenuItem debugMenu = new MenuItem
            {
                Header = AppLanguage.Get("menu_debug")
            };
            MenuItem mobileNotificationsMenu = new MenuItem
            {
                Header = AppLanguage.Get("menu_notifications")
            };
            MenuItem languageMenu = new MenuItem
            {
                Header = AppLanguage.Get("menu_language")
            };
            GenerateLanguages(languageMenu);
            MenuItem mobileMenu = new MenuItem
            {
                Header = AppLanguage.Get("menu_mobile")
            };
            MenuItem exit = new MenuItem
            {
                Header = AppLanguage.Get("menu_exit")
            };
            MenuItem automation = new MenuItem
            {
                Header = AppLanguage.Get("menu_automation")
            };
            MenuItem options = new MenuItem
            {
                Header = AppLanguage.Get("menu_options")
            };
            exit.Click += Exit_Click;
            MenuItem about = new MenuItem
            {
                Header = $"{typeof(MainWindow).Namespace} - {VER}{(DEBUG_REVISION == "" ? "" : $" REV {DEBUG_REVISION}")}",
                IsEnabled = false,
                Icon = new System.Windows.Controls.Image
                {
                    Source = ToImageSource(System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location))
                }
            };
            MenuItem openDebugWindow = new MenuItem
            {
                Header = AppLanguage.Get("menu_opendebug")
            };
            openDebugWindow.Click += OpenDebugWindow_Click;
            MenuItem checkForUpdates = new MenuItem
            {
                Header = AppLanguage.Get("menu_checkforupdates")
            };
            checkForUpdates.Click += CheckForUpdates_Click;
            MenuItem enterMobileIpAddress = new MenuItem
            {
                Header = AppLanguage.Get("menu_enterip")
            };
            enterMobileIpAddress.Click += EnterMobileIpAddress_Click;
            MenuItem enterSteamAPIKey = new MenuItem
            {
                Header = AppLanguage.Get("menu_entersteamkey")
            };
            enterSteamAPIKey.Click += EnterSteamAPIKey_Click;
            bombNotification.IsChecked = Properties.Settings.Default.bombNotification;
            bombNotification.Header = AppLanguage.Get("menu_bombnotification");
            bombNotification.IsCheckable = true;
            bombNotification.Click += BombNotification_Click;
            enableLobbyCount.IsChecked = Properties.Settings.Default.enableLobbyCount;
            enableLobbyCount.Header = AppLanguage.Get("menu_lobbycount");
            enableLobbyCount.IsCheckable = true;
            enableLobbyCount.Click += EnableLobbyCount_Click;
            acceptedNotification.IsChecked = Properties.Settings.Default.acceptedNotification;
            acceptedNotification.Header = AppLanguage.Get("menu_acceptednotification");
            acceptedNotification.IsCheckable = true;
            acceptedNotification.Click += AcceptedNotification_Click;
            mapNotification.IsChecked = Properties.Settings.Default.mapNotification;
            mapNotification.Header = AppLanguage.Get("menu_mapnotification");
            mapNotification.IsCheckable = true;
            mapNotification.Click += MapNotification_Click;
            lobbyNotification.IsChecked = Properties.Settings.Default.lobbyNotification;
            lobbyNotification.Header = AppLanguage.Get("menu_lobbynotification");
            lobbyNotification.IsCheckable = true;
            lobbyNotification.Click += LobbyNotification_Click;
            connectedNotification.IsChecked = Properties.Settings.Default.connectedNotification;
            connectedNotification.Header = AppLanguage.Get("menu_connectednotification");
            connectedNotification.IsCheckable = true;
            connectedNotification.Click += ConnectedNotification_Click;
            crashedNotification.IsChecked = Properties.Settings.Default.crashedNotification;
            crashedNotification.Header = AppLanguage.Get("menu_crashednotification");
            crashedNotification.IsCheckable = true;
            crashedNotification.Click += CrashedNotification_Click;
            enableDiscordRPC.IsChecked = Properties.Settings.Default.enableDiscordRPC;
            enableDiscordRPC.Header = AppLanguage.Get("menu_discordrpc");
            enableDiscordRPC.IsCheckable = true;
            enableDiscordRPC.Click += EnableDiscordRPC_Click;
            enableMobileApp.IsChecked = Properties.Settings.Default.mobileAppEnabled;
            enableMobileApp.Header = AppLanguage.Get("menu_enabled");
            enableMobileApp.IsCheckable = true;
            enableMobileApp.Click += EnableMobileApp_Click;
            autoCheckForUpdates.IsChecked = Properties.Settings.Default.autoCheckForUpdates;
            autoCheckForUpdates.Header = AppLanguage.Get("menu_autocheckupdates");
            autoCheckForUpdates.IsCheckable = true;
            autoCheckForUpdates.Click += AutoCheckForUpdates_Click;
            autoPauseResumeSpotify.IsChecked = Properties.Settings.Default.autoPausePlaySpotify;
            autoPauseResumeSpotify.Header = AppLanguage.Get("menu_autospotify");
            autoPauseResumeSpotify.IsCheckable = true;
            autoPauseResumeSpotify.Click += AutoPauseResumeSpotify_Click;
            startUpCheck.IsChecked = Properties.Settings.Default.runAtStartUp;
            startUpCheck.Header = AppLanguage.Get("menu_startup");
            startUpCheck.IsCheckable = true;
            startUpCheck.Click += StartUpCheck_Click;
            continueSprayingCheck.IsChecked = Properties.Settings.Default.ContinueSpraying;
            continueSprayingCheck.Header = AppLanguage.Get("menu_continuespray");
            continueSprayingCheck.IsCheckable = true;
            continueSprayingCheck.Click += ContinueSprayingCheck_Click;
            saveFramesDebug.IsChecked = Properties.Settings.Default.saveDebugFrames;
            saveFramesDebug.Header = AppLanguage.Get("menu_savedebugframes");
            saveFramesDebug.IsCheckable = true;
            saveFramesDebug.Click += DebugCheck_Click;
            saveLogsCheck.IsChecked = Properties.Settings.Default.saveLogs;
            saveLogsCheck.Header = AppLanguage.Get("menu_savedebuglogs");
            saveLogsCheck.IsCheckable = true;
            saveLogsCheck.Click += SaveLogsCheck_Click;
            autoAcceptMatchCheck.IsChecked = Properties.Settings.Default.autoAcceptMatch;
            autoAcceptMatchCheck.Header = AppLanguage.Get("menu_autoaccept");
            autoAcceptMatchCheck.IsCheckable = true;
            autoAcceptMatchCheck.Click += AutoAcceptMatchCheck_Click;
            autoBuyArmor.IsChecked = Properties.Settings.Default.autoBuyArmor;
            autoBuyArmor.Header = AppLanguage.Get("menu_autobuyarmor");
            autoBuyArmor.IsCheckable = true;
            autoBuyArmor.Click += AutoBuyArmor_Click;
            autoBuyDefuseKit.IsChecked = Properties.Settings.Default.autoBuyDefuseKit;
            autoBuyDefuseKit.Header = AppLanguage.Get("menu_autobuydefuse");
            autoBuyDefuseKit.IsCheckable = true;
            autoBuyDefuseKit.Click += AutoBuyDefuseKit_Click;
            preferArmorCheck.IsChecked = Properties.Settings.Default.preferArmor;
            preferArmorCheck.Header = AppLanguage.Get("menu_preferarmor");
            preferArmorCheck.IsCheckable = true;
            preferArmorCheck.Click += PreferArmorCheck_Click;
            autoReloadCheck.IsChecked = Properties.Settings.Default.autoReload;
            autoReloadCheck.Header = AppLanguage.Get("menu_enabled");
            autoReloadCheck.IsCheckable = true;
            autoReloadCheck.Click += AutoReloadCheck_Click;
            mobileNotificationsMenu.Items.Add(acceptedNotification);
            mobileNotificationsMenu.Items.Add(mapNotification);
            mobileNotificationsMenu.Items.Add(lobbyNotification);
            mobileNotificationsMenu.Items.Add(connectedNotification);
            mobileNotificationsMenu.Items.Add(crashedNotification);
            mobileNotificationsMenu.Items.Add(bombNotification);
            debugMenu.Items.Add(saveFramesDebug);
            debugMenu.Items.Add(saveLogsCheck);
            debugMenu.Items.Add(openDebugWindow);
            autoReloadMenu.Items.Add(autoReloadCheck);
            autoReloadMenu.Items.Add(continueSprayingCheck);
            autoBuyMenu.Items.Add(preferArmorCheck);
            autoBuyMenu.Items.Add(autoBuyArmor);
            autoBuyMenu.Items.Add(autoBuyDefuseKit);
            automation.Items.Add(autoBuyMenu);
            automation.Items.Add(autoReloadMenu);
            automation.Items.Add(autoAcceptMatchCheck);
            automation.Items.Add(autoPauseResumeSpotify);
            options.Items.Add(startUpCheck);
            options.Items.Add(autoCheckForUpdates);
            options.Items.Add(languageMenu);
            discordMenu.Items.Add(enableDiscordRPC);
            discordMenu.Items.Add(enableLobbyCount);
            discordMenu.Items.Add(enterSteamAPIKey);
            mobileMenu.Items.Add(enableMobileApp);
            mobileMenu.Items.Add(enterMobileIpAddress);
            mobileMenu.Items.Add(mobileNotificationsMenu);
            exitcm.Items.Add(about);
            exitcm.Items.Add(debugMenu);
            exitcm.Items.Add(new Separator());
            exitcm.Items.Add(mobileMenu);
            exitcm.Items.Add(discordMenu);
            exitcm.Items.Add(automation);
            exitcm.Items.Add(options);
            exitcm.Items.Add(new Separator());
            exitcm.Items.Add(checkForUpdates);
            exitcm.Items.Add(exit);
            exitcm.StaysOpen = false;
        }

        private void EnableLobbyCount_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.enableLobbyCount = enableLobbyCount.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void EnterSteamAPIKey_Click(object sender, RoutedEventArgs e)
        {
            string res = "";
            if (InputBox.Show(AppLanguage.Get("inputtitle_steamkey"), AppLanguage.Get("inputtext_steamkey"), ref res) == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.steamAPIkey = res;
                Properties.Settings.Default.Save();
            }
        }

        private void BombNotification_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.bombNotification = bombNotification.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void CrashedNotification_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.crashedNotification = crashedNotification.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void ConnectedNotification_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.connectedNotification = connectedNotification.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void LobbyNotification_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.lobbyNotification = lobbyNotification.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void MapNotification_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.mapNotification = mapNotification.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void AcceptedNotification_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.acceptedNotification = acceptedNotification.IsChecked;
            Properties.Settings.Default.Save();
        }

        private static void GenerateLanguages(MenuItem languageMenu)
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
                languageMenu.Items.Add(rb);
            }
        }

        private void EnterMobileIpAddress_Click(object sender, RoutedEventArgs e)
        {
            string res = "";
            if (InputBox.Show(AppLanguage.Get("inputtitle_mobileip"), AppLanguage.Get("inputtext_mobileip"), ref res) == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.phoneIpAddress = res;
                Properties.Settings.Default.Save();
                if (Properties.Settings.Default.connectedNotification)
                    SendMessageToServer($"<CNT>{AppLanguage.Get("server_computer")} {Environment.MachineName} ({GetLocalIPAddress()}) {AppLanguage.Get("server_online")}");
            }
        }

        private void EnableMobileApp_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.mobileAppEnabled = enableMobileApp.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void EnableDiscordRPC_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.enableDiscordRPC = enableDiscordRPC.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void InitializeDiscordRPC()
        {
            discordHandlers = default;
            //if (Properties.Settings.Default.enableDiscordRPC)
            //{
            //    DiscordRpc.Initialize("1121012657126916157", ref discordHandlers, true, "730");
            //    Log.WriteLine("DiscordRpc.Initialize();");
            //    discordRPCON = true;
            //}
        }

        private void AutoCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.autoCheckForUpdates = autoCheckForUpdates.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void OpenDebugWindow_Click(object sender, RoutedEventArgs e)
        {
            Notifyicon_LeftMouseButtonDoubleClick(null, null);
        }

        private void AutoPauseResumeSpotify_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.autoPausePlaySpotify = autoPauseResumeSpotify.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void ContinueSprayingCheck_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ContinueSpraying = continueSprayingCheck.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            CheckForUpdates();
        }
        void CheckForUpdates()
        {
            new Thread(() =>
            {
                try
                {
                    Log.WriteLine("Checking for updates");
                    string latestVersion = Github.GetLatestStringTag("murkyyt", "csauto");
                    //string latestVersion = (await Github.GetLatestTagAsyncBySemver("MurkyYT", "CSAuto")).Name;
                    //string webInfo = await client.DownloadStringTaskAsync("https://api.github.com/repos/MurkyYT/CSAuto/tags");
                    //string latestVersion = webInfo.Split(new string[] { "{\"name\":\"" }, StringSplitOptions.None)[1].Split('"')[0].Trim();
                    Log.WriteLine($"The latest version is {latestVersion}");
                    if (latestVersion == VER)
                    {
                        Log.WriteLine("Latest version installed");
                        MessageBox.Show(AppLanguage.Get("msgbox_latestversion"), AppLanguage.Get("title_update"), MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        Log.WriteLine($"Newer version found {VER} --> {latestVersion}");
                        MessageBoxResult result = MessageBox.Show($"{AppLanguage.Get("msgbox_newerversion1")} ({latestVersion}) {AppLanguage.Get("msgbox_newerversion2")}", AppLanguage.Get("title_update"), MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                        {
                            Log.WriteLine("Downloading latest version");
                            Process.Start("https://github.com/MurkyYT/CSAuto/releases/latest");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"Couldn't check for updates - '{ex.Message}'");
                    MessageBox.Show($"{AppLanguage.Get("error_update")}\n'{ex.Message}'", AppLanguage.Get("title_update"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }).Start();
        }

        private void SaveLogsCheck_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.saveLogs = saveLogsCheck.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void PreferArmorCheck_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.preferArmor = preferArmorCheck.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void AutoBuyDefuseKit_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.autoBuyDefuseKit = autoBuyDefuseKit.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void AutoBuyArmor_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.autoBuyArmor = autoBuyArmor.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void AutoReloadCheck_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.autoReload = autoReloadCheck.IsChecked;
            Properties.Settings.Default.Save();
        }
        public bool StartGSIServer()
        {
            if (ServerRunning)
                return false;

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:" + PORT + "/");
            Thread ListenerThread = new Thread(new ThreadStart(Run));
            try
            {
                _listener.Start();
            }
            catch (HttpListenerException)
            {
                return false;
            }
            ServerRunning = true;
            ListenerThread.Start();
            return true;
        }

        /// <summary>
        /// Stops listening for HTTP POST requests
        /// </summary>
        public void StopGSIServer()
        {
            if (!ServerRunning)
                return;
            ServerRunning = false;
            _listener.Close();
            (_listener as IDisposable).Dispose();
        }

        private void Run()
        {
            while (ServerRunning)
            {
                _listener.BeginGetContext(ReceiveGameState, _listener);
                _waitForConnection.WaitOne();
                _waitForConnection.Reset();
            }
            try
            {
                _listener.Stop();
            }
            catch (ObjectDisposedException)
            { /* _listener was already disposed, do nothing */ }
        }

        private void ReceiveGameState(IAsyncResult result)
        {
            HttpListenerContext context;
            try
            {
                context = _listener.EndGetContext(result);
            }
            catch (ObjectDisposedException)
            {
                // Listener was Closed due to call of Stop();
                return;
            }
            catch (HttpListenerException)
            {
                return;
            }
            finally
            {
                _waitForConnection.Set();
            }
            try
            {
                HttpListenerRequest request = context.Request;
                string JSON;

                using (Stream inputStream = request.InputStream)
                {
                    using (StreamReader sr = new StreamReader(inputStream))
                    {
                        JSON = sr.ReadToEnd();
                    }
                }
                using (HttpListenerResponse response = context.Response)
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";
                    response.Close();
                }
                GameState = new GameState(JSON);
                Activity? activity = GameState.Player.CurrentActivity;
                Phase? currentMatchState = GameState.Match.Phase;
                Phase? currentRoundState = GameState.Round.Phase;
                BombState? currentBombState = GameState.Round.Bombstate;
                Weapon currentWeapon = GameState.Player.ActiveWeapon;
                if (debugWind != null)
                    debugWind.UpdateText(JSON);
                //if (lastActivity != activity)
                //    Log.WriteLine($"Activity: {(lastActivity == null ? "None" : lastActivity.ToString())} -> {(activity == null ? "None" : activity.ToString())}");
                //if (currentMatchState != matchState)
                //    Log.WriteLine($"Match State: {(matchState == null ? "None" : matchState.ToString())} -> {(currentMatchState == null ? "None" : currentMatchState.ToString())}");
                //if (currentRoundState != roundState)
                //    Log.WriteLine($"Round State: {(roundState == null ? "None" : roundState.ToString())} -> {(currentRoundState == null ? "None" : currentRoundState.ToString())}");
                //if (round != currentRound)
                //    Log.WriteLine($"RoundNo: {(round == -1 ? "None" : round.ToString())} -> {(currentRound == -1 ? "None" : currentRound.ToString())}");
                //if (GetWeaponName(weapon) != GetWeaponName(currentWeapon))
                //    Log.WriteLine($"Current Weapon: {(weapon == null ? "None" : GetWeaponName(weapon))} -> {(currentWeapon == null ? "None" : GetWeaponName(currentWeapon))}");
                if (bombState == null && currentBombState == BombState.Planted && bombTimerThread == null)
                {
                    StartBombTimer();
                }
                if (bombState == BombState.Planted && currentBombState != BombState.Planted)
                {
                    if (bombTimerThread != null)
                        bombTimerThread.Abort();
                    bombTimerThread = null;
                    switch (currentBombState)
                    {
                        case BombState.Defused:
                            SendMessageToServer($"<BMB>{AppLanguage.Get("server_bombdefuse")}");
                            break;
                        case BombState.Exploded:
                            SendMessageToServer($"<BMB>{AppLanguage.Get("server_bombexplode")}");
                            break;
                    }

                }
                if (GameState.Match.Map != null && (discordPresence.state == IN_LOBBY_STATE || discordPresence.startTimestamp == 0))
                {
                    Log.WriteLine($"Player loaded on map {GameState.Match.Map} in mode {GameState.Match.Mode}");
                    discordPresence.startTimestamp = GameState.Timestamp;
                    discordPresence.details = $"{GameState.Match.Mode} - {GameState.Match.Map}";
                    discordPresence.largeImageKey = AVAILABLE_MAP_ICONS.Contains(GameState.Match.Map) ? $"map_icon_{GameState.Match.Map}" : "csgo_icon";
                    discordPresence.largeImageText = GameState.Match.Map;
                    if (Properties.Settings.Default.mapNotification)
                        SendMessageToServer($"<MAP>{AppLanguage.Get("server_loadedmap")} {GameState.Match.Map} {AppLanguage.Get("server_mode")} {GameState.Match.Mode}");
                }
                else if (GameState.Match.Map == null && discordPresence.state != IN_LOBBY_STATE)
                {
                    Log.WriteLine($"Player is back in main menu");
                    discordPresence.startTimestamp = GameState.Timestamp;
                    discordPresence.details = $"FriendCode: {CSGOFriendCode.Encode(GameState.MySteamID)}";
                    discordPresence.state = IN_LOBBY_STATE;
                    discordPresence.largeImageKey = "csgo_icon";
                    discordPresence.largeImageText = "Menu";
                    discordPresence.smallImageKey = null;
                    discordPresence.smallImageText = null;
                    if (Properties.Settings.Default.lobbyNotification)
                        SendMessageToServer($"<LBY>{AppLanguage.Get("server_loadedlobby")}");
                }
                lastActivity = activity;
                matchState = currentMatchState;
                roundState = currentRoundState;
                weapon = currentWeapon;
                bombState = currentBombState;
                inGame = GameState.Match.Map != null;
                if (csActive && !GameState.IsSpectating)
                {
                    if (Properties.Settings.Default.autoReload && lastActivity != Activity.Menu)
                    {
                        TryToAutoReload();
                    }
                    if (Properties.Settings.Default.preferArmor)
                    {
                        AutoBuyArmor();
                        AutoBuyDefuseKit();
                    }
                    else
                    {
                        AutoBuyDefuseKit();
                        AutoBuyArmor();
                    }
                    if (Properties.Settings.Default.autoPausePlaySpotify)
                    {
                        AutoPauseResumeSpotify();
                    }
                }
                UpdateDiscordRPC();
                SendMessageToServer($"<GSI>{JSON}{inGame}");
                //Log.WriteLine($"Got info from GSI\nActivity:{activity}\nCSGOActive:{csgoActive}\nInGame:{inGame}\nIsSpectator:{IsSpectating(JSON)}");

            }
            catch (Exception ex)
            {
                Log.WriteLine("Error happend while getting GSI Info\n" + ex);
            }
        }
        private void StartBombTimer()
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long ms = (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
            long result = ms / 1000;
            int diff = (int)(GameState.Timestamp - result);
            bombTimerThread = new Thread(() =>
            {
                for (int seconds = BOMB_SECONDS - diff; seconds >= 0; seconds--)
                {
                    SendMessageToServer($"<BMB>{AppLanguage.Get("server_timeleft")} {seconds}");
                    Thread.Sleep(BOMB_TIMER_DELAY);
                }
                bombTimerThread = null;
            });
            bombTimerThread.Start();
        }

        private void UpdateDiscordRPC()
        {
            if (!discordRPCON && Properties.Settings.Default.enableDiscordRPC)
            {
                DiscordRpc.Initialize("1121012657126916157", ref discordHandlers, true, "730");
                Log.WriteLine("DiscordRpc.Initialize();");
                discordRPCON = true;
            }
            else if (discordRPCON && !Properties.Settings.Default.enableDiscordRPC)
            {
                DiscordRpc.Shutdown();
                Log.WriteLine("DiscordRpc.Shutdown();");
                discordRPCON = false;
            }
            if (csRunning && inGame)
            {
                string phase = GameState.Match.Phase == Phase.Warmup ? "Warmup" : GameState.Round.Phase.ToString();
                discordPresence.state = GameState.Player.Team == Team.T ?
                    $"{GameState.Match.TScore} [T] ({phase}) {GameState.Match.CTScore} [CT]" :
                    $"{GameState.Match.CTScore} [CT] ({phase}) {GameState.Match.TScore} [T]";
                discordPresence.smallImageKey = GameState.IsSpectating ? "gotv_icon" : GameState.IsDead ? "spectator" : GameState.Player.Team.ToString().ToLower();
                discordPresence.smallImageText = GameState.IsSpectating ? "Watching GOTV" : GameState.IsDead ? "Spectating" : GameState.Player.Team == Team.T ? "Terrorist" : "Counter-Terrorist";
                discordPresence.partyMax = 0;
                discordPresence.partyId = null;
                discordPresence.partySize = 0;
            }
            else if (csRunning && !inGame)
            {
                if (Properties.Settings.Default.enableLobbyCount)
                {
                    string steamworksRes = GetLobbyInfoFromSteamworks();
                    string lobbyid = steamworksRes.Split('(')[1].Split(')')[0];
                    string partyMax = steamworksRes.Split('/')[1].Split('(')[0];
                    string partysize = steamworksRes.Split('/')[0];
                    discordPresence.partyMax = int.Parse(partyMax);
                    discordPresence.partyId = lobbyid == "0" ? null : lobbyid;
                    discordPresence.partySize = int.Parse(partysize);
                }
            }
            else if (discordRPCON && !csRunning)
            {
                DiscordRpc.Shutdown();
                discordRPCON = false;
                Log.WriteLine("DiscordRpc.Shutdown();");
            }
            DiscordRpc.UpdatePresence(ref discordPresence);
        }

        private void AutoPauseResumeSpotify()
        {
            if (GameState.Player.CurrentActivity == Activity.Playing)
            {
                if (GameState.Player.Health > 0 && GameState.Player.SteamID == GameState.MySteamID && Spotify.IsPlaying())
                {
                    Spotify.Pause();
                }
                else if (!Spotify.IsPlaying() && GameState.Player.SteamID != GameState.MySteamID)
                {
                    Spotify.Resume();
                }
            }
            else if (!Spotify.IsPlaying() && GameState.Player.CurrentActivity != Activity.Textinput)
            {
                Spotify.Resume();
            }

        }
        private void SendMessageToServer(string message)
        {
            if (Properties.Settings.Default.phoneIpAddress == "" || !Properties.Settings.Default.mobileAppEnabled)
                return;
            new Thread(() =>
            {
                try // Try connecting and send the message bytes  
                {
                    TcpClient client = new TcpClient(Properties.Settings.Default.phoneIpAddress, 11_000); // Create a new connection  
                    NetworkStream stream = client.GetStream();
                    byte[] messageBytes = Encoding.UTF8.GetBytes($"{message}<|EOM|>");
                    stream.Write(messageBytes, 0, messageBytes.Length); // Write the bytes  
                                                                        // Clean up  
                    stream.Dispose();
                    client.Close();
                }
                catch { }
            }).Start();
        }
        private void TimerCallback(object sender, EventArgs e)
        {
            try
            {
                Process[] prcs = Process.GetProcessesByName("csgo");
                if (csProcess == null && prcs.Length > 0)
                {
                    csProcess = prcs[0];
                    csRunning = true;
                    csProcess.Exited += CsProcess_Exited;
                    csProcess.EnableRaisingEvents = true;
                    if (!ServerRunning)
                    {
                        Log.WriteLine("Starting GSI Server");
                        StartGSIServer();
                    }
                    if (steamAPIServer == null && Properties.Settings.Default.enableLobbyCount)
                    {
                        steamAPIServer = new Process() { StartInfo = { FileName = "steamapi.exe" } };
                        steamAPIServer.Start();
                    }
                }
                else if (!csRunning)
                {
                    if (discordRPCON)
                    {
                        DiscordRpc.Shutdown();
                        Log.WriteLine("DiscordRpc.Shutdown();");
                        discordRPCON = false;
                        discordPresence = default;
                    }
                    if (GameState.Timestamp != 0)
                    {
                        GameState = new GameState(null);
                    }
                    if (ServerRunning)
                    {
                        Log.WriteLine("Stopping GSI Server");
                        StopGSIServer();
                        SendMessageToServer("<CLS>");
                    }
                    if (steamAPIServer != null)
                    {
                        steamAPIServer.Kill();
                        steamAPIServer = null;
                    }
                }
                csActive = NativeMethods.IsForegroundProcess(csProcess != null ? (uint)csProcess.Id : 0);
                if (csActive)
                {
                    csResolution = new Point(
                            (int)SystemParameters.PrimaryScreenWidth,
                            (int)SystemParameters.PrimaryScreenHeight);
                    if (Properties.Settings.Default.autoAcceptMatch && !inGame && !acceptedGame)
                        _ = AutoAcceptMatchAsync();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"{ex}");
            }
            GC.Collect();
        }

        private string GetLobbyInfoFromSteamworks()
        {
            string res = "0/0(0)";
            if (Properties.Settings.Default.steamAPIkey == "" || Properties.Settings.Default.steamAPIkey == null)
                return res;
            var pipeClient =
                    new NamedPipeClientStream(".", "csautopipe",
                        PipeDirection.InOut, PipeOptions.None,
                        TokenImpersonationLevel.Impersonation);
            pipeClient.Connect();

            var ss = new StreamString(pipeClient);
            // Validate the server's signature string.
            if (ss.ReadString() == "I am the one true server!")
            {
                // The client security token is sent with the first write.
                // Send the name of the file whose contents are returned
                // by the server.
                ss.WriteString(GetLobbyID());
                res = ss.ReadString();
            }
            else
            {
                Log.WriteLine("Server could not be verified.");
            }
            pipeClient.Close();
            return res;
        }

        private string GetLobbyID()
        {
            string KEY = Properties.Settings.Default.steamAPIkey;
            if (KEY == "" || KEY == null)
                return "0";
            string apiURL = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={KEY}&steamids={GameState.MySteamID}&appids=730";
            string webInfo = Github.GetWebInfo(apiURL);
            string[] split = webInfo.Split(new string[] { "\"lobbysteamid\":\"" }, StringSplitOptions.None);
            if (split.Length < 2)
                return "0";
            return split[1].Split('"')[0];
        }
        private void CsProcess_Exited(object sender, EventArgs e)
        {
            csRunning = false;
            Log.WriteLine($"CS Exit Code: {csProcess.ExitCode}");
            if (csProcess.ExitCode != 0 && Properties.Settings.Default.crashedNotification)
                SendMessageToServer($"<CRS>{AppLanguage.Get("server_gamecrash")}");
            csProcess = null;
        }

        private void AutoBuyArmor()
        {
            if (!Properties.Settings.Default.autoBuyArmor || lastActivity == Activity.Menu)
                return;
            int armor = GameState.Player.Armor;
            bool hasHelmet = GameState.Player.HasHelmet;
            int money = GameState.Player.Money;
            if ((matchState == Phase.Live
                && roundState == Phase.Freezetime)
                &&
                ((money >= 650 && armor <= MAX_ARMOR_AMOUNT_TO_REBUY) ||
                (money >= 350 && armor == 100 && !hasHelmet) ||
                (money >= 1000 && armor <= MAX_ARMOR_AMOUNT_TO_REBUY && !hasHelmet))
                )
            {
                DisableTextinput();
                Log.WriteLine("Auto buying armor");
                PressKey(Keyboard.DirectXKeyStrokes.DIK_B);
                Thread.Sleep(100);
                PressKeys(new Keyboard.DirectXKeyStrokes[]
                {
                    Keyboard.DirectXKeyStrokes.DIK_5,
                    Keyboard.DirectXKeyStrokes.DIK_1,
                    Keyboard.DirectXKeyStrokes.DIK_2,
                    Keyboard.DirectXKeyStrokes.DIK_B,
                    Keyboard.DirectXKeyStrokes.DIK_B
                });
            }
        }
        private void AutoBuyDefuseKit()
        {
            if (!Properties.Settings.Default.autoBuyDefuseKit || lastActivity == Activity.Menu)
                return;
            bool hasDefuseKit = GameState.Player.HasDefuseKit;
            int money = GameState.Player.Money;
            if (matchState == Phase.Live
                && roundState == Phase.Freezetime
                && money >= 400
                && !hasDefuseKit
                && GameState.Player.Team == Team.CT)
            {
                DisableTextinput();
                Log.WriteLine("Auto buying defuse kit");
                PressKey(Keyboard.DirectXKeyStrokes.DIK_B);
                Thread.Sleep(100);
                PressKeys(new Keyboard.DirectXKeyStrokes[]
                {
                    Keyboard.DirectXKeyStrokes.DIK_5,
                    Keyboard.DirectXKeyStrokes.DIK_4,
                    Keyboard.DirectXKeyStrokes.DIK_B,
                    Keyboard.DirectXKeyStrokes.DIK_B
                });
            }
        }
        private void DisableTextinput()
        {
            Activity? activity = GameState.Player.CurrentActivity;
            if (activity == Activity.Textinput)
            {
                PressKey(Keyboard.DirectXKeyStrokes.DIK_ESCAPE);
                Log.WriteLine("Disabling Textinput activity...");
            }
        }
        private void TryToAutoReload()
        {
            bool isMousePressed = Keyboard.GetKeyState(Keyboard.VirtualKeyStates.VK_LBUTTON);
            if (!isMousePressed || weapon == null)
                return;
            try
            {
                int bullets = weapon.Bullets;
                WeaponType? weaponType = weapon.Type;
                string weaponName = weapon.Name;
                if (bullets == 0)
                {
                    NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTUP,
                        System.Windows.Forms.Cursor.Position.X,
                        System.Windows.Forms.Cursor.Position.Y,
                        0, 0);
                    Log.WriteLine("Auto reloading");
                    if ((weaponType == WeaponType.Rifle
                        || weaponType == WeaponType.MachineGun
                        || weaponType == WeaponType.SubmachineGun
                        || weaponName == "weapon_cz75a")
                        && (weaponName != "weapon_sg556")
                        && Properties.Settings.Default.ContinueSpraying)
                    {
                        Thread.Sleep(100);
                        NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN,
                            System.Windows.Forms.Cursor.Position.X,
                            System.Windows.Forms.Cursor.Position.Y,
                            0, 0);
                        Log.WriteLine($"Continue spraying ({weaponName} - {weaponType})");
                    }

                }
            }
            catch { return; }
        }
        void PressKey(Keyboard.DirectXKeyStrokes key)
        {
            Keyboard.SendKey(key, false, Keyboard.InputType.Keyboard);
            Keyboard.SendKey(key, true, Keyboard.InputType.Keyboard);
        }
        void PressKeys(Keyboard.DirectXKeyStrokes[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                Keyboard.SendKey(keys[i], false, Keyboard.InputType.Keyboard);
                Keyboard.SendKey(keys[i], true, Keyboard.InputType.Keyboard);
            }
        }
        string GetSteamPath()
        {
            string X86 = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "InstallPath", null);
            string X64 = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", null);
            return X86 ?? X64;
        }
        // from - https://gist.github.com/moritzuehling/7f1c512871e193c0222f
        private string GetCSGODir()
        {
            string steamPath = GetSteamPath();
            if (steamPath == null)
                throw new Exception(AppLanguage.Get("exception_steamnotfound")/*"Couldn't find Steam Path"*/);
            string pathsFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

            if (!File.Exists(pathsFile))
                return null;

            List<string> libraries = new List<string>
            {
                Path.Combine(steamPath)
            };

            var pathVDF = File.ReadAllLines(pathsFile);
            // Okay, this is not a full vdf-parser, but it seems to work pretty much, since the 
            // vdf-grammar is pretty easy. Hopefully it never breaks. I'm too lazy to write a full vdf-parser though. 
            Regex pathRegex = new Regex(@"\""(([^\""]*):\\([^\""]*))\""");
            foreach (var line in pathVDF)
            {
                if (pathRegex.IsMatch(line))
                {
                    string match = pathRegex.Matches(line)[0].Groups[1].Value;

                    // De-Escape vdf. 
                    libraries.Add(match.Replace("\\\\", "\\"));
                }
            }

            foreach (var library in libraries)
            {
                string csgoPath = Path.Combine(library, "steamapps\\common\\Counter-Strike Global Offensive\\csgo");
                if (Directory.Exists(csgoPath))
                {
                    return csgoPath;
                }
            }

            return null;
        }
        private void AutoAcceptMatchCheck_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.autoAcceptMatch = autoAcceptMatchCheck.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void DebugCheck_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.saveDebugFrames = saveFramesDebug.IsChecked;
            Properties.Settings.Default.Save();
        }
        public static void LeftMouseClick(int xpos, int ypos)
        {
            NativeMethods.SetCursorPos(xpos, ypos);
            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
            Log.WriteLine($"Left clicked at X:{xpos} Y:{ypos}");
        }
        private void StartUpCheck_Click(object sender, RoutedEventArgs e)
        {
            string appname = Assembly.GetEntryAssembly().GetName().Name;
            string executablePath = Process.GetCurrentProcess().MainModule.FileName;
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if ((bool)startUpCheck.IsChecked)
                {
                    Properties.Settings.Default.runAtStartUp = true;
                    rk.SetValue(appname, executablePath);
                }
                else
                {
                    Properties.Settings.Default.runAtStartUp = false;
                    rk.DeleteValue(appname, false);
                }
            }
            Properties.Settings.Default.Save();
        }
        private async Task AutoAcceptMatchAsync()
        {
            using (Bitmap bitmap = new Bitmap(1, csResolution.Y))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(
                        csResolution.X / 2,
                        0),
                        Point.Empty,
                        new System.Drawing.Size(1, csResolution.Y));
                }
                if (Properties.Settings.Default.saveDebugFrames)
                {
                    Directory.CreateDirectory($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\DEBUG\\FRAMES");
                    bitmap.Save($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\DEBUG\\FRAMES\\Frame{frame++}.jpeg", ImageFormat.Jpeg);
                }
                bool found = false;
                int count = 0;
                for (int y = bitmap.Height - 1; y >= 0 && !found; y--)
                {
                    Color pixelColor = bitmap.GetPixel(0, y);
                    if (pixelColor == BUTTON_COLOR || pixelColor == ACTIVE_BUTTON_COLOR)
                    {

                        if (count >= MIN_AMOUNT_OF_PIXELS_TO_ACCEPT) /*
                                         * just in case the program finds the 0:20 timer tick
                                         * didnt happen for a while but can happen still
                                         * happend while trying to create a while loop to search for button
                                         */
                        {
                            var clickpoint = new Point(
                                csResolution.X / 2,
                                y);
                            int X = clickpoint.X;
                            int Y = clickpoint.Y;
                            Log.WriteLine($"Found accept button at X:{X} Y:{Y}", caller: "AutoAcceptMatch");
                            if (Properties.Settings.Default.acceptedNotification)
                                SendMessageToServer($"<ACP>{AppLanguage.Get("server_acceptmatch")}");
                            LeftMouseClick(X, Y);
                            found = true;
                            if (CheckIfAccepted(bitmap,Y))
                            {
                                acceptedGame = true;
                                acceptedGame = await MakeFalse(ACCEPT_BUTTON_DELAY);
                            }
                        }
                        count++;
                    }
                }
            }
        }
        private bool CheckIfAccepted(Bitmap bitmap,int maxY)
        {
            bool found = false;
            int count = 0;
            for (int y = bitmap.Height - 1; y >= maxY && !found; y--)
            {
                Color pixelColor = bitmap.GetPixel(0, y);
                if (pixelColor == BUTTON_COLOR || pixelColor == ACTIVE_BUTTON_COLOR)
                {

                    if (count >= MIN_AMOUNT_OF_PIXELS_TO_ACCEPT) /*
                                         * just in case the program finds the 0:20 timer tick
                                         * didnt happen for a while but can happen still
                                         * happend while trying to create a while loop to search for button
                                         */
                    {
                        return false;
                    }
                    count++;
                }
            }
            return true;
        }
        async Task<bool> MakeFalse(float afterSeconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(afterSeconds));
            return false;
        }
        private void Notifyicon_RightMouseButtonClick(object sender, NotifyIconLibrary.Events.MouseLocationEventArgs e)
        {
            exitcm.IsOpen = true;
            Activate();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Exited()
        {
            notifyIcon.Close();
            StopGSIServer();
            Application.Current.Shutdown();
            if (steamAPIServer != null && !steamAPIServer.HasExited)
                steamAPIServer.Kill();
        }
        private void CheckForDuplicates()
        {
            var currentProcess = Process.GetCurrentProcess();
            var duplicates = Process.GetProcessesByName(currentProcess.ProcessName).Where(o => o.Id != currentProcess.Id);
            if (duplicates.Count() > 0)
            {
                notifyIcon.Close();
                Application.Current.Shutdown();
                Log.WriteLine($"Shutting down, found another CSAuto process");
            }
        }
        string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception(AppLanguage.Get("exception_nonetworkadapter")/*"No network adapters with an IPv4 address in the system!"*/);
        }
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                Visibility = Visibility.Hidden;
                InitializeNotifyIcon();
                InitializeTimer();
                Log.WriteLine($"CSAuto v{VER}{(DEBUG_REVISION == "" ? "" : $" REV {DEBUG_REVISION}")} started");
                string csgoDir = GetCSGODir();
                if (csgoDir == null)
                    throw new Exception(AppLanguage.Get("exception_csgonotfound")/*"Couldn't find CS:GO directory"*/);
                integrationPath = csgoDir + "\\cfg\\gamestate_integration_csauto.cfg";
                InitializeGSIConfig();
                if (Properties.Settings.Default.autoCheckForUpdates)
                    AutoCheckUpdate();
                if (Properties.Settings.Default.connectedNotification)
                    SendMessageToServer($"<CNT>{AppLanguage.Get("server_computer")} {Environment.MachineName} ({GetLocalIPAddress()}) {AppLanguage.Get("server_online")}");
            }
            catch (Exception ex)
            {
                if (ex.Message == AppLanguage.Get("exception_steamnotfound")
                    || ex.Message == AppLanguage.Get("exception_csgonotfound"))
                {
                    autoReloadMenu.IsEnabled = false;
                    autoBuyMenu.IsEnabled = false;
                    autoPauseResumeSpotify.IsEnabled = false;
                    discordMenu.IsEnabled = false;
                }
                MessageBox.Show($"{ex.Message}", AppLanguage.Get("title_error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeTimer()
        {
            appTimer.Interval = TimeSpan.FromMilliseconds(1000);
            appTimer.Tick += new EventHandler(TimerCallback);
            appTimer.Start();
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon.Icon = Properties.Resources.main;
            notifyIcon.Tip = "CSAuto - CS:GO Automation";
            notifyIcon.ShowTip = true;
            notifyIcon.RightMouseButtonClick += Notifyicon_RightMouseButtonClick;
            notifyIcon.LeftMouseButtonDoubleClick += Notifyicon_LeftMouseButtonDoubleClick;
            notifyIcon.Update();
        }

        private void InitializeGSIConfig()
        {
            if (!File.Exists(integrationPath))
            {
                using (FileStream fs = File.Create(integrationPath))
                {
                    Byte[] title = new UTF8Encoding(true).GetBytes(INTEGRATION_FILE);
                    fs.Write(title, 0, title.Length);
                }
                Log.WriteLine("CSAuto was never launched, initializing 'gamestate_integration_csauto.cfg'");
            }
            else
            {
                string[] lines = File.ReadAllLines(integrationPath);
                string ver = lines[0].Split('v')[1].Split('"')[0].Trim();
                if (ver != VER + "," + DEBUG_REVISION)
                {
                    using (FileStream fs = File.Create(integrationPath))
                    {
                        byte[] title = new UTF8Encoding(true).GetBytes(INTEGRATION_FILE);
                        fs.Write(title, 0, title.Length);
                    }
                    Log.WriteLine("Different 'gamestate_integration_csauto.cfg' was found, installing correct 'gamestate_integration_csauto.cfg'");
                }
            }
        }
        void AutoCheckUpdate()
        {
            new Thread(() =>
            {
                try
                {
                    Log.WriteLine("Auto Checking for Updates");
                    string latestVersion = Github.GetLatestStringTag("murkyyt", "csauto");
                    //string latestVersion = (await Github.GetLatestTagAsyncBySemver("MurkyYT", "CSAuto")).Name;
                    //string webInfo = await client.DownloadStringTaskAsync("https://api.github.com/repos/MurkyYT/CSAuto/tags");
                    //string latestVersion = webInfo.Split(new string[] { "{\"name\":\"" }, StringSplitOptions.None)[1].Split('"')[0].Trim();
                    Log.WriteLine($"The latest version is {latestVersion}");
                    if (latestVersion == VER)
                    {
                        Log.WriteLine("Latest version installed");
                    }
                    else
                    {
                        Log.WriteLine($"Newer version found {VER} --> {latestVersion}");
                        MessageBoxResult result = MessageBox.Show($"{AppLanguage.Get("msgbox_newerversion1")} ({latestVersion}) {AppLanguage.Get("msgbox_newerversion2")}", AppLanguage.Get("title_update"), MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                        {
                            Log.WriteLine("Downloading latest version");
                            Process.Start("https://github.com/MurkyYT/CSAuto/releases/latest");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"Couldn't check for updates - '{ex.Message}'");
                }
            }).Start();
        }

        private void Notifyicon_LeftMouseButtonDoubleClick(object sender, NotifyIconLibrary.Events.MouseLocationEventArgs e)
        {
            //open debug menu
            if (debugWind == null)
            {
                debugWind = new GSIDebugWindow(this);
                Log.debugWind = debugWind;
                debugWind.Show();
            }
            else
            {
                debugWind.Activate();
            }
        }
    }
}