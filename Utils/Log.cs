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
        static string lineTemplate = "[%date%] (%caller%) - %message%";
        public static string Path { get { return path; } }
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
        public static void Write(object lines, string level = "Info", string caller = "")
        {
            StackFrame frm = new StackFrame(1, false);
            lines = lineTemplate.Replace("%date%", DateTime.Now.ToString("HH:mm:ss")).
                Replace("%level%", level).
                Replace("%caller%", caller == "" ? frm.GetMethod().Name : caller).
                Replace("%message%", lines.ToString()).ToString();
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
                catch { }
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(object lines,string level = "Info",string caller = "")
        {
            StackFrame frm = new StackFrame(1, false);
            lines = lineTemplate.Replace("%date%", DateTime.Now.ToString("HH:mm:ss")).
                Replace("%level%", level).
                Replace("%caller%", caller == "" ? frm.GetMethod().Name : caller).
                Replace("%message%", lines.ToString()).ToString();
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
                catch {  }
            }
        }
    }
}