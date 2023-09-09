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
        public const string VER = "2.0.0-beta.2";
        const string GAME_PROCCES_NAME = "cs2";
        const string DEBUG_REVISION = "";
        const string GAMESTATE_PORT = "11523";
        const string NETCON_PORT = "21823";
        const string TIMEOUT = "5.0";
        const string BUFFER = "0.1";
        const string THROTTLE = "0.0";
        const string HEARTBEAT = "10.0";
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
        readonly string[] AVAILABLE_MAP_ICONS;
        /// <summary>
        /// Publics
        /// </summary>
        public GUIWindow debugWind = null;
        public List<DiscordRPCButton> discordRPCButtons;
        /// <summary>
        /// Readonly
        /// </summary>
        readonly NotifyIconWrapper notifyIcon = new NotifyIconWrapper();
        readonly ContextMenu exitcm = new ContextMenu();
        readonly DispatcherTimer appTimer = new DispatcherTimer();
        readonly DispatcherTimer acceptButtonTimer = new DispatcherTimer();
        readonly Color BUTTON_COLOR = Color.FromArgb(77, 176, 81);
        readonly Color ACTIVE_BUTTON_COLOR = Color.FromArgb(91, 204, 95);
        /// <summary>
        /// Privates
        /// </summary>
        private DiscordRpcClient RPCClient;
        private string integrationPath = null;
        private string IN_LOBBY_STATE = "Chilling in lobby";
        /// <summary>
        /// Members
        /// </summary>
        Point csResolution = new Point();
        GameState GameState = new GameState(null);
        GameStateListener GameStateListener;
        NetCon netCon = null;
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
        Thread bombTimerThread = null;
        bool hadError = false;
        DXGICapture DXGIcapture = new DXGICapture();
        public ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }
        public ImageSource ToImageSource(IntPtr handle)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHBitmap(
                handle,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return imageSource;
        }
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                discordRPCButtons = DiscordRPCButtonSerializer.Deserialize();
                Application.Current.Exit += Current_Exit;
                AVAILABLE_MAP_ICONS = Properties.Resources.AVAILABLE_MAPS_STRING.Split(',');
                CSGOFriendCode.Encode("76561198341800115");
                InitializeDiscordRPC();
                CheckForDuplicates();
                GameStateListener = new GameStateListener(ref GameState, GAMESTATE_PORT);
                GameStateListener.OnReceive += GameStateListener_OnReceive;
                //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                InitializeContextMenu();
#if !DEBUG
                MakeSureStartupIsOn();
