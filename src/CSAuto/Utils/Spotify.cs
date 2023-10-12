using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Murky.Utils
{
    public static class Spotify
    {
        [DllImport("user32.dll")]
        static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        public static void Pause()
        {
            if (IsPlaying() && IsRunning())
            {
                SendMediaPauseResume();
                Log.WriteLine("Pausing Spotify");
            }
        }

        private static void SendMediaPauseResume()
        {
            Process prc = GetProcess();
            SendMessageW(prc.MainWindowHandle, 0x0319, prc.MainWindowHandle, (IntPtr)(14 << 16));
        }

        public static void Resume()
        {
            if (!IsPlaying() && IsRunning())
            {
                SendMediaPauseResume();
                Log.WriteLine("Resuming Spotify");
            }
        }
        public static bool IsRunning()
        {
            return Process.GetProcessesByName("Spotify").Length > 0;
        }
        public static string CurrentTrackName()
        {
            if (!IsPlaying())
                return null;
            Process main = GetProcess();
            return main.MainWindowTitle.Split(new string[] { " - " }, 2,StringSplitOptions.None)[1];
        }
        public static string CurrentAuthorName()
        {
            if (!IsPlaying())
                return null;
            Process main = GetProcess();
            return main.MainWindowTitle.Split(new string[] { " - " }, 2, StringSplitOptions.None)[0];
        }
        public static bool IsPlaying()
        {
            Process main = GetProcess();
            if (main == null)
                return false;
            // create a substring which checks if there is Spotify at the start of the main window handle
            return main.MainWindowTitle.Substring(0, "Spotify".Length) != "Spotify";
        }
        private static Process GetProcess()
        {
            Process spotifyProc = Process.GetProcessesByName("Spotify").Where(p => p.MainWindowTitle != "").ToArray()[0];
            return spotifyProc;
        }
    }
}
