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
        NotifyIconWrapper notifyicon = new NotifyIconWrapper();
        ContextMenu exitcm = new ContextMenu();
        System.Windows.Threading.DispatcherTimer appTimer = new System.Windows.Threading.DispatcherTimer();
        const string VER = "1.0.6";
        Point csgoResolution = new Point();
        Color BUTTON_COLOR = Color.FromArgb(76, 175, 80);
        Color ACTIVE_BUTTON_COLOR = Color.FromArgb(90, 203, 94);
        int frame = 0;
        MenuItem startUpCheck = new MenuItem();
        MenuItem saveFramesDebug = new MenuItem();
        MenuItem autoAcceptMatchCheck = new MenuItem();
        MenuItem autoReloadCheck = new MenuItem();
        string integrationPath = null;
        bool inGame = false;
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
                //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                Application.Current.Exit += Current_Exit;
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
                autoAcceptMatchCheck.IsChecked = Properties.Settings.Default.autoAcceptMatch;
                autoAcceptMatchCheck.Header = "Auto Accept Match";
                autoAcceptMatchCheck.IsCheckable = true;
                autoAcceptMatchCheck.Click += AutoAcceptMatchCheck_Click;
                autoReloadCheck.IsChecked = Properties.Settings.Default.autoReload;
                autoReloadCheck.Header = "Auto Reload";
                autoReloadCheck.IsCheckable = true;
                autoReloadCheck.Click += AutoReloadCheck_Click;
                debugMenu.Items.Add(saveFramesDebug);
                exitcm.Items.Add(about);
                exitcm.Items.Add(debugMenu);
                exitcm.Items.Add(new Separator());
                exitcm.Items.Add(autoReloadCheck);
                exitcm.Items.Add(autoAcceptMatchCheck);
                exitcm.Items.Add(startUpCheck);
                exitcm.Items.Add(new Separator());
                exitcm.Items.Add(exit);
                exitcm.StaysOpen = false;
                Top = -1000;
                Left = -1000;
                integrationPath = GetCSGODir()+"\\cfg\\gamestate_integration_csauto.cfg";
                if (!File.Exists(integrationPath))
                {
                    using (FileStream fs = File.Create(integrationPath))
                    {
                        Byte[] title = new UTF8Encoding(true).GetBytes("\"CSAuto Integration v " + VER + "\"\r\n{\r\n\"uri\" \"http://localhost:3000\"\r\n\"timeout\" \"5.0\"\r\n\"buffer\"  \"0.1\"\r\n\"throttle\" \"0.5\"\r\n\"heartbeat\" \"10.0\"\r\n\"data\"\r\n{\r\n   \"provider\"            \"1\"\r\n   \"map\"                 \"1\"\r\n   \"round\"               \"1\"\r\n   \"player_id\"           \"1\"\r\n   \"player_state\"        \"1\"\r\n   \"player_weapons\"      \"1\"\r\n   \"player_match_stats\"  \"1\"\r\n}\r\n}");
                        fs.Write(title, 0, title.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            string[] splitted = JSON.Split(new string[] { "\"activity\": \"" }, StringSplitOptions.None);
            if (splitted.Length > 1)
            {
                string activity = splitted[1].Split('"')[0];
                inGame = activity != "menu";
            }
            if (Properties.Settings.Default.autoReload && inGame)
            {
                TryToAutoReload(JSON);
            }
            //Debug.WriteLine($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second},{DateTime.Now.Millisecond}] - Got info from GSI");
            //AutoBuyDefuseKit(JSON); - Can't open buy menu, sadly :(
        }

        private void AutoBuyDefuseKit(string JSON)
        {
            string matchState = GetMatchState(JSON);
            string roundState = GetRoundState(JSON);
            int money = GetMoney(JSON);
            if(matchState == "live"
                && roundState == "freezetime"
                && money >= 400
                && !HasDefuseKit(JSON)
                && GetTeam(JSON) == "CT")
            {
                Debug.WriteLine("Auto buying defuse kit");
                PressKey(Keys.B);
                PressKey(Keys.D5);
                PressKey(Keys.D4);
                PressKey(Keys.Escape);
                PressKey(Keys.Escape);
            }
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
            string weapons;
            string weapon;
            int bullets;
            string weaponType;
            string weaponName;
            try
            {
                weapons = GetWeapons(JSON);
                weapon = GetActiveWeapon(weapons);
                bullets = GetBulletAmount(weapon);
                weaponType = GetWeaponType(weapon);
                weaponName = GetWeaponName(weapon);
            }
            catch { return; }
            if (bullets == 0)
            {
                mouse_event(MOUSEEVENTF_LEFTUP,
                    System.Windows.Forms.Cursor.Position.X,
                    System.Windows.Forms.Cursor.Position.Y,
                    0, 0);
                Debug.WriteLine("Auto reloading");
                if ((weaponType == "Rifle"
                    || weaponType == "Machine Gun"
                    || weaponType == "Submachine Gun")
                    && (weaponName != "weapon_sg556"))
                {
                    Thread.Sleep(200);
                    mouse_event(MOUSEEVENTF_LEFTDOWN,
                        System.Windows.Forms.Cursor.Position.X,
                        System.Windows.Forms.Cursor.Position.Y,
                        0, 0);
                    Debug.WriteLine($"Continue spraying ({weaponName} - {weaponType})");
                }
                
            }
        }

        private string GetWeaponName(string weapon)
        {
            string type = weapon.Split(new string[] { "\"name\": \"" }, StringSplitOptions.None)[1];
            return type.Split('"')[0];
        }

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
   UIntPtr dwExtraInfo);
        void PressKey(Keys key)
        {
            const int KEYEVENTF_KEYDOWN = 0x0;
            const int KEYEVENTF_KEYUP = 0x2;
            // I had some Compile errors until I Casted the final 0 to UIntPtr like this...
            keybd_event((byte)key, 0x45, KEYEVENTF_KEYDOWN, (UIntPtr)0);
            keybd_event((byte)key, 0x45, KEYEVENTF_KEYUP, (UIntPtr)0);
        }
        void PressKey(Key key)
        {
            System.Windows.Forms.SendKeys.SendWait(key.ToString());
        }
        void PressKey(string key)
        {
            System.Windows.Forms.SendKeys.SendWait(key);
        }
        private int GetBulletAmount(string weapon)
        {
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
        bool HasDefuseKit(string JSON)
        {
            string[] split = JSON.Split(new string[] { "\"defusekit\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return false;
            return bool.Parse(split[1].Split(',')[0]);
        }
        private string GetWeapons(string jSON)
        {
            string[] splitted = jSON.Split(new string[] { "\"weapons\": {" }, StringSplitOptions.None);
            string weapons = splitted[1].Split(new string[] { "\"match_stats\": {" }, StringSplitOptions.None)[0];
            return weapons;
        }

        private string GetActiveWeapon(string weapons)
        {
            string[] splitted = weapons.Split(new string[] { "\"state\": \"" }, StringSplitOptions.None);
            for (int i = 1; i < splitted.Length; i++)
            {
                if (splitted[i].Split('"')[0] == "active")
                {
                    return weapons.Split(new string[] { "\"weapon_" + (i - 1) + "\": {" }, StringSplitOptions.None)[1].Split(new string[] { "}" }, StringSplitOptions.None)[0].Replace("\t", "").Trim();
                }
            }
            return splitted[0];
        }
        private string GetWeaponType(string weapon)
        {
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
            Debug.WriteLine($"Left clicked at X:{xpos} Y:{ypos}");
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
        Process GetActiveProcess()
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
                return null;
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process p = Process.GetProcessById((int)pid);
            return p;
        }
        private void TimerCallback(object sender, EventArgs e)
        {
            if (!ServerRunning)
            {
                Debug.WriteLine("Starting GSI Server");
                StartGSIServer();
            }
            try
            {
                Process activeProcces = GetActiveProcess();
                if (activeProcces == null)
                    return;
                string proccesName = activeProcces.ProcessName;
                if (proccesName == "csgo")
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
                Debug.WriteLine($"{ex}");
            }
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
                    Directory.CreateDirectory($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.ToString())}\\FRAMES");
                    bitmap.Save($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.ToString())}\\FRAMES\\Frame{frame++}.jpeg", ImageFormat.Jpeg);
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
                        Debug.WriteLine($"Found accept button at X:{X} Y:{Y}");
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
        }
        private void Current_Exit(object sender, ExitEventArgs e)
        {
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
    }
}
