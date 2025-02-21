using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Media.Control;

namespace Murky.Utils
{
    public static class Music
    {
        public static async void Pause()
        {
            var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            var session = sessionManager?.GetCurrentSession();

            if (session != null)
                await session?.TryPauseAsync();
        }

        public static async void Resume()
        {
            var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            var session = sessionManager?.GetCurrentSession();

            if(session != null)
                await session?.TryPlayAsync();
        }
        public static string CurrentTrackName()
        {
            var sessionManager = GlobalSystemMediaTransportControlsSessionManager.RequestAsync()?.GetResults();
            var session = sessionManager?.GetCurrentSession();
            if (session == null)
                return string.Empty;
            var mediaProperties = session.TryGetMediaPropertiesAsync()?.GetResults();

            return mediaProperties?.Title ?? string.Empty;
        }
        public static string CurrentAuthorName()
        {
            var sessionManager = GlobalSystemMediaTransportControlsSessionManager.RequestAsync()?.GetResults();
            var session = sessionManager?.GetCurrentSession();
            if (session == null)
                return string.Empty;

            var mediaProperties = session.TryGetMediaPropertiesAsync()?.GetResults();

            return mediaProperties?.Artist ?? string.Empty;
        }
        public static bool IsPlaying()
        {
            var sessionManager = GlobalSystemMediaTransportControlsSessionManager.RequestAsync()?.GetResults();
            var session = sessionManager?.GetCurrentSession();
            if (session == null)
                return false;

            return session.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
        }
    }
}
