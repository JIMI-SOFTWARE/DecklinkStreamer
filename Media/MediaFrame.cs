using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media
{
    public class MediaFrame
    {
        public double DTS
        {
            get;
            set;
        }

        public ArraySegment<byte> Data
        {
            get;
            set;
        }

        public CodecTypeEnum CodecType
        {
            get;
            set;
        }

        public CodecEnum Codec
        {
            get;
            set;
        }
    }
}
