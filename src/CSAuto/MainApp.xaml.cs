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
using Commands = CSAuto.Shared.NetworkTypes.Commands;
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
using System.Globalization;
#endregion
namespace CSAuto
{
    /// <summary>
    /// Main logic for CSAuto app
    /// </summary>
    public partial class MainApp : Window
    {
        #region Constants
        public const string VER = "2.2.1";
        public const string FULL_VER = VER + (DEBUG_REVISION == "" ? "" : " REV " + DEBUG_REVISION);
        const string DEBUG_REVISION = "3";
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
        public string integrationPath = null;
        public string bindCfgPath = null;
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
        private string currentMapIcon = null;
        private Color BUTTON_COLOR;
        private Color ACTIVE_BUTTON_COLOR;
        private string localIp;
        private bool serverRunning = false;
        private DateTime startTimeStamp = DateTime.MinValue;
        #endregion
        #region Members
        RECT csResolution = new RECT();
        //Only workshop tools have netconport? https://github.com/ValveSoftware/csgo-osx-linux/issues/3603#issuecomment-2163695087
        //NetCon netCon = null;
        int frame = 0;
        int lastRound = 0;
        bool csRunning = false;
        bool autoBuy = true;
        bool inGame = false;
        bool csActive = false;
        bool? inLobby = null;
        Activity? lastActivity;
        Phase? matchState;
        Phase? roundState;
        BombState? bombState;
        Weapon weapon;
        Process steamAPIServer = null;
        Process csProcess = null;
        Process originalProcess = null;
        Thread bombTimerThread = null;
        HwndSource windowSource;
        ContextMenu exitCm;
        TcpListener server;
        Thread serverThread;
        internal List<TcpClient> clients;
        Dictionary<TcpClient, DateTime> lastKeepAlive;
        DateTime lastGameStateSend = DateTime.Now;
        BindCommandSender commandSender;
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
                throw new ArgumentNullException(nameof(bitmap));
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
                if (Properties.Settings.Default.darkTheme)
                    ThemeManager.Current.ChangeTheme(current, $"Dark.{Properties.Settings.Default.currentColor}");
                else
                    ThemeManager.Current.ChangeTheme(current, $"Light.{Properties.Settings.Default.currentColor}");

