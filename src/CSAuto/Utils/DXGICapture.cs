using System;
using System.Runtime.InteropServices;
using System.Web.UI.WebControls;

namespace Murky.Utils
{
    public class DXGICapture
    {
        const int DXGICAPTURE_ALL_SCREENS = -1;
        const string dllname = "bin\\DXGICapture.dll";
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DXGI_InitCapture();
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DXGI_DeInitCapture();
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DXGI_CaptureScreen(int index);
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DXGI_IsEnabled();

        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DXGI_SetTimeout(uint ms);
        public bool Enabled
        {
            get
            {
                lock (this)
                {
                    try
                    {
                        return DXGI_IsEnabled();
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
                    DXGI_InitCapture();
                }
            }
        }
        public void SetTimeout(uint ms)
        {
            if (!Enabled)
            {
                lock (this)
                {
                    DXGI_SetTimeout(ms);
                }
            }
        }
        public IntPtr GetCapture() 
        {
            lock (this)
            {
                return Enabled ? DXGI_CaptureScreen(DXGICAPTURE_ALL_SCREENS) : IntPtr.Zero;
            }
        }
        public void DeInit() 
        {
            if (Enabled)
            {
                lock (this)
                {
                    DXGI_DeInitCapture();
                }
            }
        }
    }
}
