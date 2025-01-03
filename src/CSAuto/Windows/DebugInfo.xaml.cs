using MahApps.Metro.Controls;
using Murky.Utils;
using Murky.Utils.CSGO;
using System;
using System.IO;
using System.Windows.Interop;

namespace CSAuto
{
    /// <summary>
    /// Interaction logic for DebugInfo.xaml
    /// </summary>
    public partial class DebugInfo : MetroWindow
    {
        public DebugInfo(MainApp main)
        {
            InitializeComponent();

            if (main.current.IsWindows11)
            {
                IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
                var attribute = NativeMethods.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
                var preference = NativeMethods.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                NativeMethods.DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
            }

            Steam.GetLaunchOptions(730, out string launchOpt);

            long id3 = Steam.GetCurrentSteamID3();
            long id64 = id3 + Steam.VALVE_STEAMID64_CONST;
            string friendCode = "";
            try
            {
                friendCode = CSGOFriendCode.Encode(id64.ToString());
            }
            catch { }

            string regularButtonColor = "";
            string activeButtonColor = "";

            if (main.BUTTON_COLORS != null)
            {
                regularButtonColor = $"({main.BUTTON_COLORS[0].R},{main.BUTTON_COLORS[0].G},{main.BUTTON_COLORS[0].B})";
                activeButtonColor = $"({main.BUTTON_COLORS[1].R},{main.BUTTON_COLORS[1].G},{main.BUTTON_COLORS[1].B})";
            }

            TextBlock.Text = $"CSAuto Version {MainApp.FULL_VER}\n"
                + $"\n====App info====\n"
                + $"IsPortable: {File.Exists(Log.WorkPath + "\\resource\\.portable")}\n"
                + $"Screen Capture Type: {(Properties.Settings.Default.oldScreenCaptureWay ? "Old capture" : "New capture")}\n"
                + $"Mouse Input Type: {(Properties.Settings.Default.oldMouseInput ? "Old input" : "New input")}\n"
                + $"Active Button Color: {activeButtonColor}\n"
                + $"Regular Button Color: {regularButtonColor}\n"
                + $"Settings Path: {DiscordRPCButtonSerializer.Path}\n"
                + $"\n====Steam info====\n"
                + $"Steam Path: \"{Steam.GetSteamPath()}\"\n"
                + $"SteamID3: {id3}\n"
                + $"SteamID64: {id64}\n"
                + $"\n====CS:GO info====\n"
                + $"CS:GO FriendCode: {friendCode}\n"
                + $"CS:GO Path: \"{Steam.GetGameDir("Counter-Strike Global Offensive")}\"\n"
                + $"CS:GO LaunchOptions: \"{launchOpt}\"";
        }
    }
}
