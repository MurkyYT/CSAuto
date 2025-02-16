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

            await sessionManager.GetCurrentSession().TryPauseAsync();
        }

        public static async void Resume()
        {
            var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            await sessionManager.GetCurrentSession().TryPlayAsync();
        }
        public static string CurrentTrackName()
        {
            var sessionManager = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetResults();
            var mediaProperties = sessionManager.GetCurrentSession().TryGetMediaPropertiesAsync().GetResults();

            return mediaProperties?.Title ?? string.Empty;
        }
        public static string CurrentAuthorName()
        {
            var sessionManager = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetResults();
            var mediaProperties = sessionManager.GetCurrentSession().TryGetMediaPropertiesAsync().GetResults();

            return mediaProperties?.Artist ?? string.Empty;
        }
        public static bool IsPlaying()
        {
            var sessionManager = GlobalSystemMediaTransportControlsSessionManager.RequestAsync().GetResults();
            var mediaProperties = sessionManager.GetCurrentSession().TryGetMediaPropertiesAsync().GetResults();

            return sessionManager.GetCurrentSession().GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
        }
    }
}