                Task.Run(() =>
                {
                    BUTTON_COLORS = LoadButtonColors();
                    UpdateColors();
                });
                discordRPCButtons = DiscordRPCButtonSerializer.Deserialize();
                Application.Current.Exit += Current_Exit;
                // Try to encode my own steamid to see if its correct
                if (CSGOFriendCode.Encode("76561198341800115") != "S4N2P-NZFJ")
                    throw new Exception("FriendCode Error");
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
                MessageBox.Show(string.Format(Languages.Strings.ResourceManager.GetString("error_startup"), ex.Message), Languages.Strings.ResourceManager.GetString("title_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Error(
                    $"{ex.Message}\n" +
                    $"StackTrace:{ex.StackTrace}\n" +
                    $"Source: {ex.Source}\n" +
                    $"Inner Exception: {ex.InnerException}");
                Application.Current.Shutdown();
            }
        }
        public async void ServerThread()
        {
            try
            {
                server.Start();
                Log.WriteLine($"|MainApp.cs| [SERVER]: Started listening at {server.LocalEndpoint}");
                byte[] keepAliveBuf = BitConverter.GetBytes(1).ToList().Append((byte)Commands.KeepAlive).ToArray();
                while (serverRunning)
                {
                    if (server.Pending())
                    {
                        TcpClient client = await server.AcceptTcpClientAsync();
                        clients.Add(client);
                        Log.WriteLine($"|MainApp.cs| [SERVER]: Accepted tcp client {client.Client.RemoteEndPoint}");
                        await client.GetStream().WriteAsync(keepAliveBuf, 0, keepAliveBuf.Length);
                        lastKeepAlive[client] = DateTime.Now;
                        guiWindow?.Dispatcher.InvokeAsync(() => { guiWindow?.ClientsListBox?.Items.Add(client.Client.RemoteEndPoint); });
                    }
                    if (clients != null)
                    {
                        lock (clients)
                        {
                            for (int i = clients.Count - 1; i >= 0; i--)
                            {
                                TcpClient client = clients[i];
                                try
                                {
                                    if (!client.Connected)
                                    {
                                        DeleteClient(client);
                                        continue;
                                    }
                                    if (client.Available > 0)
                                    {
                                        ReadData(client);
                                        continue;
                                    }
                                    if (DateTime.Now - lastKeepAlive[client] > TimeSpan.FromSeconds(10))
                                    {

                                        client.GetStream().WriteAsync(keepAliveBuf, 0, keepAliveBuf.Length);
                                        lastKeepAlive[client] = DateTime.Now;
                                    }
                                }
                                catch { DeleteClient(client); }
                            }
                        }
                    }
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex) { Log.WriteLine($"|MainApp.cs| Error ocurred in server thread\n\t'{ex}'"); serverThread = null; server?.Stop(); server = null; }
            server?.Stop();
            server = null;
            Log.WriteLine("|MainApp.cs| Stopped server");
        }
        private void DeleteClient(TcpClient client)
        {
            clients.Remove(client);
            lastKeepAlive.Remove(client);
            Log.WriteLine($"|MainApp.cs| [SERVER]: Tcp client disconnected {client.Client.RemoteEndPoint}");
            guiWindow?.Dispatcher.InvokeAsync(() => { guiWindow?.ClientsListBox?.Items.Remove(client.Client.RemoteEndPoint); });
        }
        private async void ReadData(TcpClient client)
        {
            byte[] buffer = new byte[4];
            await client.GetStream().ReadAsync(buffer, 0, 4);
            int length = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[length];
            await client.GetStream().ReadAsync(buffer, 0, length);
            var sb = new StringBuilder("{ ");
            foreach (var b in buffer)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            Log.WriteLine($"|MainApp.cs| [SERVER]: Received {sb} with length {length} from {client.Client.RemoteEndPoint}");
        }

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

                if (App.Current == null)
                    return new Color[2];

                (App.Current as App).settings.Set("ButtonColors", data);

                return SplitColorsLines(lines);
            }
            catch
            {
                Log.WriteLine("|MainApp.cs| Couldn't load colors from web, trying to load latest loaded colors");
                if ((App.Current as App).settings.KeyExists("ButtonColors"))
                {
                    string data = (App.Current as App).settings["ButtonColors"];

                    if (data != "")
                    {
                        string[] lines = data.Split(new char[] { '\n' });
                        return SplitColorsLines(lines);
                    }
                }

                Log.WriteLine("|MainApp.cs| Couldn't load colors at all");
                MessageBox.Show(Languages.Strings.ResourceManager.GetString("error_loadcolors"),
                    Languages.Strings.ResourceManager.GetString("title_error"), MessageBoxButton.OK, MessageBoxImage.Error);

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
                    var splitString = lines[i].Split(',');

                    var splitInts = splitString.Select(item => int.Parse(item)).ToArray();

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
                            SendMessageToClients(Languages.Strings.ResourceManager.GetString("server_bombdefuse"), command: Commands.Bomb);
                            break;
                        case BombState.Exploded:
                            SendMessageToClients(Languages.Strings.ResourceManager.GetString("server_bombexplode"), command: Commands.Bomb);
                            break;
                    }
                }
                if (gameState.Match.Map != null && (inLobby == true || inLobby == null))
                {
                    inLobby = false;
                    Log.WriteLine($"|MainApp.cs| Player loaded on map {gameState.Match.Map} in mode {gameState.Match.Mode}");
                    currentMapIcon = CSGOMap.GetMapIcon(gameState.Match.Map);
                    if (Properties.Settings.Default.mapNotification)
                        SendMessageToClients(string.Format(Languages.Strings.ResourceManager.GetString("server_loadedmap"), gameState.Match.Map, gameState.Match.Mode), command: Commands.LoadedOnMap);
                    if (DXGIcapture.Enabled)
                    {
                        DXGIcapture.DeInit();
                        Log.WriteLine("|MainApp.cs| Deinit DXGI Capture");
                    }
                    startTimeStamp = UnixTimeStampToDateTime(gameState.Timestamp);
                }
                else if (gameState.Match.Map == null &&
                    (inLobby == false || inLobby == null))
                {
                    inLobby = true;
                    currentMapIcon = null;
                    Log.WriteLine($"|MainApp.cs| Player is back in main menu");
                    if (Properties.Settings.Default.lobbyNotification)
                    {
                        SendMessageToClients(Languages.Strings.ResourceManager.GetString("server_loadedlobby"), command: Commands.LoadedInLobby);
                    }
                    if (!DXGIcapture.Enabled && !Properties.Settings.Default.oldScreenCaptureWay)
                    {
                        DXGIcapture.Init();
                        Log.WriteLine("|MainApp.cs| Init DXGI Capture");
                    }
                    startTimeStamp = UnixTimeStampToDateTime(gameState.Timestamp);
                }

                if (roundState == Phase.Freezetime && roundState != currentRoundState || lastRound != gameState.Round.CurrentRound)
                    autoBuy = true;

                lastRound = gameState.Round.CurrentRound;
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
                    if (lastActivity == Activity.Playing && csActive && Properties.Settings.Default.autoBuyEnabled && autoBuy)
                        AutoBuy();
                    if (Properties.Settings.Default.autoPausePlaySpotify)
                        AutoPauseResumeMusic();
                }

                UpdateDiscordRPC();

                if (DateTime.Now - lastGameStateSend > TimeSpan.FromSeconds(1))
                {
                    lastGameStateSend = DateTime.Now;
                    SendMessageToClients(gameState.JSON, onlyClients: true, command: Commands.GameState);
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
                List<BuyItem> items = current.buyMenu.GetItemsToBuy(gameState, MAX_ARMOR_AMOUNT_TO_REBUY);
                if (items.Count == 0 && !Properties.Settings.Default.autoBuyRebuy)
                {
                    autoBuy = false;
                    return;
                }
                List<string> names = current.buyMenu.GetWeaponsName(items, gameState);
                string command = "// Auto buy items:";
                foreach (string item in names)
                    command += $"\nbuy {item}";

                if (items.Count != 0)
                    commandSender.SendCommand(command);
            }
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
                    .Replace("{Deaths}", gameState.Player.Deaths.ToString())
                    .Replace("{MVPS}", gameState.Player.MVPS.ToString());
            }
            return original;
        }

        public string LimitLength(string str, int length)
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
                    rk.SetValue(appname, executablePath + " " + current.Args);
                    Log.WriteLine("|MainApp.cs| " + executablePath + " " + current.Args);
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
                Header = new TextBlock() { Text = $"{typeof(MainApp).Namespace} - {FULL_VER}", VerticalAlignment = VerticalAlignment.Center },
                IsEnabled = false,
                Icon = new System.Windows.Controls.Image
                {
                    Source = ToImageSource(System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)),
                    Margin = new Thickness(5, 5, 0, 0)
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
                    File.WriteAllText(saveFileDialog.FileName, current.settings.ToString(), Encoding.UTF8);
                    MessageBox.Show(string.Format(Languages.Strings.ResourceManager.GetString("file_savesucess"), saveFileDialog.FileName), Languages.Strings.ResourceManager.GetString("title_success"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
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
                Directory.CreateDirectory(Log.WorkPath + "\\debug\\discord");
            }
            catch { }
            RPCClient = new DiscordRpcClient(APIKeys.DISCORD_BOT_ID);
#if DEBUG
            File.Create(Log.WorkPath + "\\debug\\discord\\Debug_Log.txt").Close();
            RPCClient.Logger = new FileLogger(Log.WorkPath + "\\debug\\discord\\Debug_Log.txt", LogLevel.Trace);
#elif !DEBUG
            try
            {
                File.Create(Log.WorkPath + "\\debug\\discord\\Error_Log.txt").Close();
                RPCClient.Logger = new FileLogger(Log.WorkPath + "\\debug\\discord\\Error_Log.txt", LogLevel.Error);
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
                res[i] = new DiscordRPC.Button() { Label = discordRPCButtons[i - 1].Label, Url = FormatString(discordRPCButtons[i - 1].Url, gameState) };
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
                                File.Copy(Log.WorkPath + "\\bin\\updater.exe", path + "\\updater.exe");
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
            SendMessageToClients($"{Languages.Strings.ResourceManager.GetString("server_bombplanted")} ({DateTime.Now})", onlyTelegram: true, command: Commands.Bomb);
            bombTimerThread = new Thread(() =>
            {
                for (int seconds = BOMB_SECONDS - diff; seconds >= 0; seconds--)
                {
                    SendMessageToClients($"{Languages.Strings.ResourceManager.GetString("server_timeleft")} {seconds}", onlyClients: true, command: Commands.Bomb);
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
                        Details = LimitLength(FormatString(Properties.Settings.Default.inGameDetails, gameState), 128),
                        State = LimitLength(FormatString(Properties.Settings.Default.inGameState, gameState), 128),
                        Party = new Party() { ID = "", Size = 0, Max = 0 },
                        Assets = new Assets()
                        {
                            LargeImageKey = currentMapIcon ?? "cs2_icon",
                            LargeImageText = gameState.Match.Map,
                            SmallImageKey = gameState.IsSpectating ? "gotv_icon" : gameState.IsDead ? "spectator" : gameState.Player.Team.ToString().ToLower(),
                            SmallImageText = gameState.IsSpectating ? "Watching CSTV" : gameState.IsDead ? "Spectating" : gameState.Player.Team == Team.T ? "Terrorist" : "Counter-Terrorist"
                        },
                        Timestamps = new Timestamps()
                        {
                            Start = startTimeStamp,
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
                            Details = LimitLength(FormatString(Properties.Settings.Default.lobbyDetails, gameState), 128),
                            State = LimitLength(FormatString(Properties.Settings.Default.lobbyState, gameState), 128),
                            Party = new Party()
                            { ID = lobbyid == "0" ? "0" : lobbyid, Max = int.Parse(partyMax), Size = int.Parse(partysize) },
                            Assets = new Assets()
                            {
                                LargeImageKey = "cs2_icon",
                                LargeImageText = "Menu",
                                SmallImageKey = null,
                                SmallImageText = null
                            },
                            Timestamps = new Timestamps()
                            {
                                Start = startTimeStamp,
                                End = null
                            },
                            Buttons = GetDiscordRPCButtons()
                        });
                    }
                    else
                    {
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
                                Start = startTimeStamp,
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
            catch (NullReferenceException) { }
        }

        private void AutoPauseResumeMusic()
        {
            if (gameState.Player.CurrentActivity == Activity.Playing)
            {
                if (gameState.Player.Health > 0 && gameState.Player.SteamID == gameState.MySteamID)
                {
                    Music.Pause();
                }
                else if (gameState.Player.SteamID != gameState.MySteamID ||
                    (gameState.Player.Health <= 0 && gameState.Player.SteamID == gameState.MySteamID))
                {
                    Music.Resume();
                }
            }
            else if (gameState.Player.CurrentActivity != Activity.Textinput)
            {
                Music.Resume();
            }
        }
        private void SendMessageToClients(string message, bool onlyTelegram = false, bool onlyClients = false, Commands command = Commands.None)
        {

            new Thread(() =>
            {
                if (Properties.Settings.Default.telegramChatId != "" && !onlyClients)
                    Telegram.SendMessage(message, Properties.Settings.Default.telegramChatId,
                        Telegram.CheckToken(Properties.Settings.Default.customTelegramToken) ?
                        Properties.Settings.Default.customTelegramToken : Encoding.UTF8.GetString(Convert.FromBase64String(APIKeys.TELEGRAM_BOT_TOKEN + "==")));
                if (clients != null)
                {
                    lock (clients)
                    {
                        if (Properties.Settings.Default.phoneIpAddress == "" || !Properties.Settings.Default.mobileAppEnabled || onlyTelegram)
                            return;
                        try // Try connecting and send the message bytes  
                        {
                            foreach (TcpClient client in clients)
                            {
                                try
                                {
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
                                    client.GetStream().WriteAsync(buffer, 0, buffer.Length);
                                }
                                catch { }
                            }
                        }
                        catch (Exception ex) { Log.WriteLine(ex); }
                    }
                }
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
                                steamAPIServer = new Process() { StartInfo = { FileName = $"{Log.WorkPath}\\bin\\steamapi.exe" } };
                                if (!steamAPIServer.Start() || !File.Exists($"{Log.WorkPath}\\bin\\steamapi.exe"))
                                {
                                    Log.WriteLine("|MainApp.cs| Couldn't launch 'steamapi.exe'");
                                    steamAPIServer = null;
                                }
                            }
                            if (server == null)
                            {
                                clients = new List<TcpClient>();
                                lastKeepAlive = new Dictionary<TcpClient, DateTime>();
                                server = new TcpListener(IPAddress.Any, int.Parse(Properties.Settings.Default.serverPort));
                                serverRunning = true;
                                serverThread = new Thread(ServerThread);
                                serverThread.Start();
                            }
                            NativeMethods.OptimizeMemory();
                        }
                    }
                }
                else
                {
                    lock (csProcessLock)
                    {
                        csActive = NativeMethods.IsForegroundProcess((uint)csProcess.Id);
                        if (csActive)
                        {
                            bool success = NativeMethods.GetWindowRect(csProcess.MainWindowHandle, out RECT windSize);
                            if (success)
                                csResolution = windSize;
                            if (Properties.Settings.Default.autoAcceptMatch && !inGame)
                                AutoAcceptMatch();
                        }
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
            lock (csProcessLock)
            {
                if (Properties.Settings.Default.autoFocusOnCS &&
                    Properties.Settings.Default.autoAcceptMatch &&
                    csProcess != null && !csActive && inLobby == true &&
                    lParam == csProcess.MainWindowHandle && wParam == NativeMethods.HSHELL_FLASH)
                {
                    if (Properties.Settings.Default.focusBackOnOriginalWindow)
                        originalProcess = NativeMethods.GetForegroundProcess();
                    if (NativeMethods.BringToFront(csProcess.MainWindowHandle))
                        Log.WriteLine("|MainApp.cs| Switching to CS window");
                }
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
            if (ss.ReadString() == "I am the one true server!")
            {
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
                    SendMessageToClients(Languages.Strings.ResourceManager.GetString("server_gamecrash"), command: Commands.Crashed);
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
                    SendMessageToClients("", onlyClients: true, command: Commands.Clear);
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

                clients?.Clear();
                lastKeepAlive?.Clear();
                serverRunning = false;
                guiWindow?.Dispatcher.InvokeAsync(() => { guiWindow?.ClientsListBox?.Items.Clear(); });
                csProcess = null;
                inLobby = false;
                csRunning = false;

                if (Properties.Settings.Default.autoCloseCSAuto)
                    Dispatcher.Invoke(() => { Application.Current.Shutdown(); });

                if (File.Exists(bindCfgPath))
                    File.Delete(bindCfgPath);

                NativeMethods.OptimizeMemory();
            }
        }
        private void TryToAutoReload()
        {
            bool isMousePressed = (Input.GetKeyState(Input.VirtualKeyStates.VK_LBUTTON) < 0);
            if (!isMousePressed || weapon == null)
                return;
            try
            {
                int bullets = weapon.Bullets;
                WeaponType? weaponType = weapon.Type;
                string weaponName = weapon.Name;
                if (bullets == 0)
                {
                    commandSender.SendCommand("+reload");
                    commandSender.SendCommand("-attack");
                    Log.WriteLine("|MainApp.cs| Auto reloading");
                    commandSender.SendCommand("-reload");
                    if (Properties.Settings.Default.ContinueSpraying)
                    {
                        Thread.Sleep(50);
                        bool mousePressed = (Input.GetKeyState(Input.VirtualKeyStates.VK_LBUTTON) < 0);
                        if (mousePressed)
                        {
                            commandSender.SendCommand("+attack");
                            Log.WriteLine($"|MainApp.cs| Continue spraying ({weaponName} - {weaponType})");
                        }
                    }
                }
            }
            catch { return; }
        }

        private void LeftMouseDown(int x, int y)
        {
            if (Properties.Settings.Default.oldMouseInput)
            {
                Log.WriteLine("|MainApp.cs| Sending mouse down with mouse_event");
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN,
                            x,
                            y,
                            0, 0);
            }
            else
            {
                Log.WriteLine("|MainApp.cs| Sending mouse down with SendInput");
                Input.LMouseDown(x, y);
            }
        }

        private void LeftMouseUp(int x, int y)
        {
            if (Properties.Settings.Default.oldMouseInput)
            {
                Log.WriteLine("|MainApp.cs| Sending mouse up with mouse_event");
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTUP,
                        x,
                        y,
                        0, 0);
            }
            else
            {
                Log.WriteLine("|MainApp.cs| Sending mouse up with SendInput");
                Input.LMouseUp(x, y);
            }
        }

        void PressKey(Input.DirectXKeyStrokes key)
        {
            Input.SendKey(key, false, Input.InputType.Keyboard);
            Input.SendKey(key, true, Input.InputType.Keyboard);
        }
        void PressKeys(Input.DirectXKeyStrokes[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                PressKey(keys[i]);
            }
        }
        private string GetCSGODir()
        {
            List<string> csgoDir = Steam.GetGameDir("Counter-Strike Global Offensive");
            foreach (string path in csgoDir)
            {
                if (Directory.Exists(Path.Combine(path, "game\\csgo\\cfg\\")))
                    return $"{path}\\";
            }
            return null;
        }
        public void LeftMouseClick(int xpos, int ypos)
        {
            NativeMethods.SetCursorPos(xpos, ypos);
            Thread.Sleep(100);
            LeftMouseDown(xpos, ypos);
            Thread.Sleep(50);
            LeftMouseUp(xpos, ypos);
            Log.WriteLine($"|MainApp.cs| Left clicked at X:{xpos} Y:{ypos}");
        }
        private void AutoAcceptMatch()
        {
            if (!DXGIcapture.Enabled && !Properties.Settings.Default.oldScreenCaptureWay)
            {
                DXGIcapture.Init();
                Log.WriteLine("|MainApp.cs| Init DXGI Capture");
            }
            if ((DXGIcapture.Enabled && !Properties.Settings.Default.oldScreenCaptureWay && inLobby == true) ||
                (inLobby == true && Properties.Settings.Default.oldScreenCaptureWay))
            {
                using (Bitmap bitmap = GetBitmap())
                {
                    if (bitmap == null)
                        return;

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
                        guiWindow.DebugPixelColor.Text = $"Pixel color at ({pixelPos.X},{pixelPos.Y}): [{pixelColor.R},{pixelColor.G},{pixelColor.B}]";
                    }
                    bool found = false;
                    int count = 0;
                    int yStart = bitmap.Height - 1;
                    int xMiddle = csResolution.Width / 2;
                    for (int y = yStart; y >= 0 && !found; y--)
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
                                    if (Properties.Settings.Default.acceptedNotification)
                                        SendMessageToClients(Languages.Strings.ResourceManager.GetString("server_acceptmatch"), command: Commands.AcceptedMatch);
                                    if (acceptButtonTimer.IsEnabled)
                                        acceptButtonTimer.Stop();
                                    if (Properties.Settings.Default.sendAcceptImage && Properties.Settings.Default.telegramChatId != "")
                                        Telegram.SendPhoto(bitmap,
                                            Properties.Settings.Default.telegramChatId,
                                            Telegram.CheckToken(Properties.Settings.Default.customTelegramToken) ?
                                                Properties.Settings.Default.customTelegramToken
                                                : Encoding.UTF8.GetString(Convert.FromBase64String(APIKeys.TELEGRAM_BOT_TOKEN + "==")),
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
                                }
                            }
                            count++;
                        }
                    }
                }
            }
            else if (inLobby == true && !DXGIcapture.Enabled && Properties.Settings.Default.oldScreenCaptureWay)
            {
                DXGIcapture.Init();
                Log.WriteLine("|MainApp.cs| Init DXGI Capture");
            }
        }

        private Bitmap GetBitmap()
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
                    return null;
                }
            }

            Bitmap bitmap;
            if (Properties.Settings.Default.oldScreenCaptureWay)
            {
                bitmap = new Bitmap(csResolution.Width, csResolution.Height);
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
            {
                Bitmap origBitmap = Image.FromHbitmap(_handle);

                if (csResolution.X < 0 || csResolution.Y < 0)
                {
                    Properties.Settings.Default.oldScreenCaptureWay = true;
                    Properties.Settings.Default.Save();
                    Log.WriteLine("|MainApp.cs| Changed to old screen capture way because cs window x and y were less then 0 and this is not supported with DXGI capture");
                    return null;
                }

                bitmap = origBitmap.Clone(
                    new Rectangle()
                    {
                        X = csResolution.X,
                        Y = csResolution.Y,
                        Width = csResolution.Width,
                        Height = csResolution.Height
                    },
                    origBitmap.PixelFormat);
                NativeMethods.DeleteObject(_handle);
                origBitmap.Dispose();
            }

            return bitmap;
        }

        private void Notifyicon_RightMouseButtonClick(object sender, NotifyIconLibrary.Events.MouseLocationEventArgs e)
        {
            if (exitCm != null)
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

            serverThread?.Abort();

            serverRunning = false;

            if (current.IsPortable)
            {
                File.WriteAllText(Log.WorkPath + "\\.conf", current.settings.ToString(), Encoding.UTF8);
                current.settings.DeleteSettings();
            }

            if (File.Exists(bindCfgPath))
                File.Delete(bindCfgPath);
        }
        private void CheckForDuplicates()
        {
            var currentProcess = Process.GetCurrentProcess();
            var duplicates = Process.GetProcessesByName(currentProcess.ProcessName).Where(o => o.Id != currentProcess.Id);
            if (duplicates.Any())
            {
                duplicates.ToList().ForEach(dupl => dupl.Kill());
            }
        }
        public string GetLocalIPAddress()
        {
            if (localIp == null)
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIp = endPoint.Address.ToString();
                }
                return localIp;
            }
            return localIp;
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
                    SendMessageToClients(string.Format(Languages.Strings.ResourceManager.GetString("server_computeronline"), Environment.MachineName, GetLocalIPAddress(), FULL_VER), command: Commands.Connected);
                if (current.StartWindow)
                    Notifyicon_LeftMouseButtonDoubleClick(null, null);
                if (csgoDir == null)
                    throw new DirectoryNotFoundException(Languages.Strings.ResourceManager.GetString("exception_csgonotfound")/*"Couldn't find CS:GO directory"*/);
                integrationPath = csgoDir + "game\\csgo\\cfg\\gamestate_integration_csauto.cfg";
                bindCfgPath = csgoDir + "game\\csgo\\cfg\\csautobindcommandsender.cfg";
                commandSender = new BindCommandSender(bindCfgPath);
                Log.WriteLine($"|MainApp.cs| Integration file path is: '{integrationPath}'");
                InitializeGSIConfig();
                windowSource = PresentationSource.FromVisual(this) as HwndSource;
                windowSource.AddHook(WndProc);
                //InitializeNetConLaunchOption();
                InitializeBindLaunchOption();
                File.WriteAllText(bindCfgPath, "bind f13 exec csautobindcommandsender.cfg");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", Languages.Strings.ResourceManager.GetString("title_warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            NativeMethods.OptimizeMemory();
        }

        private void InitializeBindLaunchOption()
        {
            Steam.GetLaunchOptions(730, out string launchOpt);
            if (launchOpt != null && !HasBind(launchOpt))
            {
                if (Steam.IsRunning())
                    throw new Exception(Languages.Strings.error_steamrunning);
                Steam.SetLaunchOptions(730, launchOpt + $" +exec csautobindcommandsender.cfg");
            }
            else if (launchOpt == null)
            {
                if (Steam.IsRunning())
                    throw new Exception(Languages.Strings.error_steamrunning);
                Steam.SetLaunchOptions(730, $"+exec csautobindcommandsender.cfg");
            }
            else
                Log.WriteLine($"Already has \'+exec csautobindcommandsender.cfg\' in launch options.");
        }

        private bool HasBind(string launchOpt)
        {
            return launchOpt.Contains("+exec csautobindcommandsender.cfg");
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
                                File.Copy(Log.WorkPath + "\\bin\\updater.exe", path + "\\updater.exe");
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

        internal void Notifyicon_LeftMouseButtonDoubleClick(object sender, NotifyIconLibrary.Events.MouseLocationEventArgs e)
        {
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