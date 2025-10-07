using System;
using System.Runtime.InteropServices;

namespace Murky.Utils
{
    public class DXGICapture
    {
        const int DXGICAPTURE_ALL_SCREENS = -1;
        const string dllname = "bin\\DXGICapture.dll";
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool InitCapture();
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeInitCapture();
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CaptureScreen(int index);
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool IsEnabled();
        public bool Enabled
        {
            get
            {
                lock (this)
                {
                    try
                    {
                        return IsEnabled();
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }
        public void Init() 
        {
            if (!Enabled)
            {
                lock (this)
                {
                    InitCapture();
                }
            }
        }
        public IntPtr GetCapture() 
        {
            lock (this)
            {
                return Enabled ? CaptureScreen(DXGICAPTURE_ALL_SCREENS) : IntPtr.Zero;
            }
        }
        public void DeInit() 
        {
            if (Enabled)
            {
                lock (this)
                {
                    DeInitCapture();
                }
            }
        }
    }
}
