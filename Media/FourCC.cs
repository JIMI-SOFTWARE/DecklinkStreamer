using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media
{
    public class FourCC
    {
        public static UInt32 Make(char a, char b, char c, char d)
        {
            return (UInt32)(((int)a << 24) | ((int)b) << 16 | ((int)c) << 8 | ((int)d));
        }

        public static UInt32 Make(string fourcc)
        {
            if (string.IsNullOrEmpty(fourcc) || fourcc.Length != 4) throw new ArgumentException("fourcc string should be exactly 4 characters long");
            return Make(fourcc[0], fourcc[1], fourcc[2], fourcc[3]);
        }
    }
}
