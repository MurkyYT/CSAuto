// Ad Blocker Copyright © 2020 Ronald M. Martin
// This software is Licensed under the Code Project Open License.  See the Licenses.txt file.

using System;
using System.Windows;
using System.Windows.Input;

namespace NotifyIconLibrary.Events
{
    /// <summary>
    /// This class adds the following properties to the standard EventArgs:
    /// <para>
    /// A System.Windows.Input.MouseButton enumerator to indicate the active mouse button.
    /// </para>
    /// <para>
    /// An integer used to indicate the click count.  Expected to be in the range [0, 2] but don't
    /// count on it.  It is potentially useful for sorting out single and double clicks.
    /// </para>
    /// <para>
    /// A System.Windows.Point to indicate a location in in DPI-aware WPF screen coordinates.
    /// </para>
    /// </summary>
    public class MouseButtonEventArgs : EventArgs
    {
        /// <summary>
        /// Get or set the Button property.
        /// </summary>
        public MouseButton Button { get; set; }

        /// <summary>
        /// Get or set the Clicks property.
        /// </summary>
        public int Clicks { get; set; }

        /// <summary>
        /// Get or set the P(oint) property.
        /// </summary>
        public Point P { get; private set; }

        /// <summary>
        /// This is the parameterless class constructor for the Pointer Event Args class.
        /// </summary>
        private MouseButtonEventArgs()
        {
        }

        /// <summary>
        /// This is the functional class constructor for the Pointer Args class.
        /// </summary>
        /// <param name="button">This is the value for the Button property.</param>
        /// <param name="clicks">This is the value of the Clicks property.</param>
        /// <param name="p">This is the value for the P(oint) property.</param>
        public MouseButtonEventArgs(MouseButton button, int clicks, Point p)
        {
            Button = button;
            Clicks = clicks;
            P = p;
        }
    }
}
