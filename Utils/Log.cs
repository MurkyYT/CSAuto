using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(object lines, string level = "Info")
        {
            StackFrame frm = new StackFrame(1, false);
            lines = $"[{level}: {DateTime.Now.ToString("HH:mm:ss")}: {frm.GetMethod().Name}] " + lines.ToString();
            if (debugWind != null)
                debugWind.UpdateDebug(lines.ToString());
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
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(object lines,string level = "Info")
        {
            StackFrame frm = new StackFrame(1, false);
            lines = $"[{level}: {DateTime.Now.ToString("HH:mm:ss")}: {frm.GetMethod().Name}] " + lines.ToString();
            if (debugWind != null)
                debugWind.UpdateDebug(lines.ToString());
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