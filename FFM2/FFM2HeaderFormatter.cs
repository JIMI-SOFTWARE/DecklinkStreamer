using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Media;

namespace FFM2
{
    public class FFM2HeaderFormatter
    {
        private IEnumerable<MediaDescriptor> m_tracks;

        public FFM2HeaderFormatter(IEnumerable<MediaDescriptor> tracks)
        {
            m_tracks = tracks;
        }

        private void WriteHeaderChunk(Stream target, ArraySegment<byte> chunkData, uint id)
        {
            BitstreamWriter writer = new BitstreamWriter(target);
            writer.Put32BitsAligned(id);
            writer.Put32BitsAligned((uint)chunkData.Count);
            writer.PutAlignedBytes(chunkData.Array, chunkData.Offset, chunkData.Count);
        }

        private void WriteFFM2Chunk(Stream target)
        {
            BitstreamWriter writer = new BitstreamWriter(target);
            writer.Put32BitsAligned(FourCC.Make("FFM2"));
            writer.Put32BitsAligned(FFM.FFM_PACKET_SIZE);
            writer.Put64BitsAligned(0); //current write position
        }

        private void WriteMAINChunk(Stream target)
        {
            MemoryStream memoryStream = new MemoryStream();
            BitstreamWriter writer = new BitstreamWriter(memoryStream);

            writer.Put32BitsAligned((uint)m_tracks.Count());
            uint bitrate = 0;
            for (int i = 0; i < m_tracks.Count(); i++)
            {
                bitrate += m_tracks.ElementAt(i).Bitrate;
            }
            writer.Put32BitsAligned(bitrate);

            WriteHeaderChunk(
                target,
                new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Position),
                FourCC.Make("MAIN")
            );
        }

        private void WriteRecommendedConfiguration(Stream target, string config, uint tag)
        {
            MemoryStream memoryStream = new MemoryStream();
            BitstreamWriter writer = new BitstreamWriter(memoryStream);
            writer.PutNullTerminatedStringAligned(config);
            WriteHeaderChunk(
                target,
                new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Position),
                tag
            );
        }

        private void WriteCodecDescriptor(Stream target, MediaDescriptor descriptor)
        {
            MemoryStream memoryStream = new MemoryStream();
            BitstreamWriter writer = new BitstreamWriter(memoryStream);
            writer.Put32BitsAligned((uint)descriptor.Codec);
            writer.PutAlignedByte((byte)descriptor.CodecType);
            writer.Put32BitsAligned(descriptor.Bitrate);
            writer.Put32BitsAligned((uint)descriptor.Flags);
            writer.Put32BitsAligned(0); // flags2
            writer.Put32BitsAligned(0); // debug
            if (((uint)descriptor.Flags & (uint)CodecFlagsEnum.AV_CODEC_FLAG_GLOBAL_HEADER) != 0)
            {
                int extraDataSize = descriptor.ExtraData == null ? 0 : descriptor.ExtraData.Length;
                writer.Put32BitsAligned((uint)extraDataSize);
                if (extraDataSize > 0)
                {
                    writer.PutAlignedBytes(descriptor.ExtraData);
                }
            }
            WriteHeaderChunk(
                target,
                new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Position),
                FourCC.Make("COMM")
            );
            if (!string.IsNullOrEmpty(descriptor.RecommendedEncoderConfiguration))
            {
                switch (descriptor.CodecType)
                {
                    case CodecTypeEnum.Video: WriteRecommendedConfiguration(target, descriptor.RecommendedEncoderConfiguration, FourCC.Make("S2VI")); break;
                    case CodecTypeEnum.Audio: WriteRecommendedConfiguration(target, descriptor.RecommendedEncoderConfiguration, FourCC.Make("S2AU")); break;
                    default: throw new InvalidOperationException(string.Format("unsupported codec type: {0}", descriptor.CodecType));
                }
            }
        }

        public void RenderHeader(Stream target)
        {
            byte[] headerPacket = new byte[FFM.FFM_PACKET_SIZE];
            MemoryStream memoryTarget = new MemoryStream(headerPacket);
            WriteFFM2Chunk(memoryTarget);
            WriteMAINChunk(memoryTarget);
            foreach (MediaDescriptor descriptor in m_tracks)
            {
                WriteCodecDescriptor(memoryTarget, descriptor);
            }
            target.Write(headerPacket, 0, headerPacket.Length);
        }
    }
}
