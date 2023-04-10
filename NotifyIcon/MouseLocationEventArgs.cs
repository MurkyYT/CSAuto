// Notify Icon Copyright © 2019-2020 by Ronald M. Martin
// This software is licensed under the Code Project Open License.  See the Licenses.txt file.

using System;
using System.Windows;

namespace NotifyIconLibrary.Events
{
    /// <summary>
    /// This class adds the following property to the standard <c>EventArgs</c>:
    /// <para>
    /// A System.Windows.Point to indicate a location in in DPI-aware WPF screen coordinates.
    /// </para>
    /// </summary>
    public class MouseLocationEventArgs : EventArgs
    {
        /// <summary>
        /// Get the <c>P</c> (as in Point) property.
        /// </summary>
        public Point P { get; }

        /// <summary>
        /// This is the default class constructor for the <c>MouseLocationEventArgs</c> class.
        /// </summary>
        /// <remarks>
        /// It is private to prevent it from being invoked.
        /// </remarks>
        // ReSharper disable once UnusedMember.Local
        private MouseLocationEventArgs()
        {
        }

        /// <summary>
        /// This is the functional class constructor for the <c>MouseLocationEventArgs</c> class.
        /// </summary>
        /// <param name="p">This is the value for the <c>P</c> property.</param>
        public MouseLocationEventArgs(Point p)
        {
            P = p;
        }
    }
}
