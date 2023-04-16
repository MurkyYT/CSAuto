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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Keys = System.Windows.Forms.Keys;

namespace CSAuto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GSIDebugWindow debugWind = null;
        NotifyIconWrapper notifyicon = new NotifyIconWrapper();
        ContextMenu exitcm = new ContextMenu();
        System.Windows.Threading.DispatcherTimer appTimer = new System.Windows.Threading.DispatcherTimer();
        const string VER = "1.0.9";
        Point csgoResolution = new Point();
        Color BUTTON_COLOR = Color.FromArgb(76, 175, 80);
        Color ACTIVE_BUTTON_COLOR = Color.FromArgb(90, 203, 94);
        int frame = 0;
        MenuItem startUpCheck = new MenuItem();
        MenuItem saveFramesDebug = new MenuItem();
        MenuItem autoAcceptMatchCheck = new MenuItem();
        MenuItem autoReloadCheck = new MenuItem();
        MenuItem autoBuyArmor = new MenuItem();
        MenuItem autoBuyDefuseKit = new MenuItem();
        MenuItem preferArmorCheck = new MenuItem();
        MenuItem saveLogsCheck = new MenuItem();
        string integrationPath = null;
        bool inGame = false;
        bool csgoActive = false;
        string lastActivity;
        string matchState;
        string roundState;
        string weapon;
        int round = -1;
        string integrationFile = "\"CSAuto Integration v" + VER + "\"\r\n{\r\n\"uri\" \"http://localhost:3000\"\r\n\"timeout\" \"5.0\"\r\n\"buffer\"  \"0.1\"\r\n\"throttle\" \"0.5\"\r\n\"heartbeat\" \"10.0\"\r\n\"data\"\r\n{\r\n   \"provider\"            \"1\"\r\n   \"map\"                 \"1\"\r\n   \"round\"               \"1\"\r\n   \"player_id\"           \"1\"\r\n   \"player_state\"        \"1\"\r\n   \"player_weapons\"      \"1\"\r\n   \"player_match_stats\"  \"1\"\r\n   \"bomb\" \"1\"\r\n}\r\n}";
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
                killDuplicates();
                Log.saveLogs = Properties.Settings.Default.saveLogs;
                Log.WriteLine($"CSAuto v{VER} started");
                //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                MenuItem autoBuyMenu = new MenuItem();
                autoBuyMenu.Header = "Auto Buy";
                MenuItem debugMenu = new MenuItem();
                debugMenu.Header = "Debug";
                MenuItem exit = new MenuItem();
                exit.Header = "Exit";
                exit.Click += Exit_Click;
                MenuItem about = new MenuItem();
                about.Header = $"{typeof(MainWindow).Namespace} - {VER}";
                about.IsEnabled = false;
                about.Icon = new System.Windows.Controls.Image
                {
                    Source = ToImageSource(System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location))
                };
                startUpCheck.IsChecked = Properties.Settings.Default.runAtStartUp;
                startUpCheck.Header = "Start With Windows";
                startUpCheck.IsCheckable = true;
                startUpCheck.Click += StartUpCheck_Click;
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
                autoReloadCheck.Header = "Auto Reload";
                autoReloadCheck.IsCheckable = true;
                autoReloadCheck.Click += AutoReloadCheck_Click;
                debugMenu.Items.Add(saveFramesDebug);
                debugMenu.Items.Add(saveLogsCheck);
                autoBuyMenu.Items.Add(preferArmorCheck);
                autoBuyMenu.Items.Add(autoBuyArmor);
                autoBuyMenu.Items.Add(autoBuyDefuseKit);
                exitcm.Items.Add(about);
                exitcm.Items.Add(debugMenu);
                exitcm.Items.Add(new Separator());
                exitcm.Items.Add(autoBuyMenu);
                exitcm.Items.Add(autoReloadCheck);
                exitcm.Items.Add(autoAcceptMatchCheck);
                exitcm.Items.Add(startUpCheck);
                exitcm.Items.Add(new Separator());
                exitcm.Items.Add(exit);
                Top = -1000;
                Left = -1000;
                exitcm.StaysOpen = false;
                integrationPath = GetCSGODir()+"\\cfg\\gamestate_integration_csauto.cfg";
                if (!File.Exists(integrationPath))
                {
                    using (FileStream fs = File.Create(integrationPath))
                    {
                        Byte[] title = new UTF8Encoding(true).GetBytes(integrationFile);
                        fs.Write(title, 0, title.Length);
                    }
                    Log.WriteLine("CSAuto was never launched, initializing 'gamestate_integration_csauto.cfg'");
                }
                else
                {
                    string[] lines = File.ReadAllLines(integrationPath);
                    string ver = lines[0].Split('v')[1].Split('"')[0].Trim();
                    if(ver != VER)
                    {
                        using (FileStream fs = File.Create(integrationPath))
                        {
                            Byte[] title = new UTF8Encoding(true).GetBytes(integrationFile);
                            fs.Write(title, 0, title.Length);
                        }
                        Log.WriteLine("Different 'gamestate_integration_csauto.cfg' was found, installing correct 'gamestate_integration_csauto.cfg'");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveLogsCheck_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.saveLogs = saveLogsCheck.IsChecked;
            Properties.Settings.Default.Save();
            Log.saveLogs = Properties.Settings.Default.saveLogs;
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

        private AutoResetEvent _waitForConnection = new AutoResetEvent(false);
        private HttpListener _listener;
        private bool ServerRunning = false;
        public bool StartGSIServer()
        {
            if (ServerRunning)
                return false;

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:" + "3000" + "/");
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
            finally
            {
                _waitForConnection.Set();
            }

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
            string activity = GetActivity(JSON);
            string currentMatchState = GetMatchState(JSON);
            string currentRoundState = GetRoundState(JSON);
            string currentWeapon = GetActiveWeapon(GetWeapons(JSON));
            int currentRound = GetRound(JSON);
            int money = GetMoney(JSON);
            if (debugWind != null)
                debugWind.UpdateText(JSON);
            if (Properties.Settings.Default.saveLogs)
            {
                if (lastActivity != activity)
                    Log.WriteLine($"Activity: {(lastActivity == null ? "None" : lastActivity)} -> {(activity == null ? "None" : activity)}");
                if (currentMatchState != matchState)
                    Log.WriteLine($"Match State: {(matchState == null ? "None" : matchState)} -> {(currentMatchState == null ? "None" : currentMatchState)}");
                if (currentRoundState != roundState)
                    Log.WriteLine($"Round State: {(roundState == null ? "None" : roundState)} -> {(currentRoundState == null ? "None" : currentRoundState)}");
                if (round != currentRound)
                    Log.WriteLine($"RoundNo: {(round == -1 ? "None" : round.ToString())} -> {(currentRound == -1 ? "None" : currentRound.ToString())}");
                //if (GetWeaponName(weapon) != GetWeaponName(currentWeapon))
                //    Log.WriteLine($"Current Weapon: {(weapon == null ? "None" : GetWeaponName(weapon))} -> {(currentWeapon == null ? "None" : GetWeaponName(currentWeapon))}");
            }
            lastActivity = activity;
            matchState = currentMatchState;
            roundState = currentRoundState;
            round = currentRound;
            weapon = currentWeapon;
            inGame = activity != "menu";
            if (csgoActive && !IsSpectating(JSON))
            {
                if (Properties.Settings.Default.autoReload && inGame)
                {
                    TryToAutoReload(JSON);
                }
                if (Properties.Settings.Default.preferArmor)
                {
                    AutoBuyArmor(JSON,money);
                    AutoBuyDefuseKit(JSON,money);
                }
                else
                {
                    AutoBuyDefuseKit(JSON,money);
                    AutoBuyArmor(JSON,money);
                }
            }
            //Log.WriteLine($"Got info from GSI\nActivity:{activity}\nCSGOActive:{csgoActive}\nInGame:{inGame}\nIsSpectator:{IsSpectating(JSON)}");
        }
        bool IsForegroundProcess(uint pid)
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == null) return false;

            uint foregroundPid;
            if (GetWindowThreadProcessId(hwnd,out foregroundPid) == (IntPtr)0) return false;

            return (foregroundPid == pid);
        }
        private void TimerCallback(object sender, EventArgs e)
        {
            if (!ServerRunning)
            {
                Log.WriteLine("Starting GSI Server");
                StartGSIServer();
            }
            try
            {
                uint pid = 0;
                Process[] prcs = Process.GetProcessesByName("csgo");
                if (prcs.Length > 0)
                    pid = (uint)prcs[0].Id;
                csgoActive = IsForegroundProcess(pid);
                if (csgoActive)
                {
                    csgoResolution = new Point(
                            (int)SystemParameters.PrimaryScreenWidth,
                            (int)SystemParameters.PrimaryScreenHeight);
                    if (Properties.Settings.Default.autoAcceptMatch && !inGame)
                        AutoAcceptMatch();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"{ex}");
            }
            GC.Collect();
        }
        private int GetRound(string JSON)
        {
            string[] splitted = JSON.Split(new string[] { "\"round\": " }, StringSplitOptions.None);
            if (splitted.Length > 1)
            {
                int res;
                bool succes = int.TryParse(splitted[1].Split(',')[0],out res);
                if (succes)
                    return res;
            }
            return -1;
        }

        private string GetActivity(string JSON)
        {
            string[] splitted = JSON.Split(new string[] { "\"activity\": \"" }, StringSplitOptions.None);
            if (splitted.Length > 1)
            {
                return splitted[1].Split('"')[0];
            }
            return null;
        }

        private void AutoBuyArmor(string JSON,int money)
        {
            if (!Properties.Settings.Default.autoBuyArmor || !inGame)
                return;
            int armor = GetArmor(JSON);
            bool hasHelmet = GetHelmetState(JSON);
            if ((matchState == "live"
                && roundState == "freezetime")
                && 
                ((money >= 650 && armor <= 70)||
                (money >= 350 && armor == 100 && !hasHelmet)||
                (money >= 1000 && armor <= 70 && !hasHelmet))
                )
            {
                DisableConsole(JSON);
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
        private void AutoBuyDefuseKit(string JSON, int money)
        {
            if (!Properties.Settings.Default.autoBuyDefuseKit || !inGame)
                return;
            bool hasDefuseKit = HasDefuseKit(JSON);
            if (matchState == "live"
                && roundState == "freezetime"
                && money >= 400
                && !hasDefuseKit
                && GetTeam(JSON) == "CT")
            {
                DisableConsole(JSON);
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
        private void DisableConsole(string JSON)
        {
            string activity = GetActivity(JSON);
            if(activity == "textinput")
                PressKey(Keyboard.DirectXKeyStrokes.DIK_GRAVE);
        }

        private bool GetHelmetState(string JSON)
        {
            string[] split = JSON.Split(new string[] { "\"helmet\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return false;
            try
            {
                return bool.Parse(split[1].Split(',')[0]);
            }
            catch { return false; }
        }
        private string GetTeam(string JSON)
        {
            string[] split = JSON.Split(new string[] { "\"team\": \"" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            return split[1].Split('"')[0];
            
        }
        private int GetMoney(string JSON)
        {
            string[] split = JSON.Split(new string[] { "\"money\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            return int.Parse(split[1].Split(',')[0]);
        }
        private string GetMatchState(string JSON)
        {
            string[] split = JSON.Split(new string[] { "\"map\": {" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            return split[1].Split(new string[] { "\"phase\": \"" }, StringSplitOptions.None)[1].Split('"')[0];
        }
        
        private void TryToAutoReload(string JSON)
        {
            bool isMousePressed = (Keyboard.GetKeyState(Keyboard.VirtualKeyStates.VK_LBUTTON) & 0x80) != 0;
            if (!isMousePressed)
                return;
            try
            {
                int bullets = GetBulletAmount(weapon);
                string weaponType = GetWeaponType(weapon);
                string weaponName = GetWeaponName(weapon);
                if (bullets == 0)
                {
                    mouse_event(MOUSEEVENTF_LEFTUP,
                        System.Windows.Forms.Cursor.Position.X,
                        System.Windows.Forms.Cursor.Position.Y,
                        0, 0);
                    Log.WriteLine("Auto reloading");
                    if ((weaponType == "Rifle"
                        || weaponType == "Machine Gun"
                        || weaponType == "Submachine Gun")
                        && (weaponName != "weapon_sg556"))
                    {
                        Thread.Sleep(150);
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
        bool IsSpectating(string JSON)
        {
            return GetBombState(JSON) != null;
        }
        string GetBombState(string JSON)
        {
            string[] split = JSON.Split(new string[] { "\"bomb\": {" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;

            string state = split[1].Split(new string[] { "\"state\": \"" }, StringSplitOptions.None)[1];
            return state.Split('"')[0];
        }
        private string GetWeaponName(string weapon)
        {
            if (weapon == null)
                return null;
            string type = weapon.Split(new string[] { "\"name\": \"" }, StringSplitOptions.None)[1];
            return type.Split('"')[0];
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
        private int GetBulletAmount(string weapon)
        {
            if (weapon == null)
                return -1;
            string[] split = weapon.Split(new string[] { "\"ammo_clip\":" }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            int bullets = int.Parse(split[1].Split(',')[0]);
            return bullets;
        }
        string GetRoundState(string JSON)
        {
            string[] split = JSON.Split(new string[] { "\"round\": {" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            return split[1].Split(new string[] { "\"phase\": \"" }, StringSplitOptions.None)[1].Split('"')[0];
        }
        int GetArmor(string JSON)
        {
            string[] split = JSON.Split(new string[] { "\"armor\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            int armor = int.Parse(split[1].Split(',')[0]);
            return armor;
        }
        bool HasDefuseKit(string JSON)
        {
            string splitStr = JSON.Split(new string[] { "\"player\": {" }, StringSplitOptions.None)[1].Split('}')[0];
            string[] split = splitStr.Split(new string[] { "\"defusekit\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return false;
            try
            {
                return bool.Parse(split[1].Split(',')[0]);
            }
            catch { return false; }
        }
        private string GetWeapons(string jSON)
        {
            string[] splitted = jSON.Split(new string[] { "\"weapons\": {" }, StringSplitOptions.None);
            if (splitted.Length > 1)
            {
                string weapons = splitted[1].Split(new string[] { "\"match_stats\": {" }, StringSplitOptions.None)[0];
                return weapons;
            }
            return null;
        }

        private string GetActiveWeapon(string weapons)
        {
            if (weapons == null)
                return null;
            string[] splitted = weapons.Split(new string[] { "\"state\": \"" }, StringSplitOptions.None);
            for (int i = 1; i < splitted.Length; i++)
            {
                if (splitted[i].Split('"')[0] == "active")
                {
                    return weapons.Split(new string[] { "\"weapon_" + (i - 1) + "\": {" }, StringSplitOptions.None)[1].Split(new string[] { "}" }, StringSplitOptions.None)[0].Replace("\t", "").Trim();
                }
            }
            return null;
        }
        private string GetWeaponType(string weapon)
        {
            if (weapon == null)
                return null;
            string type = weapon.Split(new string[] { "\"type\": \"" }, StringSplitOptions.None)[1];
            return type.Split('"')[0];
        }
        string GetSteamPath()
        {
            string X86 = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "InstallPath", null);
            string X64 = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", null);
            return (X86 == null) ? X64 : X86;
        }
        private string GetCSGODir()
        {
            string steamPath = GetSteamPath();
            string pathsFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

            if (!File.Exists(pathsFile))
                return null;

            List<string> libraries = new List<string>();
            libraries.Add(Path.Combine(steamPath));

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
            string executablePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
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
        private void AutoAcceptMatch()
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
                    Directory.CreateDirectory($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.ToString())}\\DEBUG\\FRAMES");
                    bitmap.Save($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.ToString())}\\DEBUG\\FRAMES\\Frame{frame++}.jpeg", ImageFormat.Jpeg);
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
                        Log.WriteLine($"Found accept button at X:{X} Y:{Y}");
                        LeftMouseClick(X, Y);
                        found = true;
                    }
                }
            }
        }
        private void Notifyicon_RightMouseButtonClick(object sender, NotifyIconLibrary.Events.MouseLocationEventArgs e)
        {
            exitcm.IsOpen = true;
            Activate();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            this.notifyicon.Close();
            StopGSIServer();
        }
        private bool killDuplicates()
        {
            bool success = true;
            var currentProcess = Process.GetCurrentProcess();
            var duplicates = Process.GetProcessesByName(currentProcess.ProcessName).Where(o => o.Id != currentProcess.Id);

            if (duplicates.Count() > 0)
            {
                notifyicon.Close();
                App.Current.Shutdown();
                Log.WriteLine($"Shutting down, found another CSAuto process");
            }

            return success;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                Visibility = Visibility.Hidden;
                this.notifyicon.Icon = Properties.Resources.main;
                this.notifyicon.Tip = "CSAuto - CS:GO Automation";
                this.notifyicon.ShowTip = true;
                this.notifyicon.RightMouseButtonClick += Notifyicon_RightMouseButtonClick;
                this.notifyicon.LeftMouseButtonDoubleClick += Notifyicon_LeftMouseButtonDoubleClick;
                this.notifyicon.Update();
                appTimer.Interval = TimeSpan.FromMilliseconds(1000);
                appTimer.Tick += new EventHandler(TimerCallback);
                appTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                debugWind.Show();
            }
        }
    }
}
