using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Murky.Utils
{
    public static class Spotify
    {
        /*
all the appcomandcodes for utilizing other music/video players
public enum AppComandCode : uint
{
    BASS_BOOST = 20,
    BASS_DOWN = 19,
    BASS_UP = 21,
    BROWSER_BACKWARD = 1,
    BROWSER_FAVORITES = 6,
    BROWSER_FORWARD = 2,
    BROWSER_HOME = 7,
    BROWSER_REFRESH = 3,
    BROWSER_SEARCH = 5,
    BROWSER_STOP = 4,
    LAUNCH_APP1 = 17,
    LAUNCH_APP2 = 18,
    LAUNCH_MAIL = 15,
    LAUNCH_MEDIA_SELECT = 16,
    MEDIA_NEXTTRACK = 11,
    MEDIA_PLAY_PAUSE = 14,
    MEDIA_PREVIOUSTRACK = 12,
    MEDIA_STOP = 13,
    TREBLE_DOWN = 22,
    TREBLE_UP = 23,
    VOLUME_DOWN = 9,
    VOLUME_MUTE = 8,
    VOLUME_UP = 10,
    MICROPHONE_VOLUME_MUTE = 24,
    MICROPHONE_VOLUME_DOWN = 25,
    MICROPHONE_VOLUME_UP = 26,
    CLOSE = 31,
    COPY = 36,
    CORRECTION_LIST = 45,
    CUT = 37,
    DICTATE_OR_COMMAND_CONTROL_TOGGLE = 43,
    FIND = 28,
    FORWARD_MAIL = 40,
    HELP = 27,
    MEDIA_CHANNEL_DOWN = 52,
    MEDIA_CHANNEL_UP = 51,
    MEDIA_FASTFORWARD = 49,
    MEDIA_PAUSE = 47,
    MEDIA_PLAY = 46,
    MEDIA_RECORD = 48,
    MEDIA_REWIND = 50,
    MIC_ON_OFF_TOGGLE = 44,
    NEW = 29,
    OPEN = 30,
    PASTE = 38,
    PRINT = 33,
    REDO = 35,
    REPLY_TO_MAIL = 39,
    SAVE = 32,
    SEND_MAIL = 41,
    SPELL_CHECK = 42,
    UNDO = 34,
    DELETE = 53,
    DWM_FLIP3D = 54
}
         */
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private const int WM_APPCOMMAND = 0x0319;
        public static void Pause()
        {
            if (IsPlaying())
            {
                SendMediaPauseResume();
                Log.WriteLine("|Spotify.cs| Pausing Spotify");
            }
        }

        private static void SendMediaPauseResume()
        {
            Process prc = GetProcess();
            if (prc == null)
                return;
            SendMessageW(prc.MainWindowHandle, WM_APPCOMMAND, prc.MainWindowHandle, (IntPtr)(14 << 16));
        }

        public static void Resume()
        {
            if (!IsPlaying())
            {
                SendMediaPauseResume();
                Log.WriteLine("|Spotify.cs| Resuming Spotify");
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
            return !main.MainWindowTitle.StartsWith("Spotify");
        }
        private static Process GetProcess()
        {
            if (!IsRunning()) return null;
            Process spotifyProc = Process.GetProcessesByName("Spotify").Where(p => p.MainWindowTitle != "").ToList().FirstOrDefault();
            return spotifyProc;
        }
    }
}
