using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    public static class Log
    {
        static string strWorkPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string path = strWorkPath;
        static string lineTemplate = "[%date%] (%caller%) - %message%";

        public static string WorkPath { get { return strWorkPath; } }
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
            catch { }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(object lines, string level = "Info", string caller = "")
        {
            if (lines == null)
                lines = "";
            StackFrame frm = new StackFrame(1, false);
            lines = lineTemplate.Replace("%date%", DateTime.Now.ToString("HH:mm:ss")).
                Replace("%level%", level).
                Replace("%caller%", caller == "" ? frm.GetMethod().Name : caller).
                Replace("%message%", lines.ToString()).ToString();
            Debug.WriteLine(lines);
            VerifyDir();
            string fileName = "\\Update_Log.txt";
            try
            {
                StreamWriter file = new StreamWriter(path + fileName, true);
                file.WriteLine(lines);
                file.Close();
            }
            catch { }
        }
    }
}
