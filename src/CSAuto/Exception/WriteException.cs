using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAuto.Exceptions
{
    /// <summary>
    /// The exception when you cant write to a file.
    /// </summary>
    [Serializable]
    public class WriteException : Exception
    {
        public string FilePath { get; }
        public WriteException() { }

        public WriteException(string message)
            : base(message) { }
        public WriteException(string message,string filePath)
            : base(message) { FilePath = filePath; }
        public WriteException(string message, Exception inner)
            : base(message, inner) { }
    }
}
