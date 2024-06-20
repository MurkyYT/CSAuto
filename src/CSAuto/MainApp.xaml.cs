#region Usings
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using CSAuto.Exceptions;
using Murky.Utils;
using Murky.Utils.CSGO;
using System.Net.Sockets;
using System.IO.Pipes;
using System.Security.Principal;
using System.Windows.Threading;
using Image = System.Drawing.Image;
using DiscordRPC;
using DiscordRPC.Logging;
using ControlzEx.Theming;
#endregion
namespace CSAuto
{
    /// <summary>
    /// Main logic for CSAuto app
    /// </summary>
    public partial class MainApp : Window
    {
        #region Server Commands
        enum Commands
        {
            None,
            AcceptedMatch,
            LoadedOnMap,
            LoadedInLobby,
            Connected,
            Crashed,
            Bomb,
            Clear,
            GameState
        }
        #endregion
        #region Constants
        public const string VER = "2.1.2";
        public const string FULL_VER = VER + (DEBUG_REVISION == "" ? "" : " REV "+ DEBUG_REVISION);
        const string DEBUG_REVISION = "4";
        const string GAME_PROCCES_NAME = "cs2";
        const string GAME_WINDOW_NAME = "Counter-Strike 2";
        const string GAME_CLASS_NAME = "SDL_app";
        const string GAMESTATE_PORT = "11523";
        //const string NETCON_PORT = "21823";
        const string TIMEOUT = "5.0";
        const string BUFFER = "0.1";
        const string THROTTLE = "0.0";
        const string HEARTBEAT = "5.0";
        const string INTEGRATION_FILE = "\"CSAuto Integration v" + VER + "," + DEBUG_REVISION + "\"\r\n{\r\n\"uri\" \"http://localhost:" + GAMESTATE_PORT +
            "\"\r\n\"timeout\" \"" + TIMEOUT + "\"\r\n\"" +
            "buffer\"  \"" + BUFFER + "\"\r\n\"" +
            "throttle\" \"" + THROTTLE + "\"\r\n\"" +
            "heartbeat\" \"" + HEARTBEAT + "\"\r\n\"data\"\r\n{\r\n   \"provider\"            \"1\"\r\n   \"map\"                 \"1\"\r\n   \"round\"               \"1\"\r\n   \"player_id\"           \"1\"\r\n   \"player_state\"        \"1\"\r\n   \"player_weapons\"      \"1\"\r\n   \"player_match_stats\"  \"1\"\r\n   \"bomb\" \"1\"\r\n}\r\n}";
        const float ACCEPT_BUTTON_DELAY = 20;
        const int MAX_ARMOR_AMOUNT_TO_REBUY = 70;
        const int MIN_AMOUNT_OF_PIXELS_TO_ACCEPT = 5;
        const int BOMB_SECONDS_DELAY = 2;
        const int BOMB_SECONDS = 40 - BOMB_SECONDS_DELAY;
        const int BOMB_TIMER_DELAY = 950;
        #endregion
        #region Publics
        public GUIWindow guiWindow = null;
        public List<DiscordRPCButton> discordRPCButtons;
        public Color[] BUTTON_COLORS;
        public readonly App current = Application.Current as App;
        public const string ONLINE_BRANCH_NAME = "master";
        #endregion
        #region Readonly
        readonly object csProcessLock = new object();
        readonly NotifyIconWrapper notifyIcon = new NotifyIconWrapper();
        readonly DispatcherTimer appTimer = new DispatcherTimer();
        readonly DispatcherTimer acceptButtonTimer = new DispatcherTimer();
        readonly GameState gameState = new GameState(null);
        readonly GameStateListener GameStateListener;
        // Looks like the old way is working now? 20/06/24: Can confirm, indeed works!
        readonly DXGICapture DXGIcapture = new DXGICapture();
        #endregion
        #region Privates
        private DiscordRpcClient RPCClient;
        private string integrationPath = null;
        private string currentMapIcon = null;
        private Color BUTTON_COLOR;/* Color.FromArgb(16, 158, 89);*/
        private Color ACTIVE_BUTTON_COLOR;/*Color.FromArgb(21, 184, 105)*/
        #endregion
        #region Members
        RECT csResolution = new RECT();
        //Only workshop tools have netconport? https://github.com/ValveSoftware/csgo-osx-linux/issues/3603#issuecomment-2163695087
        //NetCon netCon = null;
        int frame = 0;
        bool csRunning = false;
        bool inGame = false;
        bool csActive = false;
        bool? inLobby = null;
        Activity? lastActivity;
        Phase? matchState;
        Phase? roundState;
        BombState? bombState;
        Weapon weapon;
        bool acceptedGame = false;
        Process steamAPIServer = null;
        Process csProcess = null;
        Process originalProcess = null;
        Thread bombTimerThread = null;
        bool hadError = false;
        IntPtr hCursorOriginal = IntPtr.Zero;
        HwndSource windowSource;
        ContextMenu exitCm;
        DateTime lastGameStateSend = DateTime.Now;
        #endregion
        #region ToImageSource
        public ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        public static ImageSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            lock (bitmap)
            {
                IntPtr hBitmap = bitmap.GetHbitmap();

                try
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    NativeMethods.DeleteObject(hBitmap);
                }
            }
        }
        #endregion
        public MainApp()
        {
            InitializeComponent();
            try
            {
                Task.Run(() =>
                {
                    BUTTON_COLORS = LoadButtonColors();
                    UpdateColors();
                });
                discordRPCButtons = DiscordRPCButtonSerializer.Deserialize();
                Application.Current.Exit += Current_Exit;
                // Try to encode my own steamid to see if its correct
                CSGOFriendCode.Encode("76561198341800115");
                new Thread(() => { CSGOMap.LoadMapIcons(); }).Start();
                InitializeDiscordRPC();
                RPCClient.Deinitialize();
                CheckForDuplicates();
                GameStateListener = new GameStateListener(ref gameState, GAMESTATE_PORT);
                GameStateListener.OnReceive += GameStateListener_OnReceive;
                Properties.DebugSettings.Default.Reset();
                InitializeContextMenu();
#if !DEBUG
                MakeSureStartupIsOn();
#endif
                Top = -1000;
                Left = -1000;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Languages.Strings.ResourceManager.GetString("error_startup"),ex.Message), Languages.Strings.ResourceManager.GetString("title_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Error(
                    $"{ex.Message}\n" +
                    $"StackTrace:{ex.StackTrace}\n" +
                    $"Source: {ex.Source}\n" +
                    $"Inner Exception: {ex.InnerException}");
                Application.Current.Shutdown();
            }
        }

        //private void ConDump_OnChange(object sender, EventArgs e)
        //{
        //    Log.WriteLine(sender);
        //}

        public void UpdateColors()
        {
            BUTTON_COLOR = BUTTON_COLORS[0];
            ACTIVE_BUTTON_COLOR = BUTTON_COLORS[1];
        }
        static Color[] LoadButtonColors()
        {
            try
            {
                string url = $"https://raw.githubusercontent.com/MurkyYT/CSAuto/{ONLINE_BRANCH_NAME}/Data/colors";
                string data = Github.GetWebInfo(url);
                if (data == "")
                    throw new WebException("Couldn't load button colors");
                string[] lines = data.Split(new char[] { '\n' });
                string path = DiscordRPCButtonSerializer.Path + "\\colors";
                (App.Current as App).settings.Set("ButtonColors", data);
                return SplitColorsLines(lines);
            }
            catch 
            {
                Log.WriteLine("|MainApp.cs| Couldn't load colors from web, trying to load latest loaded colors");
                //string path = DiscordRPCButtonSerializer.Path + "\\colors";
                if ((App.Current as App).settings["ButtonColors"] != null)
                {
                    string data = (App.Current as App).settings["ButtonColors"];
                    if (data != "")
                    {
                        string[] lines = data.Split(new char[] { '\n' });
                        return SplitColorsLines(lines);
                    }
                }
                Log.WriteLine("|MainApp.cs| Couldn't load colors at all");
                MessageBox.Show(Languages.Strings.ResourceManager.GetString("error_loadcolors"), Languages.Strings.ResourceManager.GetString("title_error"),MessageBoxButton.OK, MessageBoxImage.Error);
                return new Color[2];
            }
        }

        private static Color[] SplitColorsLines(string[] lines)
        {
            Color[] res = new Color[lines.Length];
            try
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    //This gives us an array of 3 strings each representing a number in text form.
                    var splitString = lines[i].Split(',');

                    //converts the array of 3 strings in to an array of 3 ints.
                    var splitInts = splitString.Select(item => int.Parse(item)).ToArray();

                    //takes each element of the array of 3 and passes it in to the correct slot
                    res[i] = Color.FromArgb(splitInts[0], splitInts[1], splitInts[2]);
                }
            }
            catch { }
            return res;
        }
        private void GameStateListener_OnReceive(object sender, EventArgs e)
        {
            try
            {
                if (hCursorOriginal == IntPtr.Zero && csActive)
                {
                    if (gameState.Player.CurrentActivity != Activity.Playing)
                        hCursorOriginal = NativeMethods.GetCursorHandle();
                    else
                    {
                        PressKey(Keyboard.DirectXKeyStrokes.DIK_ESCAPE);
                        Thread.Sleep(100);
                        hCursorOriginal = NativeMethods.GetCursorHandle();
                        Thread.Sleep(100);
                        PressKey(Keyboard.DirectXKeyStrokes.DIK_ESCAPE);
                    }
                    Log.WriteLine($"|MainApp.cs| hCurosr when in CS -> {hCursorOriginal}");
                }
                if (!RPCClient.IsInitialized && Properties.Settings.Default.enableDiscordRPC)
                {
                    InitializeDiscordRPC();
                    Log.WriteLine("|MainApp.cs| DiscordRpc.Initialize();");
                }
                else if (RPCClient.IsInitialized && !Properties.Settings.Default.enableDiscordRPC)
                {
                    RPCClient.Deinitialize();
                    Log.WriteLine("|MainApp.cs| DiscordRpc.Shutdown();");
                }
                Activity? activity = gameState.Player.CurrentActivity;
                Phase? currentMatchState = gameState.Match.Phase;
                Phase? currentRoundState = gameState.Round.Phase;
                BombState? currentBombState = gameState.Round.Bombstate;
                Weapon currentWeapon = gameState.Player.ActiveWeapon;
                guiWindow?.UpdateText(gameState.JSON);
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
                //if (netCon == null)
                //{
                //    NetConEstablishConnection();
                //}
                if (bombState == null && currentBombState == BombState.Planted && bombTimerThread == null && Properties.Settings.Default.bombNotification)
                {
                    StartBombTimer();
                }
                if (bombState == BombState.Planted && currentBombState != BombState.Planted && Properties.Settings.Default.bombNotification)
                {
                    bombTimerThread?.Abort();
                    bombTimerThread = null;
                    switch (currentBombState)
                    {
                        case BombState.Defused:
                            SendMessageToServer(Languages.Strings.ResourceManager.GetString("server_bombdefuse"),command:Commands.Bomb);
                            break;
                        case BombState.Exploded:
                            SendMessageToServer(Languages.Strings.ResourceManager.GetString("server_bombexplode"), command: Commands.Bomb);
                            break;
                    }

                }
                if (gameState.Match.Map != null && (inLobby == true || inLobby == null))
                {
                    inLobby = false;
                    Log.WriteLine($"|MainApp.cs| Player loaded on map {gameState.Match.Map} in mode {gameState.Match.Mode}");
                    currentMapIcon = CSGOMap.GetMapIcon(gameState.Match.Map);
                    RPCClient.SetPresence(new RichPresence()
                    {
                        Details = LimitLength(FormatString(Properties.Settings.Default.inGameDetails, gameState), 128),
                        State = LimitLength(FormatString(Properties.Settings.Default.inGameState, gameState), 128),
                        Party = new Party() { ID = "", Size = 0, Max = 0 },
                        Assets = new Assets()
                        {
                            LargeImageKey = currentMapIcon ?? "cs2_icon",
                            LargeImageText = gameState.Match.Map,
                            SmallImageKey = null,
                            SmallImageText = null
                        },
                        Timestamps = new Timestamps()
                        {
                            Start = UnixTimeStampToDateTime(gameState.Timestamp),
                            End = null
                        },
                        Buttons = GetDiscordRPCButtons()
                    });
                    if (Properties.Settings.Default.mapNotification)
                        SendMessageToServer(string.Format(Languages.Strings.ResourceManager.GetString("server_loadedmap"), gameState.Match.Map, gameState.Match.Mode),command:Commands.LoadedOnMap);
                    if (DXGIcapture.Enabled)
                    {
                        DXGIcapture.DeInit();
                        Log.WriteLine("|MainApp.cs| Deinit DXGI Capture");
                    }
                }
                else if (gameState.Match.Map == null && (inLobby == false || inLobby == null))
                {
                    inLobby = true;
                    currentMapIcon = null;
                    Log.WriteLine($"|MainApp.cs| Player is back in main menu");
                    RPCClient.SetPresence(new RichPresence()
                    {
                        Details = LimitLength(FormatString(Properties.Settings.Default.lobbyDetails, gameState), 128),
                        State = LimitLength(FormatString(Properties.Settings.Default.lobbyState, gameState), 128),
                        Party = new Party() { ID = "", Size = 0, Max = 0 },
                        Assets = new Assets()
                        {
                            LargeImageKey = "cs2_icon",
                            LargeImageText = "Menu",
                            SmallImageKey = null,
                            SmallImageText = null
                        },
                        Timestamps = new Timestamps()
                        {
                            Start = UnixTimeStampToDateTime(gameState.Timestamp),
                            End = null
                        },
                        Buttons = GetDiscordRPCButtons()
                    });
                    if (Properties.Settings.Default.lobbyNotification)
                        SendMessageToServer(Languages.Strings.ResourceManager.GetString("server_loadedlobby"),command:Commands.LoadedInLobby);
                    if (!DXGIcapture.Enabled && !Properties.Settings.Default.oldScreenCaptureWay)
                    {
                        DXGIcapture.Init();
                        Log.WriteLine("|MainApp.cs| Init DXGI Capture");
                    }
                }
                lastActivity = activity;
                matchState = currentMatchState;
                roundState = currentRoundState;
                weapon = currentWeapon;
                bombState = currentBombState;
                inGame = gameState.Match.Map != null;
                if (csActive && !gameState.IsSpectating)
                {
                    if (Properties.Settings.Default.autoReload && lastActivity != Activity.Menu && csActive)
                        TryToAutoReload();
                    if (lastActivity == Activity.Playing && csActive && Properties.Settings.Default.autoBuyEnabled)
                        AutoBuy();
                    if (Properties.Settings.Default.autoPausePlaySpotify)
                        AutoPauseResumeSpotify();
                }
                UpdateDiscordRPC();
                if(DateTime.Now - lastGameStateSend > TimeSpan.FromSeconds(1))
                {
                    lastGameStateSend = DateTime.Now;
                    SendMessageToServer(gameState.JSON, onlyServer: true, command: Commands.GameState);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("|MainApp.cs| An error accured while getting GSI Info\n" + ex);
            }
        }

        private void AutoBuy()
        {
            if (matchState == Phase.Live && roundState == Phase.Freezetime)
            {
                List<BuyItem> items = current.buyMenu.GetItemsToBuy(gameState,MAX_ARMOR_AMOUNT_TO_REBUY);
                if (items.Count != 0 && !BuyMenuOpen())
                {
                    PressKey(Keyboard.DirectXKeyStrokes.DIK_B);
                    Thread.Sleep(100);
                }
                foreach (BuyItem item in items)
                {
                    Log.WriteLine($"|MainApp.cs| Auto buying {item.Name}");
                    PressKeys(new Keyboard.DirectXKeyStrokes[]
                    {
                        //Category key
                        (Keyboard.DirectXKeyStrokes)(item.GetSlot()[0] - '0' + 1),
                        //Weapon key
                        (Keyboard.DirectXKeyStrokes)(item.GetSlot()[1] - '0' + 1)
                    });
                    //Have to press b after buying grenades because the buy menu stays at the grenades category
                    if (item.IsGrenade())
                        PressKey(Keyboard.DirectXKeyStrokes.DIK_B);
                }
                if (items.Count != 0)
                    PressKey(Keyboard.DirectXKeyStrokes.DIK_B);
            }
        }

        private bool BuyMenuOpen()
        {
            IntPtr res = NativeMethods.GetCursorHandle();
            Log.WriteLine($"|MainApp.cs| Original hCurosr: {hCursorOriginal} || {hCursorOriginal - 2}: new one {res}");
            return (hCursorOriginal == res || hCursorOriginal - 2 == res) && gameState.Player.CurrentActivity == Activity.Playing;
        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp);
            return dateTime;
        }
        //public void NetConEstablishConnection()
        //{
        //    netCon = new NetCon("127.0.0.1", int.Parse(NETCON_PORT));
        //    netCon.MatchFound += NetCon_MatchFound;
        //    acceptButtonTimer.Interval = TimeSpan.FromMilliseconds(1000);
        //    acceptButtonTimer.Tick += AcceptButtonTimer_Tick;
        //}

        //private void AcceptButtonTimer_Tick(object sender, EventArgs e)
        //{
        //    if (Properties.Settings.Default.autoAcceptMatch && !inGame && !acceptedGame && csActive)
        //        _ = AutoAcceptMatchAsync();
        //    else if (inGame || !Properties.Settings.Default.autoAcceptMatch)
        //        acceptButtonTimer.Stop();
        //}

        //private void NetCon_MatchFound(object sender, EventArgs e)
        //{
        //    Log.WriteLine("Match Found!");
        //    if (acceptButtonTimer.IsEnabled) acceptButtonTimer.Stop();
        //    if (!acceptButtonTimer.IsEnabled && !acceptedGame && !inGame)
        //    {
        //        Log.WriteLine("Starting searching for accept button...");
        //        acceptButtonTimer.Start();
        //    }
        //}

        //public void NetConCloseConnection()
        //{
        //    if (netCon != null)
        //    {
        //        netCon.Close();
        //        netCon = null;
        //    }
        //}
        public string FormatString(string original, GameState gameState)
        {
            if (gameState.Player != null)
            {
                return original
                    .Replace("{FriendCode}", CSGOFriendCode.Encode(gameState.MySteamID))
                    .Replace("{Gamemode}", gameState.Match.Mode.ToString())
                    .Replace("{Map}", gameState.Match.Map)
                    .Replace("{TeamScore}", gameState.Player.Team == Team.CT ? gameState.Match.CTScore.ToString() : gameState.Match.TScore.ToString())
                    .Replace("{MyTeam}", gameState.Player.Team == null ? Team.T.ToString() : gameState.Player.Team.ToString())
                    .Replace("{RoundState}", gameState.Match.Phase == Phase.Warmup ? "Warmup" : gameState.Round.Phase.ToString())
                    .Replace("{MatchState}", gameState.Match.Phase.ToString())
                    .Replace("{EnemyScore}", gameState.Player.Team == Team.CT ? gameState.Match.TScore.ToString() : gameState.Match.CTScore.ToString())
                    .Replace("{EnemyTeam}", gameState.Player.Team == Team.CT ? Team.T.ToString() : Team.CT.ToString())
                    .Replace("{TScore}", gameState.Match.TScore.ToString())
                    .Replace("{CTScore}", gameState.Match.CTScore.ToString())
                    .Replace("{SteamID}", gameState.MySteamID)
                    .Replace("{Name}", gameState.Player.Name)
                    .Replace("{Kills}", gameState.Player.Kills.ToString())
                    .Replace("{Deaths}",gameState.Player.Deaths.ToString())
                    .Replace("{MVPS}", gameState.Player.MVPS.ToString());
            }
            return original;
        }

        public string LimitLength(string str,int length)
        {
            return str?.Substring(0, Math.Min(str.Length, length));
        }

        private void MakeSureStartupIsOn()
        {
            string appname = Assembly.GetEntryAssembly().GetName().Name;
            string executablePath = Process.GetCurrentProcess().MainModule.FileName;
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (Properties.Settings.Default.runAtStartUp)
                {
                    rk.SetValue(appname , executablePath + " "+current.Args);
                    Log.WriteLine("|MainApp.cs| "+executablePath + " " +current.Args);
                }
                else
                {
                    rk.DeleteValue(appname, false);
                }
            }
        }
        private void Current_Exit(object sender, ExitEventArgs e)
        {
            Exited();
        }

        internal void InitializeContextMenu()
        {
            exitCm = new ContextMenu();
            exitCm.Closed += Exitcm_Closed;
            MenuItem exit = new MenuItem
            {
                Header = Languages.Strings.ResourceManager.GetString("menu_exit")
            };
            MenuItem launchCS = new MenuItem
            {
                Header = Languages.Strings.ResourceManager.GetString("menu_launchcs")
            };
            MenuItem options = new MenuItem
            {
                Header = Languages.Strings.ResourceManager.GetString("menu_options")
            };
            MenuItem optionsOpen = new MenuItem
            {
                Header = Languages.Strings.ResourceManager.GetString("menu_open")
            };
            MenuItem optionsExport = new MenuItem
            {
                Header = Languages.Strings.ResourceManager.GetString("menu_optionsexport")
            };
            MenuItem optionsImport = new MenuItem
            {
                Header = Languages.Strings.ResourceManager.GetString("menu_optionsimport")
            };
            exit.Click += Exit_Click;
            options.Items.Add(optionsOpen);
            options.Items.Add(new Separator());
            options.Items.Add(optionsImport);
            options.Items.Add(optionsExport);
            optionsImport.Click += OptionsImport_Click;
            optionsExport.Click += OptionsExport_Click;
            optionsOpen.Click += Options_Click;
            launchCS.Click += LaunchCs_Click;
            MenuItem about = new MenuItem
            {
                Header = $"{typeof(MainApp).Namespace} - {FULL_VER}",
                IsEnabled = false,
                Icon = new System.Windows.Controls.Image
                {
                    Source = ToImageSource(System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location))
                },
                Foreground = new SolidColorBrush(ThemeManager.Current.GetTheme($"Dark.{Properties.Settings.Default.currentColor}").PrimaryAccentColor)
            };
            MenuItem checkForUpdates = new MenuItem
            {
                Header = Languages.Strings.ResourceManager.GetString("menu_checkforupdates")
            };
            checkForUpdates.Click += CheckForUpdates_Click;
            exitCm.Items.Add(about);
            exitCm.Items.Add(new Separator());
            exitCm.Items.Add(launchCS);
            exitCm.Items.Add(options);
            exitCm.Items.Add(new Separator());
            exitCm.Items.Add(checkForUpdates);
            exitCm.Items.Add(exit);
            exitCm.StaysOpen = false;
        }

        private void OptionsExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSAuto config file| *.csauto",
                    DefaultExt = "csauto"
                };
                if ((bool)saveFileDialog.ShowDialog())
                {
                    File.WriteAllText(saveFileDialog.FileName, current.settings.ToString(),Encoding.UTF8);
                    MessageBox.Show(string.Format(Languages.Strings.ResourceManager.GetString("file_savesucess"), saveFileDialog.FileName), Languages.Strings.ResourceManager.GetString("title_success"), MessageBoxButton.OK, MessageBoxImage.Information);
                }  
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), Languages.Strings.ResourceManager.GetString("title_error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OptionsImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "CSAuto config file| *.csauto",
                    DefaultExt = "csauto"
                };
                if ((bool)openFileDialog.ShowDialog())
                {
                    current.settings.Import(openFileDialog.FileName);
                    current.LoadSettings();
                    current.buyMenu.Load(current.settings);
                    MessageBox.Show(string.Format(Languages.Strings.ResourceManager.GetString("file_importsucess"), openFileDialog.FileName), Languages.Strings.ResourceManager.GetString("title_success"), MessageBoxButton.OK, MessageBoxImage.Information);
                    RestartMessageBox();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), Languages.Strings.ResourceManager.GetString("title_error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RestartMessageBox()
        {
            var restart = MessageBox.Show(Languages.Strings.ResourceManager.GetString("msgbox_restartneeded"), Languages.Strings.ResourceManager.GetString("title_restartneeded"), MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (restart == MessageBoxResult.OK)
            {
                Process.Start(Assembly.GetExecutingAssembly().Location, "--restart" + (guiWindow == null ? "" : " --show ") + current.Args);
                Application.Current.Shutdown();
            }
        }
        // Making the context menu rounded leaves some trasparent artifacts :(
        //private void ExitCm_Opened(object sender, RoutedEventArgs e)
        //{
        //    if (current.IsWindows11)
        //    {
        //        PresentationSource src = HwndSource.FromDependencyObject(exitCm);
        //        IntPtr handle = ((HwndSource)src).Handle;
        //        var attribute = NativeMethods.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
        //        var preference = NativeMethods.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
        //        NativeMethods.DwmSetWindowAttribute(handle, attribute, ref preference, sizeof(uint));
        //        NativeMethods.SetWindowPos(handle, IntPtr.Zero, 0, 0, (int)(exitCm.Width + 50), (int)(exitCm.Height + 50), 2);
        //        NativeMethods.UpdateWindow(handle);
        //    }
        //}

        private void LaunchCs_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://rungameid/730");
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            Notifyicon_LeftMouseButtonDoubleClick(null, null);
        }

        private void InitializeDiscordRPC()
        {
            try
            {
                Directory.CreateDirectory(Log.WorkPath + "\\DEBUG\\DISCORD");
            }
            catch { }
            RPCClient = new DiscordRpcClient(APIKeys.DISCORD_BOT_ID);
            //Subscribe to events
#if DEBUG
            File.Create(Log.WorkPath + "\\DEBUG\\DISCORD\\Debug_Log.txt").Close();
            RPCClient.Logger = new FileLogger(Log.WorkPath + "\\DEBUG\\DISCORD\\Debug_Log.txt",DiscordRPC.Logging.LogLevel.Trace);
#elif !DEBUG
            try
            {
                File.Create(Log.WorkPath + "\\DEBUG\\DISCORD\\Error_Log.txt").Close();
                RPCClient.Logger = new FileLogger(Log.WorkPath + "\\DEBUG\\DISCORD\\Error_Log.txt", DiscordRPC.Logging.LogLevel.Error);
            }
            catch { MessageBox.Show(Languages.Strings.ResourceManager.GetString("error_createfiles"), Languages.Strings.ResourceManager.GetString("title_warning"), MessageBoxButton.OK, MessageBoxImage.Warning); }
#endif
            RPCClient.OnReady += (sender, e) =>
            {
                Log.WriteLine($"|MainApp.cs| Received Discord RPC Ready! {e.User.Username}");
            };
            RPCClient.Initialize();
        }

        private DiscordRPC.Button[] GetDiscordRPCButtons()
        {
            DiscordRPC.Button[] res = new DiscordRPC.Button[1 + discordRPCButtons.Count];
            res[0] = new DiscordRPC.Button() { Label = "CSAuto", Url = "https://github.com/MurkyYT/CSAuto" };
            for (int i = 1; i < res.Length; i++)
            {
                res[i] = new DiscordRPC.Button() { Label = discordRPCButtons[i - 1].Label, Url = FormatString(discordRPCButtons[i - 1].Url,gameState) };
            }
            return res;
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
                    Log.WriteLine("|MainApp.cs| Checking for updates");
                    string latestVersion = Github.GetWebInfo($"https://raw.githubusercontent.com/MurkyYT/CSAuto/{ONLINE_BRANCH_NAME}/Data/version");
                    Log.WriteLine($"|MainApp.cs| The latest version is {latestVersion}");
                    if (latestVersion == VER)
                    {
                        Log.WriteLine("|MainApp.cs| Latest version installed");
                        MessageBox.Show(Languages.Strings.ResourceManager.GetString("msgbox_latestversion"), Languages.Strings.ResourceManager.GetString("title_update"), MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        Log.WriteLine($"|MainApp.cs| Newer version found ({latestVersion}),current version is {VER}");
                        MessageBoxResult result = MessageBox.Show(string.Format(Languages.Strings.ResourceManager.GetString("msgbox_newerversion"),latestVersion), Languages.Strings.ResourceManager.GetString("title_update"), MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                        {
                            Log.WriteLine("|MainApp.cs| Launching updater");
                            try
                            {
                                string path = Path.GetTempPath() + "CSAutoUpdate";
                                if (Directory.Exists(path))
                                    Directory.Delete(path, true);
                                Directory.CreateDirectory(path);
                                File.Copy(Log.WorkPath + "\\updater.exe", path + "\\updater.exe");
                                Process.Start(path + "\\updater.exe", $"{Log.WorkPath} https://github.com/murkyyt/csauto/releases/latest/download/CSAuto_Portable.zip CSAuto.exe \"{current.Args} {(guiWindow == null ? "" : "--show")} --restart\" .portable");
                                Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
                            }
                            catch
                            {
                                Process.Start("https://github.com/MurkyYT/CSAuto/releases/latest");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"|MainApp.cs| Couldn't check for updates - '{ex.Message}'");
                    MessageBox.Show($"{Languages.Strings.ResourceManager.GetString("error_update")}\n'{ex.Message}'", Languages.Strings.ResourceManager.GetString("title_update"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }).Start();
        }
        private void StartBombTimer()
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long ms = (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
            long result = ms / 1000;
            int diff = (int)(gameState.Timestamp - result);
            SendMessageToServer($"{Languages.Strings.ResourceManager.GetString("server_bombplanted")} ({DateTime.Now})", onlyTelegram: true,command:Commands.Bomb);
            bombTimerThread = new Thread(() =>
            {
                for (int seconds = BOMB_SECONDS - diff; seconds >= 0; seconds--)
                {
                    SendMessageToServer($"{Languages.Strings.ResourceManager.GetString("server_timeleft")} {seconds}", onlyServer: true, command: Commands.Bomb);
                    Thread.Sleep(BOMB_TIMER_DELAY);
                }
                bombTimerThread = null;
            });
            bombTimerThread.Start();
        }

        private void UpdateDiscordRPC()
        {
            try
            {
                if (csRunning && inGame)
                {
                    RPCClient.SetPresence(new RichPresence()
                    {
                        Details = LimitLength(FormatString(Properties.Settings.Default.inGameDetails, gameState),128),
                        State = LimitLength(FormatString(Properties.Settings.Default.inGameState, gameState),128),
                        Party = new Party(),
                        Assets = new Assets()
                        {
                            LargeImageKey = currentMapIcon ?? "cs2_icon",
                            LargeImageText = RPCClient.CurrentPresence.Assets.LargeImageText,
                            SmallImageKey = gameState.IsSpectating ? "gotv_icon" : gameState.IsDead ? "spectator" : gameState.Player.Team.ToString().ToLower(),
                            SmallImageText = gameState.IsSpectating ? "Watching CSTV" : gameState.IsDead ? "Spectating" : gameState.Player.Team == Team.T ? "Terrorist" : "Counter-Terrorist"
                        },
                        Timestamps = new Timestamps()
                        {
                            Start = RPCClient.CurrentPresence.Timestamps.Start,
                            End = null
                        },
                        Buttons = GetDiscordRPCButtons()
                    });
                }
                else if (csRunning && !inGame)
                {
                    if (Properties.Settings.Default.enableLobbyCount)
                    {
                        string steamworksRes = GetLobbyInfoFromSteamworks();
                        string lobbyid = steamworksRes.Split('(')[1].Split(')')[0];
                        string partyMax = steamworksRes.Split('/')[1].Split('(')[0];
                        string partysize = steamworksRes.Split('/')[0];
                        RPCClient.SetPresence(new RichPresence()
                        {
                            Details = RPCClient.CurrentPresence.Details,
                            State = RPCClient.CurrentPresence.State,
                            Party = new Party()
                            { ID = lobbyid == "0" ? "0" : lobbyid, Max = int.Parse(partyMax), Size = int.Parse(partysize) },
                            Assets = new Assets()
                            {
                                LargeImageKey = RPCClient.CurrentPresence.Assets.LargeImageKey,
                                LargeImageText = RPCClient.CurrentPresence.Assets.LargeImageText,
                                SmallImageKey = RPCClient.CurrentPresence.Assets.SmallImageKey,
                                SmallImageText = RPCClient.CurrentPresence.Assets.SmallImageText
                            },
                            Timestamps = new Timestamps()
                            {
                                Start = RPCClient.CurrentPresence.Timestamps.Start,
                                End = null
                            },
                            Buttons = GetDiscordRPCButtons()
                        });
                    }
                }
                else if (RPCClient.IsInitialized && !csRunning)
                {
                    RPCClient.Deinitialize();
                    Log.WriteLine("|MainApp.cs| DiscordRpc.Shutdown();");
                }
            }
            catch(NullReferenceException) { }
        }

        private void AutoPauseResumeSpotify()
        {
            if (gameState.Player.CurrentActivity == Activity.Playing)
            {
                if (gameState.Player.Health > 0 && gameState.Player.SteamID == gameState.MySteamID)
                {
                    Spotify.Pause();
                }
                else if (gameState.Player.SteamID != gameState.MySteamID ||
                    (gameState.Player.Health <= 0 && gameState.Player.SteamID == gameState.MySteamID))
                {
                    Spotify.Resume();
                }
            }
            else if (gameState.Player.CurrentActivity != Activity.Textinput)
            {
                Spotify.Resume();
            }

        }
        private void SendMessageToServer(string message, bool onlyTelegram = false, bool onlyServer = false,Commands command = Commands.None)
        {
            new Thread(() =>
            {
                if (Properties.Settings.Default.telegramChatId != "" && !onlyServer)
                    Telegram.SendMessage(message, Properties.Settings.Default.telegramChatId,
                        Telegram.CheckToken(Properties.Settings.Default.customTelegramToken) ? 
                        Properties.Settings.Default.customTelegramToken : APIKeys.TELEGRAM_BOT_TOKEN);
                if (Properties.Settings.Default.phoneIpAddress == "" || !Properties.Settings.Default.mobileAppEnabled || onlyTelegram)
                    return;
                try // Try connecting and send the message bytes  
                {
                    TcpClient client = new TcpClient(Properties.Settings.Default.phoneIpAddress, 11_000); // Create a new connection  
                    NetworkStream stream = client.GetStream();
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    // message + command
                    int length = messageBytes.Length + 1;
                    byte[] lengthBuf = BitConverter.GetBytes(length);
                    byte[] buffer = new byte[4 + length];
                    for (int i = 0; i < 4; i++)
                        buffer[i] = lengthBuf[i];
                    buffer[4] = (byte)command;
                    for (int i = 0; i < messageBytes.Length; i++)
                        buffer[5 + i] = messageBytes[i];
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Dispose();
                    client.Close();
                }
                catch (Exception ex){ Log.WriteLine(ex); }
            }).Start();
        }
        private void TimerCallback(object sender, EventArgs e)
        {
            try
            {
                if (!csRunning)
                {
                    lock (csProcessLock)
                    {
                        csProcess = NativeMethods.GetProccesByWindowName(GAME_WINDOW_NAME, out bool suc, GAME_CLASS_NAME, GAME_PROCCES_NAME);
                        if (suc)
                        {
                            NativeMethods.RegisterShellHookWindow(windowSource.Handle);
                            csRunning = true;
                            csProcess.Exited += CsProcess_Exited;
                            csProcess.EnableRaisingEvents = true;
                            if (!GameStateListener.ServerRunning)
                            {
                                Log.WriteLine("|MainApp.cs| Starting GSI Server");
                                GameStateListener.StartGSIServer();
                            }
                            if (steamAPIServer == null && Properties.Settings.Default.enableLobbyCount)
                            {
                                steamAPIServer = new Process() { StartInfo = { FileName = "steamapi.exe" } };
                                if (!steamAPIServer.Start())
                                {
                                    Log.WriteLine("|MainApp.cs| Couldn't launch 'steamapi.exe'");
                                    steamAPIServer = null;
                                }
                            }
                            //ConDump.StartListening();
                            //ConDump.OnChange += ConDump_OnChange;
                            hCursorOriginal = IntPtr.Zero;
                            NativeMethods.OptimizeMemory();
                        }
                    }
                }
                else
                {
                    csActive = NativeMethods.IsForegroundProcess((uint)csProcess.Id);
                    if (csActive)
                    {
                        bool success = NativeMethods.GetWindowRect(csProcess.MainWindowHandle, out RECT windSize);
                        //screenResolution = new Size(
                        //        (int)SystemParameters.PrimaryScreenWidth,
                        //        (int)SystemParameters.PrimaryScreenHeight);
                        if (success)
                            csResolution = windSize;
                        if (Properties.Settings.Default.autoAcceptMatch && !inGame && !acceptedGame)
                            _ = AutoAcceptMatchAsync();
                    }
                } 
            }
            catch (Exception ex)
            {
                Log.WriteLine($"|MainApp.cs| {ex}");
            }
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (Properties.Settings.Default.autoFocusOnCS &&
                Properties.Settings.Default.autoAcceptMatch &&
                csProcess != null && !csActive && inLobby == true &&
                lParam == csProcess.MainWindowHandle && wParam == NativeMethods.HSHELL_FLASH)
            {
                if (Properties.Settings.Default.focusBackOnOriginalWindow)
                    originalProcess = NativeMethods.GetForegroundProcess();
                if(NativeMethods.BringToFront(csProcess.MainWindowHandle))
                    Log.WriteLine("|MainApp.cs| Switching to CS window");
            }
            return IntPtr.Zero;
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
                Log.WriteLine("|MainApp.cs| Server could not be verified.");
            }
            pipeClient.Close();
            return res;
        }

        private string GetLobbyID()
        {
            string KEY = Properties.Settings.Default.steamAPIkey;
            if (KEY == "" || KEY == null)
                return "0";
            string apiURL = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={KEY}&steamids={gameState.MySteamID}&appids=730";
            string webInfo = Github.GetWebInfo(apiURL);
            string[] split = webInfo.Split(new string[] { "\"lobbysteamid\":\"" }, StringSplitOptions.None);
            if (split.Length < 2)
                return "0";
            return split[1].Split('"')[0];
        }
        private void CsProcess_Exited(object sender, EventArgs e)
        {
            lock (csProcessLock)
            {
                Log.WriteLine($"|MainApp.cs| CS Exit Code: {csProcess.ExitCode}");
                if (csProcess.ExitCode != 0 && Properties.Settings.Default.crashedNotification)
                    SendMessageToServer(Languages.Strings.ResourceManager.GetString("server_gamecrash"),command:Commands.Crashed);
                if (windowSource.Handle != IntPtr.Zero)
                    NativeMethods.DeregisterShellHookWindow(windowSource.Handle);
                if (RPCClient.IsInitialized)
                {
                    RPCClient.Deinitialize();
                    Log.WriteLine("|MainApp.cs| DiscordRpc.Shutdown();");
                }
                if (gameState.Timestamp != 0)
                {
                    gameState.UpdateJson(null);
                }
                if (GameStateListener.ServerRunning)
                {
                    Log.WriteLine("|MainApp.cs| Stopping GSI Server");
                    GameStateListener.StopGSIServer();
                    //NetConCloseConnection();
                    SendMessageToServer("", onlyServer: true,command:Commands.Clear);
                }
                if (steamAPIServer != null)
                {
                    try
                    {
                        steamAPIServer.Kill();
                    }
                    catch { }
                    steamAPIServer = null;
                }
                if (DXGIcapture.Enabled)
                {
                    DXGIcapture.DeInit();
                    Log.WriteLine("|MainApp.cs| Deinit DXGI Capture");
                }
                if (Properties.Settings.Default.autoCloseCSAuto)
                    Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
                csProcess = null;
                inLobby = false;
                csRunning = false;
                //ConDump.StopListening();
                //ConDump.OnChange -= ConDump_OnChange;
                NativeMethods.OptimizeMemory();
            }
        }
        private void TryToAutoReload()
        {
            bool isMousePressed = (Keyboard.GetKeyState(Keyboard.VirtualKeyStates.VK_LBUTTON) < 0);
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
                    //netCon.SendCommand("+reload");
                    //netCon.SendCommand("-attack");
                    Log.WriteLine("|MainApp.cs| Auto reloading");
                    if ((weaponType == WeaponType.Rifle
                        || weaponType == WeaponType.MachineGun
                        || weaponType == WeaponType.SubmachineGun
                        || weaponName == "weapon_cz75a")
                        && (weaponName != "weapon_sg556")
                        && Properties.Settings.Default.ContinueSpraying)
                    {
                        Thread.Sleep(100);
                        //bool mousePressed = (Keyboard.GetKeyState(Keyboard.VirtualKeyStates.VK_LBUTTON) < 0);
                        //if (mousePressed)
                        //{
                        //netCon.SendCommand("+attack");

                        NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN,
                            System.Windows.Forms.Cursor.Position.X,
                            System.Windows.Forms.Cursor.Position.Y,
                            0, 0);
                        Log.WriteLine($"|MainApp.cs| Continue spraying ({weaponName} - {weaponType})");
                        //}
                    }
                    //netCon.SendCommand("-reload");
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

        // from - https://gist.github.com/moritzuehling/7f1c512871e193c0222f
        private string GetCSGODir()
        {
            string csgoDir = Steam.GetGameDir("Counter-Strike Global Offensive");
            if (csgoDir != null)
                return $"{csgoDir}\\";
            return null;
        }
        public static void LeftMouseClick(int xpos, int ypos)
        {
            NativeMethods.SetCursorPos(xpos, ypos);
            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
            Log.WriteLine($"|MainApp.cs| Left clicked at X:{xpos} Y:{ypos}");
        }
        private async Task AutoAcceptMatchAsync()
        {
            if(!DXGIcapture.Enabled && !Properties.Settings.Default.oldScreenCaptureWay)
            {
                DXGIcapture.Init();
                Log.WriteLine("|MainApp.cs| Init DXGI Capture");
            }
            if ((DXGIcapture.Enabled && !Properties.Settings.Default.oldScreenCaptureWay && inLobby == true) ||
                (inLobby == true && Properties.Settings.Default.oldScreenCaptureWay))
            {
                IntPtr _handle = IntPtr.Zero;
                if (!Properties.Settings.Default.oldScreenCaptureWay)
                {
                    _handle = DXGIcapture.GetCapture();
                    if (_handle == IntPtr.Zero)
                    {
                        DXGIcapture.DeInit();
                        Log.WriteLine("|MainApp.cs| Deinit DXGI Capture");
                        DXGIcapture.Init();
                        Log.WriteLine("|MainApp.cs| Init DXGI Capture");
                    }
                }
                Bitmap bitmap = Properties.Settings.Default.oldScreenCaptureWay ?
                    new Bitmap(csResolution.Width, csResolution.Height) : Image.FromHbitmap(_handle);
                if (Properties.Settings.Default.oldScreenCaptureWay)
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(new Point(
                            csResolution.Left,
                            csResolution.Top),
                            Point.Empty,
                            new System.Drawing.Size(csResolution.Width, csResolution.Height));
                    }
                }
                else
                    bitmap = bitmap.Clone(new Rectangle() { X = csResolution.X, Y = csResolution.Y, Width = csResolution.Width, Height = csResolution.Height }, bitmap.PixelFormat);
                if (Properties.Settings.Default.saveDebugFrames)
                {
                    try
                    {
                        Directory.CreateDirectory($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\DEBUG\\FRAMES");
                        bitmap.Save($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\DEBUG\\FRAMES\\Frame{frame++}.jpeg", ImageFormat.Jpeg);
                    }
                    catch { }
                }
                if (guiWindow != null)
                {
                    guiWindow.latestCapturedFrame.Source = CreateBitmapSourceFromBitmap(bitmap);
                    Point pixelPos = new Point(csResolution.Width / 2, (int)(csResolution.Height / (1050f / 473f)) + 1);
                    Color pixelColor = bitmap.GetPixel(pixelPos.X, pixelPos.Y);
                    guiWindow.DebugPixelColor.Text = $"Pixel color at ({pixelPos.X},{pixelPos.Y}): {pixelColor}";
                }
                bool found = false;
                int count = 0;
                int yStart = bitmap.Height - 1;
                int xMiddle = csResolution.Width / 2;
                for (int y = yStart; y >= 0 && !found && !acceptedGame; y--)
                {
                    Color pixelColor = bitmap.GetPixel(xMiddle, y);
                    if (pixelColor == BUTTON_COLOR || pixelColor == ACTIVE_BUTTON_COLOR)
                    {

                        if (count >= MIN_AMOUNT_OF_PIXELS_TO_ACCEPT) /*
                                        * just in case the program finds the 0:20 timer tick
                                        * didnt happen for a while but can happen still
                                        * happend while trying to create a while loop to search for button
                                        */
                        {
                            var clickpoint = new Point(
                                csResolution.X + xMiddle,
                                y);
                            int X = clickpoint.X;
                            int Y = clickpoint.Y;
                            Log.WriteLine($"|MainApp.cs| Found accept button at X:{X} Y:{Y} Color:{pixelColor}", caller: "AutoAcceptMatch");
                            found = true;
                            if (Properties.DebugSettings.Default.pressAcceptButton)
                            {
                                LeftMouseClick(X, Y);
                                if (CheckIfAccepted(bitmap, Y))
                                {
                                    if (Properties.Settings.Default.acceptedNotification)
                                        SendMessageToServer(Languages.Strings.ResourceManager.GetString("server_acceptmatch"),command:Commands.AcceptedMatch);
                                    acceptedGame = true;
                                    if (acceptButtonTimer.IsEnabled)
                                        acceptButtonTimer.Stop();
                                    if (Properties.Settings.Default.sendAcceptImage && Properties.Settings.Default.telegramChatId != "")
                                        Telegram.SendPhoto(bitmap,
                                            Properties.Settings.Default.telegramChatId,
                                            Telegram.CheckToken(Properties.Settings.Default.customTelegramToken) ?
                                                Properties.Settings.Default.customTelegramToken
                                                : APIKeys.TELEGRAM_BOT_TOKEN,
                                            $"{csResolution.Width}X{csResolution.Height}\n" +
                                            $"({X},{Y})\n" +
                                            $"{DateTime.Now}");
                                    if (originalProcess != null && Properties.Settings.Default.focusBackOnOriginalWindow)
                                    {
                                        if (NativeMethods.BringToFront(originalProcess.MainWindowHandle))
                                        {
                                            Log.WriteLine($"|MainApp.cs| Switched back to '{originalProcess.MainWindowTitle}'");
                                            originalProcess = null;
                                        }
                                    }
                                    acceptedGame = await MakeFalse(ACCEPT_BUTTON_DELAY);
                                }
                            }
                        }
                        count++;
                    }
                }
                if(!Properties.Settings.Default.oldScreenCaptureWay)
                    NativeMethods.DeleteObject(_handle);
                bitmap.Dispose();
            }
            else if (inLobby == true && !DXGIcapture.Enabled && Properties.Settings.Default.oldScreenCaptureWay)
            {
                DXGIcapture.Init();
                Log.WriteLine("|MainApp.cs| Init DXGI Capture");
            }
        }
        private bool CheckIfAccepted(Bitmap bitmap, int maxY)
        {
            int count = 0;
            for (int y = bitmap.Height - 1; y >= maxY; y--)
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
            if(exitCm != null)
                exitCm.IsOpen = true;
            Activate();
        }

        private void Exitcm_Closed(object sender, RoutedEventArgs e)
        {
            NativeMethods.OptimizeMemory();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Exited()
        {
            notifyIcon?.Close();

            GameStateListener?.StopGSIServer();
            
            if (steamAPIServer != null && !steamAPIServer.HasExited)
                steamAPIServer.Kill();

            RPCClient?.Dispose();

            DiscordRPCButtonSerializer.Serialize(discordRPCButtons);
            Properties.Settings.Default.Save();
            current.MoveSettings();

            if (current.IsPortable)
            {
                File.WriteAllText(Log.WorkPath+"\\.conf", current.settings.ToString(), Encoding.UTF8);
                current.settings.DeleteSettings();
            }
            //Application.Current.Shutdown();
        }
        private void CheckForDuplicates()
        {
            var currentProcess = Process.GetCurrentProcess();
            var duplicates = Process.GetProcessesByName(currentProcess.ProcessName).Where(o => o.Id != currentProcess.Id);
            if (duplicates.Count() > 0)
            {
                //notifyIcon.Close();
                //Application.Current.Shutdown();
                //Log.WriteLine($"Shutting down, found another CSAuto process");
                duplicates.ToList().ForEach(dupl => dupl.Kill());
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
            throw new Exception(Languages.Strings.ResourceManager.GetString("exception_nonetworkadapter")/*"No network adapters with an IPv4 address in the system!"*/);
        }
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                Visibility = Visibility.Hidden;
                InitializeNotifyIcon();
                InitializeTimer();
                Log.WriteLine($"|MainApp.cs| CSAuto v{FULL_VER} started");
                string csgoDir = GetCSGODir();
#if !DEBUG
                if (Properties.Settings.Default.autoCheckForUpdates)
                    AutoCheckUpdate();
#endif
                if (Properties.Settings.Default.connectedNotification && !current.Restarted)
                    SendMessageToServer(string.Format(Languages.Strings.ResourceManager.GetString("server_computeronline"), Environment.MachineName, GetLocalIPAddress(),FULL_VER),command:Commands.Connected);
                if (current.StartWindow)
                    Notifyicon_LeftMouseButtonDoubleClick(null, null);
                if (csgoDir == null)
                    throw new DirectoryNotFoundException(Languages.Strings.ResourceManager.GetString("exception_csgonotfound")/*"Couldn't find CS:GO directory"*/);
                integrationPath = csgoDir + "game\\csgo\\cfg\\gamestate_integration_csauto.cfg";
                InitializeGSIConfig();
                windowSource = PresentationSource.FromVisual(this) as HwndSource;
                windowSource.AddHook(WndProc);
                //InitializeGameStateLaunchOption();
                //InitializeNetConLaunchOption();
            }
            catch (Exception ex)
            {
                Type type = ex.GetType();
                if (type == typeof(WriteException) ||
                    type == typeof(DirectoryNotFoundException))
                {
                    //autoReloadMenu.IsEnabled = false;
                    //autoBuyMenu.IsEnabled = false;
                    //autoPauseResumeSpotify.IsEnabled = false;
                    //discordMenu.IsEnabled = false;
                    hadError = true;
                }
                MessageBox.Show($"{ex.Message}", Languages.Strings.ResourceManager.GetString("title_warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            NativeMethods.OptimizeMemory();
        }

        //private void InitializeNetConLaunchOption()
        //{
        //    try
        //    {
        //        Steam.GetLaunchOptions(730, out string launchOpt);
        //        if (launchOpt != null && !HasNetCon(launchOpt))
        //            Steam.SetLaunchOptions(730, launchOpt + $" -netconport {NETCON_PORT}");
        //        else if (launchOpt == null)
        //            Steam.SetLaunchOptions(730, $"-netconport {NETCON_PORT}");
        //        else
        //            Log.WriteLine($"Already has \'-netconport {NETCON_PORT}\' in launch options.");
        //    }
        //    catch
        //    {
        //        throw new WriteException($"Couldn't add '-netconport {NETCON_PORT}' to launch options\n" +
        //        "please add it manually");
        //    }
        //}
        //private bool HasNetCon(string launchOpt)
        //{
        //    string[] split = launchOpt.Split(' ');
        //    for (int i = 0; i < split.Length; i++)
        //    {
        //        if (split[i].Trim() == "-netconport")
        //            if (i + 1 < split.Length && split[i + 1].Trim() == NETCON_PORT)
        //                return true;
        //    }
        //    return false;
        //}
        //private void InitializeGameStateLaunchOption()
        //{
        //    try
        //    {
        //        Steam.GetLaunchOptions(730, out string launchOpt);
        //        if (launchOpt != null && !HasGSILaunchOption(launchOpt))
        //            Steam.SetLaunchOptions(730, launchOpt + " -gamestateintegration");
        //        else if (launchOpt == null)
        //            Steam.SetLaunchOptions(730, "-gamestateintegration");
        //        else
        //            Log.WriteLine("Already has \'-gamestateintegration\' in launch options.");
        //    }
        //    catch
        //    {
        //        throw new WriteException("Couldn't add -gamestateintegration to launch options\n" +
        //        "please refer the the FAQ at the git hub page");
        //    }
        //}

        private void InitializeTimer()
        {
            appTimer.Interval = TimeSpan.FromMilliseconds(1000);
            appTimer.Tick += new EventHandler(TimerCallback);
            appTimer.Start();
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon.Icon = Properties.Resources.main;
            notifyIcon.Tip = "CSAuto";
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
                Log.WriteLine("|MainApp.cs| CSAuto was never launched, initializing 'gamestate_integration_csauto.cfg'");
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
                    Log.WriteLine("|MainApp.cs| Different 'gamestate_integration_csauto.cfg' was found, installing correct 'gamestate_integration_csauto.cfg'");
                }
            }
        }
        void AutoCheckUpdate()
        {
            new Thread(() =>
            {
                try
                {
                    Log.WriteLine("|MainApp.cs| Auto Checking for Updates");
                    string latestVersion = Github.GetWebInfo($"https://raw.githubusercontent.com/MurkyYT/CSAuto/{ONLINE_BRANCH_NAME}/Data/version");
                    Log.WriteLine($"|MainApp.cs| The latest version is {latestVersion}");
                    if (latestVersion == VER)
                    {
                        Log.WriteLine("|MainApp.cs| Latest version installed");
                    }
                    else
                    {
                        Log.WriteLine($"|MainApp.cs| Newer version found ({latestVersion}), current version is {VER}");
                        MessageBoxResult result = MessageBox.Show(string.Format(Languages.Strings.ResourceManager.GetString("msgbox_newerversion"), latestVersion), Languages.Strings.ResourceManager.GetString("title_update"), MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                        {
                            Log.WriteLine("|MainApp.cs| Launching updater");
                            try
                            {
                                string path = Path.GetTempPath() + "CSAutoUpdate";
                                if (Directory.Exists(path))
                                    Directory.Delete(path, true);
                                Directory.CreateDirectory(path);
                                File.Copy(Log.WorkPath + "\\updater.exe", path + "\\updater.exe");
                                Process.Start(path + "\\updater.exe", $"{Log.WorkPath} https://github.com/murkyyt/csauto/releases/latest/download/CSAuto_Portable.zip CSAuto.exe \"{current.Args} {(guiWindow == null ? "" : "--show")} --restart\" .portable");
                                Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
                            }
                            catch
                            {
                                Process.Start("https://github.com/MurkyYT/CSAuto/releases/latest");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"|MainApp.cs| Couldn't check for updates - '{ex.Message}'");
                }
            }).Start();
        }
        //private bool HasGSILaunchOption(string launchOpt)
        //{
        //    string[] split = launchOpt.Split(' ');
        //    for (int i = 0; i < split.Length; i++)
        //    {
        //        if (split[i].Trim() == "-gamestateintegration")
        //            return true;
        //    }
        //    return false;
        //}

        internal void Notifyicon_LeftMouseButtonDoubleClick(object sender, NotifyIconLibrary.Events.MouseLocationEventArgs e)
        {
            //open debug menu
            if (guiWindow == null)
            {
                guiWindow = new GUIWindow();
                Log.debugWind = guiWindow;
                guiWindow.Show();
                guiWindow.Activate();
            }
            else
            {
                guiWindow.Activate();
            }
        }
    }
}