using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAuto
{
    static class Extensions
    {
        public static int IndexOf(this Array arr, object obj)
        {
            for (int i = 0; i < arr.Length; i++)
                if (obj.Equals(arr.GetValue(i)))
                    return i;
            return -1;
        }
    }
}
