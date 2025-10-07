using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Murky.Utils
{
    class Log
    {
        private const int MaxMemoryLog = 512;
        public static CSAuto.GUIWindow debugWind { 
            get { lock (_debugWindLock) { return _debugWind; } } 
            set { lock (_debugWindLock) { _debugWind = value; } } }
        private static CSAuto.GUIWindow _debugWind = null;
        private static object _debugWindLock = new object();
        private static List<string> memoryLog = new List<string>();
        static string strWorkPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string path = strWorkPath + "\\debug\\logs\\";
        static string lineTemplate = "[%date%] (%caller%) %message%";
        public static string Path { get { return path; } }
        public static string WorkPath { get { return strWorkPath; } }
        public static string MemoryLog 
        { 
            get 
            { 
                lock(memoryLog) 
                {
                    return string.Join(Environment.NewLine, memoryLog) + Environment.NewLine;
                }
            } 
        }
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
            catch {  }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(object lines, string level = "Info", string caller = "")
        {
            if (lines == null)
                lines = "";
            StackFrame frm = new StackFrame(1, false);
            lines = lineTemplate.Replace("%date%", DateTime.Now.ToString("HH:mm:ss")).
                Replace("%level%", level).
                Replace("%caller%", caller == "" ? frm.GetMethod().Name : caller).
                Replace("%message%", lines.ToString()).ToString();
            lock (_debugWindLock)
            {
                debugWind?.UpdateDebug(lines.ToString());
            }
            Debug.WriteLine(lines);

            lock (memoryLog)
            {
                if (memoryLog.Count >= MaxMemoryLog)
                    memoryLog.RemoveAt(0);

                memoryLog.Add(lines.ToString());
            }

            if (CSAuto.Properties.Settings.Default.saveLogs || (Application.Current as CSAuto.App)?.LogArg == true)
            {
                VerifyDir();
                string fileName = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + "_Log.txt";
                try
                {
                    StreamWriter file = new StreamWriter(path + fileName, true);
                    file.Write(lines);
                    file.Close();
                }
                catch { }
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(object lines,string level = "Info",string caller = "")
        {
            if (lines == null)
                lines = "";
            StackFrame frm = new StackFrame(1, false);
            lines = lineTemplate.Replace("%date%", DateTime.Now.ToString("HH:mm:ss")).
                Replace("%level%", level).
                Replace("%caller%", caller == "" ? frm.GetMethod().Name : caller).
                Replace("%message%", lines.ToString()).ToString();
            lock (_debugWindLock)
            {
                debugWind?.UpdateDebug(lines.ToString());
            }

            Debug.WriteLine(lines);

            lock (memoryLog)
            {
                if (memoryLog.Count >= MaxMemoryLog)
                    memoryLog.RemoveAt(0);

                memoryLog.Add(lines.ToString());
            }

            if (CSAuto.Properties.Settings.Default.saveLogs || (Application.Current as CSAuto.App)?.LogArg == true)
            {
                VerifyDir();
                string fileName = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + "_Log.txt";
                try
                {
                    StreamWriter file = new StreamWriter(path + fileName, true);
                    file.WriteLine(lines);
                    file.Close();
                }
                catch {  }
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Error(object lines, string level = "Info", string caller = "")
        {
            if (lines == null)
                lines = "";
            lines = $"{DateTime.Now:dd/MM/yyyy HH:mm:ss} CRITICAL - {lines}";
            lock (_debugWindLock)
            {
                debugWind?.UpdateDebug(lines.ToString());
            }
            Debug.WriteLine(lines);

            lock (memoryLog)
            {
                if (memoryLog.Count >= MaxMemoryLog)
                    memoryLog.RemoveAt(0);

                memoryLog.Add(lines.ToString());
            }

            string fileName = "Error_Log.txt";
            try
            {
                StreamWriter file = new StreamWriter(strWorkPath + "\\"+fileName, true);
                file.WriteLine(lines);
                file.Close();
            }
            catch { }
        }
    }
}