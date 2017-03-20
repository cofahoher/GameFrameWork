using System;
using System.Collections;
using System.Collections.Generic;

namespace BaseUtil
{
    public class NetInStream
    {
        public NetInStream(NetBuffer buffer)
        {
            m_buffer = buffer;
        }

        public void Read(ref bool val)
        {
            Skip(1);
            val = BitConverter.ToBoolean(m_buffer.buffer, m_offset - 1);
        }

        public void Read(ref float val)
        {
            Skip(4);
            val = BitConverter.ToSingle(m_buffer.buffer, m_offset - 4);
        }

        public void Read(ref short val)
        {
            Skip(2);
            val = BitConverter.ToInt16(m_buffer.buffer, m_offset - 2);
        }

        public void Read(ref ushort val)
        {
            Skip(2);
            val = (ushort)BitConverter.ToInt16(m_buffer.buffer, m_offset - 2);
        }

        public void Read(ref int val)
        {
            Skip(4);
            val = BitConverter.ToInt32(m_buffer.buffer, m_offset - 4);
        }

        public void Read(ref uint val)
        {
            Skip(4);
            val = (uint)BitConverter.ToInt32(m_buffer.buffer, m_offset - 4);
        }

        public void Read(ref long val)
        {
            Skip(8);
            val = BitConverter.ToInt64(m_buffer.buffer, m_offset - 8);
        }

        public void Read(ref ulong val)
        {
            Skip(8);
            val = (ulong)BitConverter.ToInt64(m_buffer.buffer, m_offset - 8);
        }

        public void Read(ref byte val)
        {
            Skip(1);
            val = m_buffer.buffer[m_offset - 1];
        }

        public void Read(ref sbyte val)
        {
            Skip(1);
            val = (sbyte)m_buffer.buffer[m_offset - 1];
        }

        public void Read(ref double val)
        {
            Skip(8);
            val = BitConverter.ToDouble(m_buffer.buffer, m_offset - 8);
        }

        public void Read(ref string val)
        {
            int length = 0;
            this.Read(ref length);
            Skip(length);

            this.Read(ref val, length);
        }

        public void Read(ref string val, int length)
        {
            Skip(length);
            string str = System.Text.Encoding.UTF8.GetString(m_buffer.buffer, m_offset - length, length);
            byte eos = 0;
            this.Read(ref eos);

            //过滤掉复杂的unicode字符
            val = "";
            for (int i = 0; i < str.Length; ++i)
            {
                if (!Char.IsLowSurrogate(str[i]) && !Char.IsHighSurrogate(str[i]))
                {
                    val += str[i];
                }
            }
        }

        public void Read(ref byte[] buffer)
        {
            int length = 0;
            this.Read(ref length);
            this.Read(ref buffer, length);
        }

        public void Read(ref byte[] buffer, int length)
        {
            buffer = new byte[length];
            System.Array.Copy(m_buffer.buffer, m_offset, buffer, 0, length);
            m_offset += length;
        }

        public void Read(ref NetBuffer buffer)
        {
            int length = 0;
            this.Read(ref length);
            this.Read(ref buffer, length);
        }

        public void Read(ref NetBuffer buffer, int length)
        {
            buffer.ExpandTo(length);
            System.Array.Copy(m_buffer.buffer, m_offset, buffer.buffer, 0, length);
            buffer.SetLength(length);
            m_offset += length;
        }


        public void Skip(int size)
        {
            if (m_offset + size > m_buffer.length)
            {
                throw new NetStreamException("stream out of range", size);
            }

            m_offset += size;
        }
        public int BytesLeft()
        {
            return m_buffer.length - m_offset;
        }

        public void Rewind()
        {
            m_offset = 0;
        }
        public NetBuffer GetBuffer() { return m_buffer; }
        public int Offset { get { return m_offset; } }
        private NetBuffer m_buffer;
        private int m_offset = 0;
    }
}
