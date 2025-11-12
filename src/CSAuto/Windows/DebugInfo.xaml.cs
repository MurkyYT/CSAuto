using MahApps.Metro.Controls;
using Murky.Utils;
using Murky.Utils.CS;
using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace CSAuto
{
    /// <summary>
    /// Interaction logic for DebugInfo.xaml
    /// </summary>
    public partial class DebugInfo : MetroWindow
    {
        MainApp main;
        public DebugInfo(MainApp main)
        {
            InitializeComponent();

            this.main = main;
            if (main.current.IsWindows11)
            {
                IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
                var attribute = NativeMethods.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
                var preference = NativeMethods.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                NativeMethods.DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
            }

            if (main.current.RTLLanguage)
            {
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }

            Steam.GetLaunchOptions(730, out string launchOpt);

            long id3 = Steam.GetCurrentSteamID3();
            long id64 = id3 + Steam.VALVE_STEAMID64_CONST;
            string friendCode = "";
            try
            {
                friendCode = CSFriendCode.Encode(id64.ToString());
            }
            catch { }

            string regularButtonColor = "";
            string activeButtonColor = "";

            if (main.BUTTON_COLORS != null)
            {
                regularButtonColor = $"({main.BUTTON_COLORS[0].R},{main.BUTTON_COLORS[0].G},{main.BUTTON_COLORS[0].B})";
                activeButtonColor = $"({main.BUTTON_COLORS[1].R},{main.BUTTON_COLORS[1].G},{main.BUTTON_COLORS[1].B})";
            }

            string dxgiInfo = "";

            if (!Properties.Settings.Default.oldScreenCaptureWay)
            {
                bool wasEnabled = main.DXGIcapture.Enabled;

                dxgiInfo += "\n====DXGI info====\n";

                if (!wasEnabled)
                    main.DXGIcapture.Init();

                int adapterCount = main.DXGIcapture.AdaptersCount;
                for (int i = 0; i < adapterCount; i++)
                {
                    try
                    {
                        var desc = main.DXGIcapture.GetAdapterDescription(i);
                        dxgiInfo += $"Adapter {i}:\n";
                        dxgiInfo += $"  Description: {desc.Description}\n";
                        dxgiInfo += $"  VendorId: 0x{desc.VendorId:X}\n";
                        dxgiInfo += $"  DeviceId: 0x{desc.DeviceId:X}\n";
                        dxgiInfo += $"  SubSysId: 0x{desc.SubSysId:X}\n";
                        dxgiInfo += $"  Revision: {desc.Revision}\n";

                        double dedicatedVideoGB = (double)desc.DedicatedVideoMemory / (1024 * 1024 * 1024);
                        double dedicatedSystemGB = (double)desc.DedicatedSystemMemory / (1024 * 1024 * 1024);
                        double sharedSystemGB = (double)desc.SharedSystemMemory / (1024 * 1024 * 1024);

                        dxgiInfo += $"  DedicatedVideoMemory: {dedicatedVideoGB:F2} GB\n";
                        dxgiInfo += $"  DedicatedSystemMemory: {dedicatedSystemGB:F2} GB\n";
                        dxgiInfo += $"  SharedSystemMemory: {sharedSystemGB:F2} GB\n";

                        dxgiInfo += $"  Flags: 0x{desc.Flags:X}\n";
                    }
                    catch (Exception ex)
                    {
                        dxgiInfo += $"Adapter {i}: Failed to get description: {ex.Message}\n";
                    }
                }

                try
                {
                    int outputCount = main.DXGIcapture.OutputsCount;
                    for (int i = 0; i < outputCount; i++)
                    {
                        var outputDesc = main.DXGIcapture.GetOutputDescription(i);
                        dxgiInfo += $"\nOutput {i}:\n";
                        dxgiInfo += $"  DeviceName: {outputDesc.DeviceName}\n";
                        dxgiInfo += $"  DesktopCoordinates: Left={outputDesc.DesktopCoordinates.Left}, Top={outputDesc.DesktopCoordinates.Top}, Right={outputDesc.DesktopCoordinates.Right}, Bottom={outputDesc.DesktopCoordinates.Bottom}\n";
                        dxgiInfo += $"  AttachedToDesktop: {outputDesc.AttachedToDesktop}\n";
                        dxgiInfo += $"  Rotation: {outputDesc.Rotation}\n";
                        dxgiInfo += $"  Monitor Handle: {outputDesc.Monitor}\n";
                    }
                }
                catch (Exception ex)
                {
                    dxgiInfo += $"Failed to get outputs: {ex.Message}\n";
                }

                if (!wasEnabled)
                    main.DXGIcapture.DeInit();
            }

            var (windowsVersion, buildNumber, displayVersion) = GetWindowsVersionInfo();
            string osInfo = $"\n====System info====\n"
                + $"{windowsVersion}\n"
                + $"Version {displayVersion} (OS Build {buildNumber})\n"
                + $"System Type: {(Environment.Is64BitOperatingSystem ? "64-bit operating system, " : "32-bit operating system, ")}{(Environment.Is64BitProcess ? "running as 64-bit" : "running as 32-bit")}\n"
                + $"Processor Count: {Environment.ProcessorCount} logical processors\n"
                + $".NET Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}\n";

            TextBlock.Text = $"CSAuto Version {MainApp.FULL_VER} ({CompileInfo.GitHash})\nBuilt at: {CompileInfo.Date} {CompileInfo.Time}\n"
                + osInfo
                + $"\n====App info====\n"
                + $"IsPortable: {File.Exists(Log.WorkPath + "\\resource\\.portable")}\n"
                + $"Screen Capture Type: {(Properties.Settings.Default.oldScreenCaptureWay ? "Old capture" : "New capture")}\n"
                + $"Mouse Input Type: {(Properties.Settings.Default.oldMouseInput ? "Old input" : "New input")}\n"
                + $"Active Button Color: {activeButtonColor}\n"
                + $"Regular Button Color: {regularButtonColor}\n"
                + $"Settings Path: {DiscordRPCButtonSerializer.Path}\n"
                + dxgiInfo
                + $"\n====Steam info====\n"
                + $"Steam Path: \"{Steam.GetSteamPath()}\"\n"
                + $"SteamID3: {id3}\n"
                + $"SteamID64: {id64}\n"
                + $"\n====CS:GO info====\n"
                + $"CS:GO FriendCode: {friendCode}\n"
                + $"CS:GO Integration Path: \"{main.integrationPath}\"\n"
                + $"CS:GO LaunchOptions: \"{launchOpt}\"";
        }

        private (string productName, string buildNumber, string displayVersion) GetWindowsVersionInfo()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        string productName = key.GetValue("ProductName")?.ToString() ?? "Unknown Windows";
                        string currentBuild = key.GetValue("CurrentBuild")?.ToString() ?? "Unknown";
                        string ubr = key.GetValue("UBR")?.ToString() ?? "0";
                        string displayVersion = key.GetValue("DisplayVersion")?.ToString() ??
                                               key.GetValue("ReleaseId")?.ToString() ?? "Unknown";

                        string buildNumber = $"{currentBuild}.{ubr}";

                        if (int.TryParse(currentBuild, out int build) && build >= 22000)
                        {
                            productName = productName.Replace("Windows 10", "Windows 11");
                        }

                        return (productName, buildNumber, displayVersion);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"|DebugInfo.cs| Error reading Windows version from registry: {ex.Message}");
            }

            return ("Microsoft Windows", Environment.OSVersion.Version.ToString(), "Unknown");
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