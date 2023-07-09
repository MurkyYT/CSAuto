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
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Murky.Utils;
using Murky.Utils.CSGO;
namespace CSAuto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Constants
        /// </summary>
        const string VER = "1.0.7";
        const string DEBUG_REVISION = "2";
        const string PORT = "11523";
        const string TIMEOUT = "5.0";
        const string BUFFER = "0.1";
        const string THROTTLE = "0.5";
        const string HEARTBEAT = "10.0";
        const string INTEGRATION_FILE = "\"CSAuto Integration v" + VER + "\"\r\n{\r\n\"uri\" \"http://localhost:" + PORT +
            "\"\r\n\"timeout\" \"" + TIMEOUT + "\"\r\n\"" +
            "buffer\"  \"" + BUFFER + "\"\r\n\"" +
            "throttle\" \"" + THROTTLE + "\"\r\n\"" +
            "heartbeat\" \"" + HEARTBEAT + "\"\r\n\"data\"\r\n{\r\n   \"provider\"            \"1\"\r\n   \"map\"                 \"1\"\r\n   \"round\"               \"1\"\r\n   \"player_id\"           \"1\"\r\n   \"player_state\"        \"1\"\r\n   \"player_weapons\"      \"1\"\r\n   \"player_match_stats\"  \"1\"\r\n   \"bomb\" \"1\"\r\n}\r\n}";
        const string IN_LOBBY_STATE = "Chilling in lobby";
        const float ACCEPT_BUTTON_DELAY = 20;
        const int MAX_ARMOR_AMOUNT_TO_REBUY = 70;
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
        readonly MenuItem autoBuyMenu = new MenuItem
        {
            Header = "Auto Buy"
        };
        readonly MenuItem discordMenu = new MenuItem
        {
            Header = "Discord"
        };
        readonly MenuItem autoReloadMenu = new MenuItem
        {
            Header = "Auto Reload"
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
        Point csgoResolution = new Point();
        GameState GameState = new GameState(null);
        int frame = 0;
        bool csgoRunning = false;
        bool inGame = false;
        bool csgoActive = false;
        bool discordRPCON = false;
        Activity? lastActivity;
        Phase? matchState;
        Phase? roundState;
        Weapon weapon;
        bool acceptedGame = false;
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
                AVAILABLE_MAP_ICONS = Properties.Resources.AVAILABLE_MAPS_STRING.Split(',');
                CSGOFriendCode.Encode("76561198341800115");
                InitializeDiscordRPC();
                CheckForDuplicates();
                //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                MenuItem debugMenu = new MenuItem
                {
                    Header = "Debug"
                };
                MenuItem exit = new MenuItem
                {
                    Header = "Exit"
                };
                MenuItem automation = new MenuItem
                {
                    Header = "Automation"
                };
                MenuItem options = new MenuItem
                {
                    Header = "Options"
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
                    Header = "Open Debug Window"
                };
                openDebugWindow.Click += OpenDebugWindow_Click;
                MenuItem checkForUpdates = new MenuItem
                {
                    Header = "Check for updates"
                };
                checkForUpdates.Click += CheckForUpdates_Click;
                enableDiscordRPC.IsChecked = Properties.Settings.Default.enableDiscordRPC;
                enableDiscordRPC.Header = "Discord RPC";
                enableDiscordRPC.IsCheckable = true;
                enableDiscordRPC.Click += EnableDiscordRPC_Click;
                autoCheckForUpdates.IsChecked = Properties.Settings.Default.autoCheckForUpdates;
                autoCheckForUpdates.Header = "Check For Updates On Startup";
                autoCheckForUpdates.IsCheckable = true;
                autoCheckForUpdates.Click += AutoCheckForUpdates_Click;
                autoPauseResumeSpotify.IsChecked = Properties.Settings.Default.autoPausePlaySpotify;
                autoPauseResumeSpotify.Header = "Auto Pause/Resume Spotify";
                autoPauseResumeSpotify.IsCheckable = true;
                autoPauseResumeSpotify.Click += AutoPauseResumeSpotify_Click;
                startUpCheck.IsChecked = Properties.Settings.Default.runAtStartUp;
                startUpCheck.Header = "Start With Windows";
                startUpCheck.IsCheckable = true;
                startUpCheck.Click += StartUpCheck_Click;
                continueSprayingCheck.IsChecked = Properties.Settings.Default.ContinueSpraying;
                continueSprayingCheck.Header = "Continue Spraying (Experimental)";
                continueSprayingCheck.IsCheckable = true;
                continueSprayingCheck.Click += ContinueSprayingCheck_Click;
                saveFramesDebug.IsChecked = Properties.Settings.Default.saveDebugFrames;
                saveFramesDebug.Header = "Save Frames";
                saveFramesDebug.IsCheckable = true;
                saveFramesDebug.Click += DebugCheck_Click;
                saveLogsCheck.IsChecked = Properties.Settings.Default.saveLogs;
                saveLogsCheck.Header = "Save Logs";
                saveLogsCheck.IsCheckable = true;
                saveLogsCheck.Click += SaveLogsCheck_Click;
                autoAcceptMatchCheck.IsChecked = Properties.Settings.Default.autoAcceptMatch;
                autoAcceptMatchCheck.Header = "Auto Accept Match";
                autoAcceptMatchCheck.IsCheckable = true;
                autoAcceptMatchCheck.Click += AutoAcceptMatchCheck_Click;
                autoBuyArmor.IsChecked = Properties.Settings.Default.autoBuyArmor;
                autoBuyArmor.Header = "Auto Buy Armor";
                autoBuyArmor.IsCheckable = true;
                autoBuyArmor.Click += AutoBuyArmor_Click;
                autoBuyDefuseKit.IsChecked = Properties.Settings.Default.autoBuyDefuseKit;
                autoBuyDefuseKit.Header = "Auto Buy Defuse Kit";
                autoBuyDefuseKit.IsCheckable = true;
                autoBuyDefuseKit.Click += AutoBuyDefuseKit_Click;
                preferArmorCheck.IsChecked = Properties.Settings.Default.preferArmor;
                preferArmorCheck.Header = "Prefer armor";
                preferArmorCheck.IsCheckable = true;
                preferArmorCheck.Click += PreferArmorCheck_Click;
                autoReloadCheck.IsChecked = Properties.Settings.Default.autoReload;
                autoReloadCheck.Header = "Enabled";
                autoReloadCheck.IsCheckable = true;
                autoReloadCheck.Click += AutoReloadCheck_Click;
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
                discordMenu.Items.Add(enableDiscordRPC);
                exitcm.Items.Add(about);
                exitcm.Items.Add(debugMenu);
                exitcm.Items.Add(new Separator());
                exitcm.Items.Add(discordMenu);
                exitcm.Items.Add(automation);
                exitcm.Items.Add(options);
                exitcm.Items.Add(new Separator());
                exitcm.Items.Add(checkForUpdates);
                exitcm.Items.Add(exit);
                Top = -1000;
                Left = -1000;
                exitcm.StaysOpen = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error ocurred\n'{ex.Message}'\nTry to download the latest version from github.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
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
        Task CheckForUpdates()
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
                    MessageBox.Show("You have the latest version!", "Check for updates (CSAuto)", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Log.WriteLine($"Newer version found {VER} --> {latestVersion}");
                    MessageBoxResult result = MessageBox.Show($"Found newer verison ({latestVersion}) would you like to download it?", "Check for updates (CSAuto)", MessageBoxButton.YesNo, MessageBoxImage.Information);
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
                MessageBox.Show($"Couldn't check for updates,Try again in an hour\n'{ex.Message}'", "Check for updates (CSAuto)", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Task.CompletedTask;
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
                if (GameState.Match.Map != null && (discordPresence.state == IN_LOBBY_STATE || discordPresence.startTimestamp == 0))
                {
                    Log.WriteLine($"Player loaded on map {GameState.Match.Map} in mode {GameState.Match.Mode}");
                    discordPresence.startTimestamp = GameState.Timestamp;
                    discordPresence.details = $"{GameState.Match.Mode} - {GameState.Match.Map}";
                    discordPresence.largeImageKey = AVAILABLE_MAP_ICONS.Contains(GameState.Match.Map) ? $"map_icon_{GameState.Match.Map}" : "csgo_icon";
                    discordPresence.largeImageText = GameState.Match.Map;
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
                }
                lastActivity = activity;
                matchState = currentMatchState;
                roundState = currentRoundState;
                weapon = currentWeapon;
                inGame = GameState.Match.Map != null;
                if (csgoActive && !GameState.IsSpectating)
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
                //Log.WriteLine($"Got info from GSI\nActivity:{activity}\nCSGOActive:{csgoActive}\nInGame:{inGame}\nIsSpectator:{IsSpectating(JSON)}");

            }
            catch (Exception ex)
            {
                Log.WriteLine("Error happend while getting GSI Info\n" + ex);
            }
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
            if (csgoRunning && inGame)
            {
                string phase = GameState.Match.Phase == Phase.Warmup ? "Warmup" : GameState.Round.Phase.ToString();
                discordPresence.state = GameState.Player.Team == Team.T ?
                    $"{GameState.Match.TScore} [T] ({phase}) {GameState.Match.CTScore} [CT]" :
                    $"{GameState.Match.CTScore} [CT] ({phase}) {GameState.Match.TScore} [T]";
                discordPresence.smallImageKey = GameState.IsDead ? "spectator" : GameState.IsSpectating ? "gotv_icon" : GameState.Player.Team.ToString().ToLower();
                discordPresence.smallImageText = GameState.IsDead ? "Spectating" : GameState.IsSpectating ? "Watching GOTV" : GameState.Player.Team == Team.T ? "Terrorist" : "Counter-Terrorist";
                /* maybe add in the feature join lobby
                discordPresence.joinSecret = "dsadasdsad";
                discordPresence.partyMax = 5;
                discordPresence.partyId = "37123098213021";
                discordPresence.partySize = 1;
                */
            }
            else if (discordRPCON && !csgoRunning)
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

        bool IsForegroundProcess(uint pid)
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == null) return false;

            if (GetWindowThreadProcessId(hwnd, out uint foregroundPid) == (IntPtr)0) return false;

            return (foregroundPid == pid);
        }
        private void TimerCallback(object sender, EventArgs e)
        {
            try
            {
                uint pid = 0;
                Process[] prcs = Process.GetProcessesByName("csgo");
                csgoRunning = prcs.Length > 0;
                if (csgoRunning)
                {
                    pid = (uint)prcs[0].Id;
                    if (!ServerRunning)
                    {
                        Log.WriteLine("Starting GSI Server");
                        StartGSIServer();
                    }
                }
                else
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
                    }
                }
                csgoActive = IsForegroundProcess(pid);
                if (csgoActive)
                {
                    csgoResolution = new Point(
                            (int)SystemParameters.PrimaryScreenWidth,
                            (int)SystemParameters.PrimaryScreenHeight);
                    if (Properties.Settings.Default.autoAcceptMatch && !inGame && !acceptedGame)
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        AutoAcceptMatchAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"{ex}");
            }
            GC.Collect();
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
            bool isMousePressed = (Keyboard.GetKeyState(Keyboard.VirtualKeyStates.VK_LBUTTON) & 0x80) != 0;
            if (!isMousePressed || weapon == null)
                return;
            try
            {
                int bullets = weapon.Bullets;
                WeaponType? weaponType = weapon.Type;
                string weaponName = weapon.Name;
                if (bullets == 0)
                {
                    mouse_event(MOUSEEVENTF_LEFTUP,
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
                        mouse_event(MOUSEEVENTF_LEFTDOWN,
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
                throw new Exception("Couldn't find Steam Path");
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

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        //This is a replacement for Cursor.Position in WinForms
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        //This simulates a left mouse click
        public static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
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
            using (Bitmap bitmap = new Bitmap(1, csgoResolution.Y))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(
                        csgoResolution.X / 2,
                        0),
                        Point.Empty,
                        new System.Drawing.Size(1, csgoResolution.Y));
                }
                if (Properties.Settings.Default.saveDebugFrames)
                {
                    Directory.CreateDirectory($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\DEBUG\\FRAMES");
                    bitmap.Save($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\DEBUG\\FRAMES\\Frame{frame++}.jpeg", ImageFormat.Jpeg);
                }
                bool found = false;
                for (int y = bitmap.Height - 1; y >= 0 && !found; y--)
                {
                    Color pixelColor = bitmap.GetPixel(0, y);
                    if (pixelColor == BUTTON_COLOR || pixelColor == ACTIVE_BUTTON_COLOR)
                    {
                        var clickpoint = new Point(
                            csgoResolution.X / 2,
                            y);
                        int X = clickpoint.X;
                        int Y = clickpoint.Y;
                        Log.WriteLine($"Found accept button at X:{X} Y:{Y}",caller:"AutoAcceptMatch");
                        LeftMouseClick(X, Y);
                        found = true;
                        acceptedGame = true;
                        acceptedGame = await MakeFalse(ACCEPT_BUTTON_DELAY);
                    }
                }
            }
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
            notifyIcon.Close();
            StopGSIServer();
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
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                Visibility = Visibility.Hidden;
                notifyIcon.Icon = Properties.Resources.main;
                notifyIcon.Tip = "CSAuto - CS:GO Automation";
                notifyIcon.ShowTip = true;
                notifyIcon.RightMouseButtonClick += Notifyicon_RightMouseButtonClick;
                notifyIcon.LeftMouseButtonDoubleClick += Notifyicon_LeftMouseButtonDoubleClick;
                notifyIcon.Update();
                appTimer.Interval = TimeSpan.FromMilliseconds(1000);
                appTimer.Tick += new EventHandler(TimerCallback);
                appTimer.Start();
                Log.WriteLine($"CSAuto v{VER}{(DEBUG_REVISION == "" ? "" : $" REV {DEBUG_REVISION}")} started");
                string csgoDir = GetCSGODir();
                if (csgoDir == null)
                    throw new Exception("Couldn't find CS:GO directory");
                integrationPath = csgoDir + "\\cfg\\gamestate_integration_csauto.cfg";
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
                    if (ver != VER)
                    {
                        using (FileStream fs = File.Create(integrationPath))
                        {
                            byte[] title = new UTF8Encoding(true).GetBytes(INTEGRATION_FILE);
                            fs.Write(title, 0, title.Length);
                        }
                        Log.WriteLine("Different 'gamestate_integration_csauto.cfg' was found, installing correct 'gamestate_integration_csauto.cfg'");
                    }
                }
                if (Properties.Settings.Default.autoCheckForUpdates)
                    AutoCheckUpdate();
            }
            catch (Exception ex)
            {
                if (ex.Message == "Couldn't find Steam Path"
                    || ex.Message == "Couldn't find CS:GO directory")
                {
                    autoReloadMenu.IsEnabled = false;
                    autoBuyMenu.IsEnabled = false;
                    autoPauseResumeSpotify.IsEnabled = false;
                    discordMenu.IsEnabled = false;
                }
                MessageBox.Show($"{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        Task AutoCheckUpdate()
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
                    MessageBoxResult result = MessageBox.Show($"Found newer verison ({latestVersion}) would you like to download it?", "Check for updates (CSAuto)", MessageBoxButton.YesNo, MessageBoxImage.Information);
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

            return Task.CompletedTask;
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
