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

namespace Murky.Utils
{
    public static class Spotify
    {
        public static void Pause()
        {
            if (IsPlaying() && IsRunning())
            {
                Keyboard.PressKey(Keyboard.VirtualKeyStates.VK_MEDIA_PLAY_PAUSE);
                Log.WriteLine("Pausing Spotify");
            }
        }
        public static void Resume()
        {
            if (!IsPlaying() && IsRunning())
            {
                Keyboard.PressKey(Keyboard.VirtualKeyStates.VK_MEDIA_PLAY_PAUSE);
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
