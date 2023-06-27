using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Murky.Utils
{
    class Log
    {
        public static CSAuto.GSIDebugWindow debugWind = null;
        static string strWorkPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string path = strWorkPath + "/DEBUG/LOGS/";
        public static void VerifyDir()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (!dir.Exists)
                {
                    dir.Create();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.StackTrace); }
        }

        public static void Write(string lines)
        {
            lines = $"[{DateTime.Now.ToString("HH:mm:ss")}] - " + lines;
            if (debugWind != null)
                debugWind.UpdateDebug(lines);
            if (CSAuto.Properties.Settings.Default.saveLogs)
            {
                VerifyDir();
                string fileName = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + "_Log.txt";
                try
                {
                    System.IO.StreamWriter file = new System.IO.StreamWriter(path + fileName, true);
                    Debug.Write(lines);
                    file.Write(lines);
                    file.Close();

                }
                catch (Exception ex) { MessageBox.Show(ex.StackTrace); }
            }
        }
        public static void WriteLine(string lines)
        {
            lines = $"[{DateTime.Now.ToString("HH:mm:ss")}] - " + lines;
            if (debugWind != null)
                debugWind.UpdateDebug(lines);
            if (CSAuto.Properties.Settings.Default.saveLogs)
            {
                VerifyDir();
                string fileName = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + "_Log.txt";
                try
                {
                    System.IO.StreamWriter file = new System.IO.StreamWriter(path + fileName, true);
                    Debug.WriteLine(lines);
                    file.WriteLine(lines);
                    file.Close();

                }
                catch (Exception ex) { MessageBox.Show(ex.StackTrace); }
            }
        }
    }
}