using System;
using System.Runtime.InteropServices;

namespace Murky.Utils
{
    public class DXGICapture
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DXGI_ADAPTER_DESC1
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string Description;

            public uint VendorId;
            public uint DeviceId;
            public uint SubSysId;
            public uint Revision;

            public uint DedicatedVideoMemory;
            public uint DedicatedSystemMemory;
            public uint SharedSystemMemory;

            public LUID AdapterLuid;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DXGI_OUTPUT_DESC
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;

            public RECT DesktopCoordinates;
            [MarshalAs(UnmanagedType.Bool)]
            public bool AttachedToDesktop;
            public DXGI_MODE_ROTATION Rotation;
            public IntPtr Monitor;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public enum DXGI_MODE_ROTATION
        {
            DXGI_MODE_ROTATION_UNSPECIFIED = 0,
            DXGI_MODE_ROTATION_IDENTITY = 1,
            DXGI_MODE_ROTATION_ROTATE90 = 2,
            DXGI_MODE_ROTATION_ROTATE180 = 3,
            DXGI_MODE_ROTATION_ROTATE270 = 4
        }

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

        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DXGI_GetAdapterDescription(int index, out DXGI_ADAPTER_DESC1 desc);

        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DXGI_AdaptersCount();

        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DXGI_GetOutputDescription(int index, out DXGI_OUTPUT_DESC desc);

        [DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DXGI_OutputsCount();

        public bool Enabled
        {
            get
            {
                lock (this)
                {
                    try { return DXGI_IsEnabled(); }
                    catch { return false; }
                }
            }
        }

        public int AdaptersCount
        {
            get
            {
                lock (this)
                {
                    try { return DXGI_AdaptersCount(); }
                    catch { return 0; }
                }
            }
        }

        public int OutputsCount
        {
            get
            {
                lock (this)
                {
                    try { return DXGI_OutputsCount(); }
                    catch { return 0; }
                }
            }
        }

        public void Init()
        {
            if (!Enabled)
            {
                lock (this) DXGI_InitCapture();
            }
        }

        public void SetTimeout(uint ms)
        {
            if (!Enabled)
            {
                lock (this) DXGI_SetTimeout(ms);
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
                lock (this) DXGI_DeInitCapture();
            }
        }

        public DXGI_ADAPTER_DESC1 GetAdapterDescription(int index)
        {
            lock (this)
            {
                if (!Enabled) return new DXGI_ADAPTER_DESC1();

                DXGI_ADAPTER_DESC1 desc;
                DXGI_GetAdapterDescription(index, out desc);
                return desc;
            }
        }

        public DXGI_OUTPUT_DESC GetOutputDescription(int index)
        {
            lock (this)
            {
                if (!Enabled) return new DXGI_OUTPUT_DESC();

                DXGI_OUTPUT_DESC desc;
                DXGI_GetOutputDescription(index, out desc);
                return desc;
            }
        }
    }
}
