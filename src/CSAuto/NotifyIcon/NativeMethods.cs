// Notify Icon Copyright © 2019-2020 by Ronald M. Martin
// This software is licensed under the Code Project Open License.  See the Licenses.txt file.

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

// ReSharper disable InconsistentNaming

namespace NotifyIconLibrary
{
    /// <summary>
    /// This class contains import declarations to allow us to call methods in the Win32 API to supplement the
    /// facilities provided by WPF and the foundation class libraries, structure and class definitions to allow
    /// us to marshal data back and forth between managed and unmanaged code, and constants and enumerations
    /// that have special meaning to these API methods.
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        public static extern int RegisterWindowMessage(string lpString);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class NotifyIconData
        {
            public int cbSize = Marshal.SizeOf(typeof(NotifyIconData));
            public IntPtr hWnd;
            public int uID;
            public int uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public int dwState = 0;
            public int dwStateMask = 0;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public int uVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public int dwInfoFlags;
            public Guid guidItem;
            public IntPtr hBalloonIcon;
        }

        [DllImport(ExternDll.Shell32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Shell_NotifyIcon(int message, NotifyIconData pnId);
       
    }
}
