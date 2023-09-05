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
        [DllImport(dllname)]
        private static extern IntPtr InitCapture();
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeInitCapture(IntPtr ptr);
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool UpdateFrame(IntPtr ptr);
        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetBitmap(IntPtr ptr);
        private IntPtr _handle;
        public DXGICapture()
        {
            _handle = InitCapture();
        }
        public IntPtr GetCapture()
        {
            if(UpdateFrame(_handle))
                return GetBitmap(_handle);
            return IntPtr.Zero;
        }
        public void DeInit()
        {
            DeInitCapture(_handle);
        }
    }
}
