// Notify Icon Copyright © 2019-2020 by Ronald M. Martin
// This software is licensed under the Code Project Open License.  See the Licenses.txt file.

namespace NotifyIconLibrary
{
    /// <summary>
    /// This class is used by the NativeMethods class to make references to system dynamic link libraries more concise,
    /// less error-prone, searchable as code references and readily able to change the names of libraries.
    /// </summary>
    internal class ExternDll
    {
        public const string User32 = "user32.dll";
        public const string Shell32 = "shell32.dll";
    }
}