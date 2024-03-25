using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Point = System.Drawing.Point;

namespace CSAuto
{
    public static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeregisterShellHookWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SwitchToThisWindow(IntPtr hWnd,bool fUnknown);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        public static IntPtr HSHELL_FLASH = IntPtr.Add(IntPtr.Zero, 0x8006);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterShellHookWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll", EntryPoint = "GetCursorInfo")]
        public static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll", EntryPoint = "CopyIcon")]
        public static extern IntPtr CopyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public Int32 cbSize;        // Specifies the size, in bytes, of the structure. 
            public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
            public IntPtr hCursor;          // Handle to the cursor. 
            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
        }
        public static Process GetForegroundProcess()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                return null;
            GetWindowThreadProcessId(hWnd, out uint prcsId);
            if (prcsId != 0)
                return Process.GetProcessById((int)prcsId);
            return null;
        }
        public static bool BringToFront(IntPtr hWnd)
        {
            if (GetForegroundWindow() == hWnd)
                return false;
            bool res = SetForegroundWindow(hWnd);
            if (GetForegroundWindow() != hWnd)
            {
                bool res1 = SwitchToThisWindow(hWnd, true);
                bool res2 = SetForegroundWindow(hWnd);
                return res1 && res2;
            }
            return res;
        }
        public static CURSORINFO GetCursorInfo()
        {
            CURSORINFO curin = new CURSORINFO();
            curin.cbSize = Marshal.SizeOf(curin);
            if (GetCursorInfo(out curin))
                return curin;
            return curin;
        }
        public static IntPtr GetCursorHandle()
        {
            CURSORINFO curin = GetCursorInfo();
            if(curin.hCursor != IntPtr.Zero)
                return curin.hCursor;
            return IntPtr.Zero;
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, UInt32 uFlags);

        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(IntPtr hWnd);
        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }

        // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
        // what value of the enum to set.
        // Copied from dwmapi.h
        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        // Import dwmapi.dll and define DwmSetWindowAttribute in C# corresponding to the native function.
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void DwmSetWindowAttribute(IntPtr hwnd,
                                                         DWMWINDOWATTRIBUTE attribute,
                                                         ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                         uint cbAttribute);
        public static Process GetProccesByWindowName(string windowName,out bool success, string className = null, string processName = null)
        {
            success = false;
            IntPtr hwnd = FindWindow(className, windowName);
            if (hwnd == IntPtr.Zero)
                return null;
            GetWindowThreadProcessId(hwnd, out uint pid);
            Process res = Process.GetProcessById((int)pid);
            if (processName == null)
            {
                success = true;
                return res;
            }
            if (res.ProcessName == processName)
            {
                success = true;
                return res;
            }
            return null;
        }
        public static void OptimizeMemory()
        {
            IntPtr pHandle = GetCurrentProcess();
            //SetProcessWorkingSetSize(pHandle, min, max);
            EmptyWorkingSet(pHandle);
        }
        // For Windows Mobile, replace user32.dll with coredll.dll
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(IntPtr parentHandle, string lpClassName, string lpWindowName);
        [DllImport("psapi")]
        public static extern bool EmptyWorkingSet(IntPtr hProcess);
        [DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetProcessWorkingSetSize(IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);
        [DllImport("KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetCurrentProcess();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public static bool IsForegroundProcess(uint pid)
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == null) return false;

            if (GetWindowThreadProcessId(hwnd, out uint foregroundPid) == (IntPtr)0) return false;

            return (foregroundPid == pid);
        }
        public static unsafe void ReplaceColor(Bitmap target,
                          int x,
                          int y,
                          Color color)
        {
            const int pixelSize = 4; // 32 bits per pixel

            BitmapData targetData = null;

            try
            {
                targetData = target.LockBits(
                  new Rectangle(0, 0, target.Width, target.Height),
                  ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                byte* targetRow = (byte*)targetData.Scan0 + (y * targetData.Stride);


                targetRow[x * pixelSize + 0] = color.B;
                targetRow[x * pixelSize + 1] = color.G;
                targetRow[x * pixelSize + 2] = color.R;
                targetRow[x * pixelSize + 3] = color.A;
            }
            finally
            {
                target.UnlockBits(targetData);
            }
        }
        public unsafe static Color GetPixel(Bitmap target, int x, int y)
        {
            const int pixelSize = 4; // 32 bits per pixel

            BitmapData targetData = null;
            Color res;
            try
            {
                targetData = target.LockBits(
                    new Rectangle(0, 0, target.Width, target.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                byte* targetRow = (byte*)targetData.Scan0 + (y * targetData.Stride);

                res = Color.FromArgb(targetRow[x * pixelSize + 2],
                    targetRow[x * pixelSize + 1],
                    targetRow[x * pixelSize + 0]);
                //targetRow[x * pixelSize + 0] = color.B;
                //targetRow[x * pixelSize + 1] = color.G;
                //targetRow[x * pixelSize + 2] = color.R;
                //targetRow[x * pixelSize + 3] = color.A;
            }
            finally
            {
                target.UnlockBits(targetData);
            }
            return res;
        }
        public static unsafe void CopyRegionIntoImage(Bitmap srcBitmap, Rectangle srcRegion, ref Bitmap destBitmap, Rectangle destRegion)
        {
            const int pixelSize = 4; // 32 bits per pixel

            BitmapData targetData = null,sourceData = null;
            try
            {
                targetData = destBitmap.LockBits(
                    new Rectangle(0, 0, destBitmap.Width, destBitmap.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                sourceData = srcBitmap.LockBits(
                    new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                for (int y = srcRegion.Y; y < srcRegion.Height + srcRegion.Y; y++)
                {
                    for (int x = srcRegion.X; x < srcRegion.Width + srcRegion.X; x++)
                    {
                        byte* targetRow = (byte*)targetData.Scan0 + ((destRegion.Y + y - srcRegion.Y) * targetData.Stride);
                        byte* source = (byte*)sourceData.Scan0 + (y * sourceData.Stride);

                        targetRow[(x + destRegion.X - srcRegion.X) * pixelSize + 0] = source[x * pixelSize + 0];
                        targetRow[(x + destRegion.X - srcRegion.X) * pixelSize + 1] = source[x * pixelSize + 1];
                        targetRow[(x + destRegion.X - srcRegion.X) * pixelSize + 2] = source[x * pixelSize + 2];
                        targetRow[(x + destRegion.X - srcRegion.X) * pixelSize + 3] = source[x * pixelSize + 3];
                    }
                }
            }
            finally
            {
                destBitmap.UnlockBits(targetData);
                srcBitmap.UnlockBits(sourceData);
            }
        }
    }
}
