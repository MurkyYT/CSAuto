using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CSAuto.Utils
{
    public static class Spotify
    {
        public static void Pause()
        {
            if (IsPlaying())
                Keyboard.PressKey(Keyboard.VirtualKeyStates.VK_MEDIA_PLAY_PAUSE);
        }
        public static void Resume()
        {
            if (!IsPlaying())
                Keyboard.PressKey(Keyboard.VirtualKeyStates.VK_MEDIA_PLAY_PAUSE);
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
            Process[] spotifyProcs = Process.GetProcessesByName("Spotify");
            Process spotifyProc = null;
            for (int i = 0; i < spotifyProcs.Length && spotifyProc == null; i++)
            {
                if (spotifyProcs[i].MainWindowTitle != "")
                    spotifyProc = spotifyProcs[i];
            }
            return spotifyProc;
        }
    }
}
