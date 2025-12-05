using DiscordRPC.Logging;
using Murky.Utils;

namespace CSAuto
{
    /// <summary>
    /// Logs the outputs to the default logger
    /// </summary>
    public class UtilsLogger : ILogger
    {
        public LogLevel Level { get; set; }

        public UtilsLogger()
            : this(LogLevel.Info) { }

        public UtilsLogger(LogLevel level)
        {
            Level = level;
        }
        public void Trace(string message, params object[] args)
        {
            if (Level > LogLevel.Trace) return;
            Log.WriteLine("[Discord RPC] " + (args.Length > 0 ? string.Format(message, args) : message));
        }
        public void Info(string message, params object[] args)
        {
            if (Level > LogLevel.Info) return;
            Log.WriteLine("[Discord RPC] " + (args.Length > 0 ? string.Format(message, args) : message));
        }
        public void Warning(string message, params object[] args)
        {
            if (Level > LogLevel.Warning) return;
            Log.WriteLine("[Discord RPC] " + (args.Length > 0 ? string.Format(message, args) : message));
        }
        public void Error(string message, params object[] args)
        {
            if (Level > LogLevel.Error) return;
            Log.WriteLine("[Discord RPC] " + (args.Length > 0 ? string.Format(message, args) : message));
        }

    }
}
