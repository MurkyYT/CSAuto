using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Murky.Utils.CSGO
{
    public static class ConDump
    {
        //public static event EventHandler SearchStarted;
        public static event EventHandler OnChange;
        private static FileSystemWatcher fileWathcer = new FileSystemWatcher();
        private static int lastLineIndex = 0;
        private static readonly string path;
        private static readonly string fileName = "console.log";
        static ConDump()
        {
            string csgoDir = Steam.GetGameDir("Counter-Strike Global Offensive").ElementAtOrDefault(0);
            if (csgoDir != null)
                path = csgoDir + "\\game\\csgo";
            fileWathcer.Path = path;
            fileWathcer.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.Size;
            fileWathcer.Filter = fileName;
            fileWathcer.Changed += FileWathcer_Changed;
        }
        public static void StartListening()
        { 
            fileWathcer.EnableRaisingEvents = true;
        }

        private static void FileWathcer_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                byte[] buffer;
                using (FileStream file = File.Open(path + "\\" + fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    buffer = new byte[file.Length];
                    file.Read(buffer, 0, buffer.Length);
                }
                string result = UTF8Encoding.UTF8.GetString(buffer);
                char[] fileArr = result.ToCharArray();
                int index = fileArr.Length - 1;
                string resultChanged = "";
                while (index >= lastLineIndex)
                {
                    resultChanged = fileArr[index] + resultChanged;
                    index--;
                }
                lastLineIndex = fileArr.Length;
                OnChange?.Invoke(resultChanged, null);
            }
            catch { }
        }

        public static void StopListening()
        {
            fileWathcer.EnableRaisingEvents = false;
        }
    }
}
