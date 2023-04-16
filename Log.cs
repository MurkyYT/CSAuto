using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAuto
{
    class Log
    {
        public static bool saveLogs = false;
        public static GSIDebugWindow debugWind = null;
        public static void VerifyDir(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (!dir.Exists)
                {
                    dir.Create();
                }
            }
            catch { }
        }

        public static void Write(string lines)
        {
            lines = $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second},{DateTime.Now.Millisecond}] - " + lines;
            if (debugWind != null)
                debugWind.UpdateDebug(lines);
            if (saveLogs)
            {
                string path = "DEBUG/logs/";
                VerifyDir(path);
                string fileName = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + "_Log.txt";
                try
                {

                    System.IO.StreamWriter file = new System.IO.StreamWriter(path + fileName, true);
                    Debug.Write(lines);
                    file.Write(lines);
                    file.Close();

                }
                catch (Exception) { }
            }
        }
        public static void WriteLine(string lines)
        {
            lines = $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second},{DateTime.Now.Millisecond}] - " + lines;
            if (debugWind != null)
                debugWind.UpdateDebug(lines);
            if (saveLogs)
            {
                string path = "DEBUG/logs/";
                VerifyDir(path);
                string fileName = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + "_Log.txt";
                try
                {

                    System.IO.StreamWriter file = new System.IO.StreamWriter(path + fileName, true);
                    Debug.WriteLine(lines);
                    file.WriteLine(lines);
                    file.Close();

                }
                catch (Exception) { }
            }
        }
    }
}