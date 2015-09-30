using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Media;

namespace FFM2
{
    public class FFM2Muxer
    {
        private List<MediaDescriptor> m_tracks;
        private FFM2HeaderFormatter m_headerFormatter;

        public FFM2Muxer(Stream target)
        {
            this.TargetStream = target;
            m_tracks = new List<MediaDescriptor>();
            m_headerFormatter = new FFM2HeaderFormatter(m_tracks);
        }

        protected Stream TargetStream
        {
            get;
            private set;
        }

        public int AddTrack(MediaDescriptor trackDescriptor)
        {
            m_tracks.Add(trackDescriptor);
            return m_tracks.Count - 1;
        }

        public void WriteFrame(MediaFrame frame)
        {

        }

        public void WriteHeader()
        {
            m_headerFormatter.RenderHeader(TargetStream);
        }
    }
}
