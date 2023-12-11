using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Murky.Utils.CSGO
{
    public static class ConDump
    {
        //public static event EventHandler SearchStarted;
        //public static event EventHandler OnChange;
        public static int Delay = 500;
        private static IEnumerable<string> oldFile = Enumerable.Empty<string>();
        private static readonly string path;
        private static readonly Thread workThread;
        static ConDump()
        {
            string csgoDir = Steam.GetGameDir("Counter-Strike Global Offensive");
            if (csgoDir != null)
                path = csgoDir + "\\game\\csgo\\";
            workThread = new Thread(CheckForChange);
        }
        public static void StartListening()
        {
            if (workThread != null && workThread.ThreadState == ThreadState.Unstarted)
                workThread.Start();
            if (workThread != null && workThread.ThreadState == ThreadState.Suspended)
                workThread.Resume();
        }
        public static void StopListening()
        {
            if (workThread != null && workThread.ThreadState == ThreadState.Running)
                workThread.Suspend();
        }
        private static void CheckForChange()
        {
            while (true) 
            {
                IEnumerable<string> newFile = ReadFile(path + "console.log");
                IEnumerable<string> diff = newFile.Except(oldFile);
                if (diff.Count() > 0)
                    OnChanged(diff);
                oldFile = newFile.ToList();
                Thread.Sleep(Delay);
            }
        }
        private static IEnumerable<string> ReadFile(string path)
        {
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                string line = "";
                using (StreamReader reader = new StreamReader(stream))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        yield return line;
                    }
                }
            }
        }
        private static void OnChanged(IEnumerable<string> diff)
        {
            try
            {
                foreach (string str in diff)
                {
                    Log.WriteLine(str);
                }
            }
            catch { }
        }
    }
}
