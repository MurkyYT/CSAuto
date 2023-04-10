using Microsoft.Win32;
using NotifyIconLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

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
        const string VER = "1.0.0";
        Point csgoResolution = new Point();
        Color BUTTON_COLOR = Color.FromArgb(76, 175, 80);
        MenuItem startUpCheck = new MenuItem();
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
                exitcm.Items.Add(about);
                exitcm.Items.Add(new Separator());
                exitcm.Items.Add(startUpCheck);
                exitcm.Items.Add(new Separator());
                exitcm.Items.Add(exit);
                exitcm.StaysOpen = false;
                Top = -1000;
                Left = -1000;
                InitializeComponent();
                try
                {
                    Visibility = Visibility.Hidden;
                    //this.notifyicon.Icon = Properties.Resources.unlocked;
                    this.notifyicon.ShowTip = false;
                    this.notifyicon.RightMouseButtonClick += Notifyicon_RightMouseButtonClick;
                    this.notifyicon.Update();
                    appTimer.Interval = TimeSpan.FromMilliseconds(1000);
                    appTimer.Tick += new EventHandler(TimerCallback);
                    appTimer.Start();
                    System.Windows.Threading.DispatcherTimer timer2 = new System.Windows.Threading.DispatcherTimer();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            Debug.WriteLine($"Left clicked at ({xpos},{ypos})");
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
            try
            {
                Process activeProcces = GetActiveProcess();
                if (activeProcces == null)
                    return;
                string proccesName = activeProcces.ProcessName;
                if(proccesName == "csgo")
                {
                    if (csgoResolution == new Point()) 
                    {
                        csgoResolution = new Point(
                            (int)SystemParameters.PrimaryScreenWidth,
                            (int)SystemParameters.PrimaryScreenHeight);
                    }
                    
                    using (Bitmap bitmap = new Bitmap(218, 86))
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.CopyFromScreen(new Point(591, 393), Point.Empty, new System.Drawing.Size(218,86));
                        }
                        if(bitmap.GetPixel(70,10) == BUTTON_COLOR)
                        {
                            Debug.WriteLine("Found accept button");
                            var clickpoint = new Point(591 + 70, 393 + 10);
                            int X = clickpoint.X;
                            int Y = clickpoint.Y;
                            LeftMouseClick(X, Y);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
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
    }
}
