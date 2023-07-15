// System Information Copyright © 2019-2020 by Ronald M. Martin
// This software is licensed under the Code Project Open License.  See the Licenses.txt file.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SystemInformationLibrary
{
    /// <summary>
    /// This class contains import declarations to allow us to call methods in the Win32 API to supplement the
    /// facilities provided by WPF and the foundation class libraries.
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport(ExternDll.User32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int GetSystemMetrics(int nIndex);
    }
}
