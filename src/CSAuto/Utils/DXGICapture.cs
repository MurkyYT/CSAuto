using System;
using System.Runtime.InteropServices;

namespace Murky.Utils
{
    public class DXGICapture
    {
        const string dllname = "bin\\DXGICapture.dll";
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool InitCapture();
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeInitCapture();
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr UpdateFrame();
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
        public void Init() => InitCapture();
        public IntPtr GetCapture() 
        {
            lock (this)
            {
                return Enabled ? UpdateFrame() : IntPtr.Zero;
            }
        }
        public void DeInit() 
        {
            lock (this)
            {
                DeInitCapture();
            }
        }
    }
}
