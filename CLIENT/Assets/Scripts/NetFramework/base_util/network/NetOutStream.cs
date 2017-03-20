using System;
using System.Collections.Generic;

namespace BaseUtil
{
    public class BitConvertNoGC
    {
        public static int GetBytes(float value, byte[] output, int offset = 0)
        {
            unsafe
            {
                fixed (byte* ptr = &output[offset])
                {
                    *(float*)ptr = value;
                    offset += sizeof(float);
                    return offset;
                }
            }
        }
        public static int GetBytes(ulong value, byte[] output, int offset = 0)
        {
            unsafe
            {
                fixed (byte* ptr = &output[offset])
                {
                    *(ulong*)ptr = value;
                    offset += sizeof(ulong);
                    return offset;
                }
            }
        }
        public static int GetBytes(uint value, byte[] output, int offset = 0)
        {
            unsafe
            {
                fixed (byte* ptr = &output[offset])
                {
                    *(uint*)ptr = value;
                    offset += sizeof(uint);
                    return offset;
                }
            }
        }
        public static int GetBytes(ushort value, byte[] output, int offset = 0)
        {
            unsafe
            {
                fixed (byte* ptr = &output[offset])
                {
                    *(ushort*)ptr = value;
                    offset += sizeof(ushort);
                    return offset;
                }
            }
        }
        public static int GetBytes(long value, byte[] output, int offset = 0)
        {
            unsafe
            {
                fixed (byte* ptr = &output[offset])
                {
                    *(long*)ptr = value;
                    offset += sizeof(long);
                    return offset;
                }
            }
        }
        public static int GetBytes(double value, byte[] output, int offset = 0)
        {
            unsafe
            {
                fixed (byte* ptr = &output[offset])
                {
                    *(double*)ptr = value;
                    offset += sizeof(double);
                    return offset;
                }
            }
        }
        public static int GetBytes(short value, byte[] output, int offset = 0)
        {
            unsafe
            {
                fixed (byte* ptr = &output[offset])
                {
                    *(short*)ptr = value;
                    offset += sizeof(short);
                    return offset;
                }
            }
        }
        public static int GetBytes(char value, byte[] output, int offset = 0)
        {
            unsafe
            {
                fixed (byte* ptr = &output[offset])
                {
                    *(char*)ptr = value;
                    offset += sizeof(char);
                    return offset;
                }
            }
        }
        public static int GetBytes(bool value, byte[] output, int offset = 0)
        {
            unsafe
            {
                fixed (byte* ptr = &output[offset])
                {
                    *(bool*)ptr = value;
                    offset += sizeof(bool);
                    return offset;
                }
            }
        }
        public static int GetBytes(int value, byte[] output, int offset = 0)
        {
            unsafe
            {
                fixed (byte* ptr = &output[offset])
                {
                    *(int*)ptr = value;
                    offset += sizeof(int);
                    return offset;
                }
            }
        }
    }

    public class NetOutStream
    {
        public NetOutStream(NetBuffer buffer)
        {
            m_buffer = buffer;
        }
        public NetBuffer GetBuffer() { return m_buffer; }
        public int Offset { get { return m_offset; } }
        public void Seek(int offset) { m_offset = offset; }
        public void Write(bool val)
        {
            this.m_buffer.ExpandTo(m_offset + 1);
            BitConvertNoGC.GetBytes(val, m_buffer.buffer, m_offset);
            m_offset += 1;
        }

        public void Write(short val)
        {
            this.m_buffer.ExpandTo(m_offset + 2);
            BitConvertNoGC.GetBytes(val, m_buffer.buffer, m_offset);
            m_offset += 2;
        }

        public void Write(ushort val)
        {
            this.m_buffer.ExpandTo(m_offset + 2);
            BitConvertNoGC.GetBytes(val, m_buffer.buffer, m_offset);
            m_offset += 2;
        }

        public void Write(int val)
        {
            this.m_buffer.ExpandTo(m_offset + 4);
            BitConvertNoGC.GetBytes(val, m_buffer.buffer, m_offset);
            m_offset += 4;
        }

        public void Write(uint val)
        {
            this.m_buffer.ExpandTo(m_offset + 4);
            BitConvertNoGC.GetBytes(val, m_buffer.buffer, m_offset);
            m_offset += 4;
        }

        public void Write(long val)
        {
            this.m_buffer.ExpandTo(m_offset + 8);
            BitConvertNoGC.GetBytes(val, m_buffer.buffer, m_offset);
            m_offset += 8;
        }

        public void Write(ulong val)
        {
            this.m_buffer.ExpandTo(m_offset + 8);
            BitConvertNoGC.GetBytes(val, m_buffer.buffer, m_offset);
            m_offset += 8;
        }

        public void Write(byte val)
        {
            this.m_buffer.ExpandTo(m_offset + 1);
            m_buffer.buffer[m_offset] = val;
            m_offset += 1;
        }

        public void Write(sbyte val)
        {
            Write((byte)val);
        }

        public void Write(float val)
        {
            this.m_buffer.ExpandTo(m_offset + 4);
            //BitConvertNoGC.GetBytes(val, m_buffer.buffer, m_offset);
            byte[] array = BitConverter.GetBytes(val);
            array.CopyTo(m_buffer.buffer, m_offset);
            m_offset += 4;
        }

        public void Write(double val)
        {
            this.m_buffer.ExpandTo(m_offset + 8);
            //BitConvertNoGC.GetBytes(val, m_buffer.buffer, m_offset);
            byte[] array = BitConverter.GetBytes(val);
            array.CopyTo(m_buffer.buffer, m_offset);
            m_offset += 8;
        }

        public void Write(string val, bool write_lenth = true)
        {
            //take care the encoding
            byte[] strval = System.Text.Encoding.UTF8.GetBytes(val);

            if (write_lenth)
            {
                this.Write(strval.Length);
            }

            this.m_buffer.ExpandTo(m_offset + strval.Length);

            strval.CopyTo(m_buffer.buffer, m_offset);
            m_offset += strval.Length;

            byte eos = 0;
            this.Write(eos);
        }

        public void Write(byte[] bytearray, bool write_lenth = true)
        {
            int length = bytearray.Length;

            if (write_lenth)
            {
                this.Write(length);
            }

            this.m_buffer.ExpandTo(m_offset + length);
            System.Array.Copy(bytearray, 0, m_buffer.buffer, m_offset, length);
            m_offset += length;
        }

        public void Write(NetBuffer buffer, bool write_lenth = true)
        {
            int length = buffer.length;

            if (write_lenth)
            {
                this.Write(length);
            }

            this.m_buffer.ExpandTo(m_offset + length);
            System.Array.Copy(buffer.buffer, 0, m_buffer.buffer, m_offset, length);
            m_offset += length;
        }

        private NetBuffer m_buffer;
        private int m_offset = 0;
    }
}
