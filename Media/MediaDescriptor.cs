using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media
{
    public class MediaDescriptor
    {
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

        public uint Bitrate
        {
            get;
            set;
        }

        public CodecFlagsEnum Flags
        {
            get;
            set;
        }

        public byte[] ExtraData
        {
            get;
            set;
        }

        public string RecommendedEncoderConfiguration
        {
            get;
            set;
        }
    }
}
