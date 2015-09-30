using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Media
{
    public class BitstreamWriter
    {
        private byte m_bitsToWrite = 0;
        private int m_nextBitShift = 0;


        public BitstreamWriter(Stream stream)
        {
            this.Stream = stream;
        }

        protected Stream Stream
        {
            get;
            private set;
        }

        public void PutAlignedBytes(byte[] data)
        {
            PutAlignedBytes(data, 0, data.Length);
        }

        public void PutAlignedBytes(byte[] data, int offset, int length)
        {
            if (m_nextBitShift != 0) throw new InvalidOperationException("data buffer isn't aligned to 8 bit");
            Stream.Write(data, offset, length);
        }

        public void PutAlignedByte(byte data)
        {
            if (m_nextBitShift != 0) throw new InvalidOperationException("data buffer isn't aligned to 8 bit");
            Stream.WriteByte(data);
        }

        public void Put64BitsAligned(UInt64 data)
        {
            if (m_nextBitShift != 0) throw new InvalidOperationException("data buffer isn't aligned to 8 bit");
            Stream.Write(BitConverter.GetBytes(data).Reverse().ToArray(), 0, 8);
        }

        public void Put32BitsAligned(UInt32 data)
        {
            if (m_nextBitShift != 0) throw new InvalidOperationException("data buffer isn't aligned to 8 bit");
            Stream.Write(BitConverter.GetBytes(data).Reverse().ToArray(), 0, 4);
        }

        public void PutNullTerminatedStringAligned(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    PutAlignedByte((byte)str[i]);
                }
            }
            PutAlignedByte(0);
        }

        public void PutBits(uint data, int bitsCount)
        {
            if (bitsCount > 32) throw new ArgumentOutOfRangeException("too many bits at one operation [" + bitsCount + "], 32 is a maximum allowed value");
            uint sourceMask = (0x00000001U << (bitsCount - 1));
            for (int i = 0; i < bitsCount; i++)
            {
                byte bit = ((data & sourceMask) != 0 ? (byte)0x01 : (byte)0x00);
                sourceMask >>= 1;
                bit <<= (7 - m_nextBitShift);
                m_bitsToWrite |= bit;
                m_nextBitShift++;
                if (m_nextBitShift >= 8)
                {
                    Stream.WriteByte(m_bitsToWrite);
                    m_bitsToWrite = 0;
                    m_nextBitShift = 0;
                }
            }
        }

        public void PutOneBitFlag(bool flag)
        {
            PutBits((flag ? 0x01u : 0x00u), 1);
        }

        public long GetPosition()
        {
            if (m_nextBitShift != 0) throw new InvalidOperationException("data buffer isn't aligned to 8 bit");
            return Stream.Position;
        }

        public void SetPosition(long pos)
        {
            Stream.Position = pos;
        }

        public void SkipBytes(int count)
        {
            if (m_nextBitShift != 0) throw new InvalidOperationException("data buffer isn't aligned to 8 bit");
            Stream.Position = Stream.Position + count;
        }

        public void Close()
        {
            if (Stream != null)
            {
                Stream.Close();
            }
        }
    }
}