#endif
                Top = -1000;
                Left = -1000;
                IN_LOBBY_STATE = FormatString(Properties.Settings.Default.lobbyState, GameState);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{AppLanguage.Get("error_startup1")}\n'{ex.Message}'\n{AppLanguage.Get("error_startup2")}", AppLanguage.Get("title_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void GameStateListener_OnReceive(object sender, EventArgs e)
        {
            try
            {
                if (!RPCClient.IsInitialized && Properties.Settings.Default.enableDiscordRPC)
                {
                    InitializeDiscordRPC();
                    Log.WriteLine("DiscordRpc.Initialize();");
                }
                else if (RPCClient.IsInitialized && !Properties.Settings.Default.enableDiscordRPC)
                {
                    RPCClient.Deinitialize();
                    Log.WriteLine("DiscordRpc.Shutdown();");
                }
                Activity? activity = GameState.Player.CurrentActivity;
                Phase? currentMatchState = GameState.Match.Phase;
                Phase? currentRoundState = GameState.Round.Phase;
                BombState? currentBombState = GameState.Round.Bombstate;
                Weapon currentWeapon = GameState.Player.ActiveWeapon;
                if (debugWind != null)
                    debugWind.UpdateText(GameState.JSON);
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
                if (GameState.Match.Map != null && (inLobby == true || inLobby == null))
                {
                    inLobby = false;
                    Log.WriteLine($"Player loaded on map {GameState.Match.Map} in mode {GameState.Match.Mode}");
                    RPCClient.SetPresence(new RichPresence()
                    {
                        Details = FormatString(Properties.Settings.Default.inGameDetails, GameState),
                        State = FormatString(Properties.Settings.Default.inGameState, GameState),
                        Party = new Party() { ID = "", Size = 0, Max = 0 },
                        Assets = new Assets()
                        {
                            LargeImageKey = AVAILABLE_MAP_ICONS.Contains(GameState.Match.Map) ? $"map_icon_{GameState.Match.Map}" : "cs2_icon",
                            LargeImageText = GameState.Match.Map,
                            SmallImageKey = null,
                            SmallImageText = null
                        },
                        Timestamps = new Timestamps()
                        {
                            Start = UnixTimeStampToDateTime(GameState.Timestamp),
                            End = null
                        },
                        Buttons = GetDiscordRPCButtons()
                    });
                    if (Properties.Settings.Default.mapNotification)
                        SendMessageToServer($"<MAP>{AppLanguage.Get("server_loadedmap")} {GameState.Match.Map} {AppLanguage.Get("server_mode")} {GameState.Match.Mode}");
                    if (DXGIcapture.Enabled)
                    {
                        DXGIcapture.DeInit();
                        Log.WriteLine("Deinit DXGI Capture");
                    }
                }
                else if (GameState.Match.Map == null && (inLobby == false || inLobby == null))
                {
                    inLobby = true;
                    IN_LOBBY_STATE = FormatString(Properties.Settings.Default.lobbyState, GameState);
                    Log.WriteLine($"Player is back in main menu");
                    RPCClient.SetPresence(new RichPresence()
                    {
                        Details = FormatString(Properties.Settings.Default.lobbyDetails, GameState),
                        State = IN_LOBBY_STATE,
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
                            Start = UnixTimeStampToDateTime(GameState.Timestamp),
                            End = null
                        },
                        Buttons = GetDiscordRPCButtons()
                    });
                    if (Properties.Settings.Default.lobbyNotification)
                        SendMessageToServer($"<LBY>{AppLanguage.Get("server_loadedlobby")}");
                    if (!DXGIcapture.Enabled)
                    {
                        DXGIcapture.Init();
                        Log.WriteLine("Init DXGI Capture");
                    }
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
                SendMessageToServer($"<GSI>{GameState.JSON}{inGame}", onlyServer: true);
                //Log.WriteLine($"Got info from GSI\nActivity:{activity}\nCSGOActive:{csgoActive}\nInGame:{inGame}\nIsSpectator:{IsSpectating(JSON)}");
            }
            catch (Exception ex)
            {
                Log.WriteLine("Error happend while getting GSI Info\n" + ex);
            }
        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp);
            return dateTime;
        }
        public void NetConEstablishConnection()
        {
            netCon = new NetCon("127.0.0.1", int.Parse(NETCON_PORT));
            netCon.MatchFound += NetCon_MatchFound;
            acceptButtonTimer.Interval = TimeSpan.FromMilliseconds(1000);
            acceptButtonTimer.Tick += AcceptButtonTimer_Tick;
        }

        private void AcceptButtonTimer_Tick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.autoAcceptMatch && !inGame && !acceptedGame && csActive)
                _ = AutoAcceptMatchAsync();
            else if (inGame || !Properties.Settings.Default.autoAcceptMatch)
                acceptButtonTimer.Stop();
        }

        private void NetCon_MatchFound(object sender, EventArgs e)
        {
            Log.WriteLine("Match Found!");
            if (acceptButtonTimer.IsEnabled) acceptButtonTimer.Stop();
            if (!acceptButtonTimer.IsEnabled && !acceptedGame && !inGame)
            {
                Log.WriteLine("Starting searching for accept button...");
                acceptButtonTimer.Start();
            }
        }

        public void NetConCloseConnection()
        {
            if (netCon != null)
            {
                netCon.Close();
                netCon = null;
            }
        }
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
                    .Replace("{SteamID}", gameState.MySteamID);
            }
            return original;
        }

        private void MakeSureStartupIsOn()
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

        public string TelegramSendMessage(string text)
        {
            try
            {
                string urlString = $"https://api.telegram.org/bot{APIKeys.APIKeys.TelegramBotToken}/sendMessage?chat_id={Properties.Settings.Default.telegramChatId}&text={text}";

                WebClient webclient = new WebClient();

                return webclient.DownloadString(urlString);
            }
            catch (WebException ex) { if (!ex.Message.Contains("(429)") && ex.Message != "Unable to connect to the remote server") { MessageBox.Show($"{AppLanguage.Get("error_telegrammessage")}\n'{ex.Message}'\n'{text}'", AppLanguage.Get("title_error"), MessageBoxButton.OK, MessageBoxImage.Error); } return null; }
        }
        private void Current_Exit(object sender, ExitEventArgs e)
        {
            Exited();
        }

        private void InitializeContextMenu()
        {
            MenuItem exit = new MenuItem
            {
                Header = AppLanguage.Get("menu_exit")
            };
            MenuItem options = new MenuItem
            {
                Header = AppLanguage.Get("menu_options")
            };
            exit.Click += Exit_Click;
            options.Click += Options_Click;
            MenuItem about = new MenuItem
            {
                Header = $"{typeof(MainWindow).Namespace} - {VER}{(DEBUG_REVISION == "" ? "" : $" REV {DEBUG_REVISION}")}",
                IsEnabled = false,
                Icon = new System.Windows.Controls.Image
                {
                    Source = ToImageSource(System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location))
                }
            };
            MenuItem checkForUpdates = new MenuItem
            {
                Header = AppLanguage.Get("menu_checkforupdates")
            };
            checkForUpdates.Click += CheckForUpdates_Click;
            exitcm.Items.Add(about);
            exitcm.Items.Add(new Separator());
            exitcm.Items.Add(options);
            exitcm.Items.Add(new Separator());
            exitcm.Items.Add(checkForUpdates);
            exitcm.Items.Add(exit);
            exitcm.StaysOpen = false;
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            Notifyicon_LeftMouseButtonDoubleClick(null, null);
        }

        private void InitializeDiscordRPC()
        {
            RPCClient = new DiscordRpcClient(APIKeys.APIKeys.DiscordAppID);
            //Subscribe to events
            RPCClient.OnReady += (sender, e) =>
            {
                Log.WriteLine($"Received Discord RPC Ready! {e.User.Username}");
            };
            RPCClient.Initialize();
        }

        private DiscordRPC.Button[] GetDiscordRPCButtons()
        {
            DiscordRPC.Button[] res = new DiscordRPC.Button[1 + discordRPCButtons.Count];
            res[0] = new DiscordRPC.Button() { Label = "CSAuto", Url = "https://github.com/MurkyYT/CSAuto" };
            for (int i = 1; i < res.Length; i++)
            {
                res[i] = new DiscordRPC.Button() { Label = discordRPCButtons[i - 1].Label, Url = FormatString(discordRPCButtons[i - 1].Url,GameState) };
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
                    Log.WriteLine("Checking for updates");
                    string latestVersion = Github.GetLatestStringTag("murkyyt", "csauto");
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
        private void StartBombTimer()
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long ms = (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
            long result = ms / 1000;
            int diff = (int)(GameState.Timestamp - result);
            SendMessageToServer($"<BMB>{AppLanguage.Get("server_bombplanted")} ({DateTime.Now})", onlyTelegram: true);
            bombTimerThread = new Thread(() =>
            {
                for (int seconds = BOMB_SECONDS - diff; seconds >= 0; seconds--)
                {
                    SendMessageToServer($"<BMB>{AppLanguage.Get("server_timeleft")} {seconds}", onlyServer: true);
                    Thread.Sleep(BOMB_TIMER_DELAY);
                }
                bombTimerThread = null;
            });
            bombTimerThread.Start();
        }

        private void UpdateDiscordRPC()
        {
            if (csRunning && inGame)
            {
                RPCClient.SetPresence(new RichPresence()
                {
                    Details = RPCClient.CurrentPresence.Details,
                    State = FormatString(Properties.Settings.Default.inGameState, GameState),
                    Party = new Party(),
                    Assets = new Assets()
                    {
                        LargeImageKey = RPCClient.CurrentPresence.Assets.LargeImageKey,
                        LargeImageText = RPCClient.CurrentPresence.Assets.LargeImageText,
                        SmallImageKey = GameState.IsSpectating ? "gotv_icon" : GameState.IsDead ? "spectator" : GameState.Player.Team.ToString().ToLower(),
                        SmallImageText = GameState.IsSpectating ? "Watching CSTV" : GameState.IsDead ? "Spectating" : GameState.Player.Team == Team.T ? "Terrorist" : "Counter-Terrorist"
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
                        { ID= lobbyid == "0" ? "0" : lobbyid ,Max= int.Parse(partyMax) ,Size= int.Parse(partysize) },
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
                Log.WriteLine("DiscordRpc.Shutdown();");
            }
        }

        private void AutoPauseResumeSpotify()
        {
            if (GameState.Player.CurrentActivity == Activity.Playing)
            {
                if (GameState.Player.Health > 0 && GameState.Player.SteamID == GameState.MySteamID && Spotify.IsPlaying())
                {
                    Spotify.Pause();
                }
                else if (!Spotify.IsPlaying() && GameState.Player.SteamID != GameState.MySteamID ||
                    (GameState.Player.Health <= 0 && GameState.Player.SteamID == GameState.MySteamID))
                {
                    Spotify.Resume();
                }
            }
            else if (!Spotify.IsPlaying() && GameState.Player.CurrentActivity != Activity.Textinput)
            {
                Spotify.Resume();
            }

        }
        private void SendMessageToServer(string message, bool onlyTelegram = false, bool onlyServer = false)
        {
            new Thread(() =>
            {
                if (Properties.Settings.Default.telegramChatId != "" && !onlyServer)
                    TelegramSendMessage(message.Substring(5));
                if (Properties.Settings.Default.phoneIpAddress == "" || !Properties.Settings.Default.mobileAppEnabled || onlyTelegram)
                    return;
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
                Process[] prcs = new Process[0];
                if (csProcess == null)
                    prcs = Process.GetProcessesByName(GAME_PROCCES_NAME);
                if (csProcess == null && prcs.Length > 0)
                {
                    csProcess = prcs[0];
                    csRunning = true;
                    csProcess.Exited += CsProcess_Exited;
                    csProcess.EnableRaisingEvents = true;
                    if (!GameStateListener.ServerRunning)
                    {
                        Log.WriteLine("Starting GSI Server");
                        GameStateListener.StartGSIServer();
                    }
                    if (steamAPIServer == null && Properties.Settings.Default.enableLobbyCount)
                    {
                        steamAPIServer = new Process() { StartInfo = { FileName = "steamapi.exe" } };
                        steamAPIServer.Start();
                    }
                    if (!DXGIcapture.Enabled)
                    {
                        DXGIcapture.Init();
                        Log.WriteLine("Init DXGI Capture");
                    }
                }
                else if (!csRunning)
                {
                    if (RPCClient.IsInitialized)
                    {
                        RPCClient.Deinitialize();
                        Log.WriteLine("DiscordRpc.Shutdown();");
                    }
                    if (GameState.Timestamp != 0)
                    {
                        GameState.UpdateJson(null);
                    }
                    if (GameStateListener.ServerRunning)
                    {
                        Log.WriteLine("Stopping GSI Server");
                        GameStateListener.StopGSIServer();
                        NetConCloseConnection();
                        SendMessageToServer("<CLS>", onlyServer: true);
                    }
                    if (steamAPIServer != null)
                    {
                        steamAPIServer.Kill();
                        steamAPIServer = null;
                    }
                    if (DXGIcapture.Enabled)
                    {
                        DXGIcapture.DeInit();
                        Log.WriteLine("Deinit DXGI Capture");
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
            inLobby = false;
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
                //if (Properties.Settings.Default.oldAutoBuy)
                //    DisableTextinput();
                if (lastActivity != Activity.Textinput)
                {
                    Log.WriteLine("Auto buying armor");
                    PressKey(Keyboard.DirectXKeyStrokes.DIK_B);
                    Thread.Sleep(100);
                    PressKeys(new Keyboard.DirectXKeyStrokes[]
                    {
                Keyboard.DirectXKeyStrokes.DIK_1,
                Keyboard.DirectXKeyStrokes.DIK_1,
                Keyboard.DirectXKeyStrokes.DIK_1,
                Keyboard.DirectXKeyStrokes.DIK_2,
                Keyboard.DirectXKeyStrokes.DIK_B
                    });
                    //netCon.SendCommand("buy vest");
                    //netCon.SendCommand("buy vesthelm");
                }
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
                //if (Properties.Settings.Default.oldAutoBuy)
                //    DisableTextinput();
                if (lastActivity != Activity.Textinput)
                {
                    Log.WriteLine("Auto buying defuse kit");
                    PressKey(Keyboard.DirectXKeyStrokes.DIK_B);
                    Thread.Sleep(100);
                    PressKeys(new Keyboard.DirectXKeyStrokes[]
                    {
                Keyboard.DirectXKeyStrokes.DIK_1,
                Keyboard.DirectXKeyStrokes.DIK_4,
                Keyboard.DirectXKeyStrokes.DIK_B
                    });
                    //netCon.SendCommand("buy defuser");
                }
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
                    Log.WriteLine("Auto reloading");
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
                        Log.WriteLine($"Continue spraying ({weaponName} - {weaponType})");
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
            Log.WriteLine($"Left clicked at X:{xpos} Y:{ypos}");
        }
        private async Task AutoAcceptMatchAsync()
        {
            IntPtr _handle = DXGIcapture.GetCapture();
            if (_handle == IntPtr.Zero)
                return;
            using (Bitmap bitmap = Image.FromHbitmap(_handle))
            {
                //    using (Graphics g = Graphics.FromImage(bitmap))
                //    {
                //        g.CopyFromScreen(new Point(
                //            csResolution.X / 2,
                //            0),
                //            Point.Empty,
                //            new System.Drawing.Size(1, csResolution.Y));
                //    }
                if (Properties.Settings.Default.saveDebugFrames)
                {
                    Directory.CreateDirectory($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\DEBUG\\FRAMES");
                    bitmap.Save($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\DEBUG\\FRAMES\\Frame{frame++}.jpeg", ImageFormat.Jpeg);
                }
                if(debugWind != null)
                {
                    debugWind.latestCapturedFrame.Source = ToImageSource(_handle);
                }
                bool found = false;
                int count = 0;
                for (int y = bitmap.Height - 1; y >= 0 && !found && !acceptedGame; y--)
                {
                    Color pixelColor = bitmap.GetPixel(csResolution.X/2, y);
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
                            if (CheckIfAccepted(bitmap, Y))
                            {
                                acceptedGame = true;
                                if (acceptButtonTimer.IsEnabled)
                                    acceptButtonTimer.Stop();
                                acceptedGame = await MakeFalse(ACCEPT_BUTTON_DELAY);
                            }
                        }
                        count++;
                    }
                }
                NativeMethods.DeleteObject(_handle);
            }
            //}
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
            exitcm.IsOpen = true;
            Activate();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Exited()
        {
            if(notifyIcon != null)
                notifyIcon.Close();

            if (GameStateListener != null)
                GameStateListener.StopGSIServer();
            
            if (steamAPIServer != null && !steamAPIServer.HasExited)
                steamAPIServer.Kill();

            if(RPCClient != null)
                RPCClient.Dispose();

            DiscordRPCButtonSerializer.Serialize(discordRPCButtons);

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
                    throw new DirectoryNotFoundException(AppLanguage.Get("exception_csgonotfound")/*"Couldn't find CS:GO directory"*/);
                integrationPath = csgoDir + "game\\csgo\\cfg\\gamestate_integration_csauto.cfg";
                InitializeGSIConfig();
                //InitializeGameStateLaunchOption();
                //InitializeNetConLaunchOption();
#if !DEBUG
                if (Properties.Settings.Default.autoCheckForUpdates)
                    AutoCheckUpdate();
#endif
                if (Properties.Settings.Default.connectedNotification)
                    SendMessageToServer($"<CNT>{AppLanguage.Get("server_computer")} {Environment.MachineName} ({GetLocalIPAddress()}) {AppLanguage.Get("server_online")}");
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
                MessageBox.Show($"{ex.Message}", AppLanguage.Get("title_error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeNetConLaunchOption()
        {
            try
            {
                Steam.GetLaunchOptions(730, out string launchOpt);
                if (launchOpt != null && !HasNetCon(launchOpt))
                    Steam.SetLaunchOptions(730, launchOpt + $" -netconport {NETCON_PORT}");
                else if (launchOpt == null)
                    Steam.SetLaunchOptions(730, $"-netconport {NETCON_PORT}");
                else
                    Log.WriteLine($"Already has \'-netconport {NETCON_PORT}\' in launch options.");
            }
            catch
            {
                throw new WriteException($"Couldn't add '-netconport {NETCON_PORT}' to launch options\n" +
                "please add it manually");
            }
        }
        private bool HasNetCon(string launchOpt)
        {
            string[] split = launchOpt.Split(' ');
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Trim() == "-netconport")
                    if (i + 1 < split.Length && split[i + 1].Trim() == NETCON_PORT)
                        return true;
            }
            return false;
        }
        private void InitializeGameStateLaunchOption()
        {
            try
            {
                Steam.GetLaunchOptions(730, out string launchOpt);
                if (launchOpt != null && !HasGSILaunchOption(launchOpt))
                    Steam.SetLaunchOptions(730, launchOpt + " -gamestateintegration");
                else if (launchOpt == null)
                    Steam.SetLaunchOptions(730, "-gamestateintegration");
                else
                    Log.WriteLine("Already has \'-gamestateintegration\' in launch options.");
            }
            catch
            {
                throw new WriteException("Couldn't add -gamestateintegration to launch options\n" +
                "please refer the the FAQ at the git hub page");
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
            notifyIcon.Tip = "CSAuto - CS2 Automation";
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
        private bool HasGSILaunchOption(string launchOpt)
        {
            string[] split = launchOpt.Split(' ');
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Trim() == "-gamestateintegration")
                    return true;
            }
            return false;
        }

        private void Notifyicon_LeftMouseButtonDoubleClick(object sender, NotifyIconLibrary.Events.MouseLocationEventArgs e)
        {
            //open debug menu
            if (debugWind == null)
            {
                debugWind = new GUIWindow();
                Log.debugWind = debugWind;
                debugWind.Show();
                debugWind.Activate();
            }
            else
            {
                debugWind.Activate();
            }
        }
    }
}