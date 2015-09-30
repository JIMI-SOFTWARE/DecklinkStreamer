using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFM2
{
    public static class FFM
    {
        /* The FFM file is made of blocks of fixed size */
        public const int FFM_HEADER_SIZE = 14;
        public const int FFM_PACKET_SIZE = 4096;
        public const uint PACKET_ID = 0x666d;

        /* each packet contains frames (which can span several packets */
        public const int FRAME_HEADER_SIZE = 16;
        public const uint FLAG_KEY_FRAME = 0x01;
        public const uint FLAG_DTS = 0x02;
    }
}
