using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media
{
    public class AudioDescriptor: MediaDescriptor
    {
        public int SampleRate
        {
            get;
            set;
        }

        public int ChannelsCount
        {
            get;
            set;
        }
    }
}
