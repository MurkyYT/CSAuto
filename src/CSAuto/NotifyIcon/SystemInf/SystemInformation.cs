// System Information Copyright © 2019-2020 by Ronald M. Martin
// This software is licensed under the Code Project Open License.  See the Licenses.txt file.

using DefinitionLibrary;

namespace SystemInformationLibrary
{
    /// <summary>
    /// This static class provides information about the host computer.
    /// </summary>
    public static class SystemInformation
    {
        /// <summary>
        /// Gets the dimensions of a default icon, in pixels.
        /// </summary>
        /// <returns>A <see cref="System.Drawing.Size"/> that represents the size, in pixels, of a default icon.</returns>
        public static System.Drawing.Size IconSize =>
            new System.Drawing.Size(NativeMethods.GetSystemMetrics(SystemMetrics.XIcon),
                                    NativeMethods.GetSystemMetrics(SystemMetrics.YIcon));

        /// <summary>
        /// Gets the dimensions of a small icon, in pixels.
        /// </summary>
        /// <returns>A <see cref="System.Drawing.Size"/> that represents the size, in pixels, of a small icon.</returns>
        public static System.Drawing.Size SmallIconSize =>
            new System.Drawing.Size(NativeMethods.GetSystemMetrics(SystemMetrics.XSmIcon),
                                    NativeMethods.GetSystemMetrics(SystemMetrics.YSmIcon));
    }
}