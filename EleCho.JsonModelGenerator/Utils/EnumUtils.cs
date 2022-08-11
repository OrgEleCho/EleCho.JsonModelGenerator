using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EleCho.JsonModelGenerator.Utils
{
    internal class EnumUtils
    {
        public static int NumValue(params bool[] bits)
        {
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    return i;
            }

            return -1;
        }
    }
}
