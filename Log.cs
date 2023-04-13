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
            if (saveLogs)
            {
                string path = "DEBUG/logs/";
                VerifyDir(path);
                string fileName = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + "_Log.txt";
                try
                {

                    System.IO.StreamWriter file = new System.IO.StreamWriter(path + fileName, true);
                    Debug.Write($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second},{DateTime.Now.Millisecond}] - " + lines);
                    file.Write($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second},{DateTime.Now.Millisecond}] - " + lines);
                    file.Close();

                }
                catch (Exception) { }
            }
        }
        public static void WriteLine(string lines)
        {
            if (saveLogs)
            {
                string path = "DEBUG/logs/";
                VerifyDir(path);
                string fileName = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + "_Log.txt";
                try
                {

                    System.IO.StreamWriter file = new System.IO.StreamWriter(path + fileName, true);
                    Debug.WriteLine($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second},{DateTime.Now.Millisecond}] - "+lines);
                    file.WriteLine($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second},{DateTime.Now.Millisecond}] - " + lines);
                    file.Close();

                }
                catch (Exception) { }
            }
        }
    }
}