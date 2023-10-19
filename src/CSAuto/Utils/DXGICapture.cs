using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Murky.Utils
{
    public class DXGICapture
    {
        const string dllname = "DXGICapture.dll";
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool InitCapture();
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeInitCapture();
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr UpdateFrame();
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool IsEnabled();
        public bool Enabled => IsEnabled();
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
