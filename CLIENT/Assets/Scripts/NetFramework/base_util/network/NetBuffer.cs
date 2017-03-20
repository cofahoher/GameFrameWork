using System;

namespace BaseUtil
{
    public class NetBuffer
    {
        public NetBuffer()
        {
            m_buffer = new byte[256];
            m_length = 0;
        }

        public NetBuffer(int iniSize)
        {
            m_buffer = new byte[iniSize];
            m_length = 0;
        }
        public NetBuffer(byte[] buffer)
        {
            m_buffer = buffer;
            m_length = m_buffer.Length;
        }

        public NetBuffer(NetBuffer buff)
        {
            m_buffer = new byte[buff.length];
            System.Array.Copy(m_buffer, buff.m_buffer, buff.length);
            m_length = buff.length;
        }

        public void ReduceSize(int size)
        {
            if (m_length > size)
                m_length = size;
        }

        public void ReAlloc(int size)
        {
            if (size <= this.capcity)
                return;

            int next = this.capcity;
            do
            {
                next *= 2;
            }
            while (next < size);

            byte[] new_buffer = new byte[next];
            m_buffer.CopyTo(new_buffer, 0);
            m_buffer = new_buffer;
        }

        public void ExpandTo(int size)
        {
            ReAlloc(size);

            if (m_length < size)
                m_length = size;
        }
        public void SetLength(int size)
        {
            m_length = size;
        }
        public byte[] buffer
        {
            get
            {
                return m_buffer;
            }
            set
            {
                m_buffer = value;
            }
        }

        public int capcity
        {
            get
            {
                return m_buffer.Length;
            }
        }

        public int length
        {
            get
            {
                return m_length;
            }
            //set
            //{
            //    m_length = value;
            //}
        }
        //public 

        private byte[] m_buffer;
        private int m_length;
    }


    internal class ArrayCache
    {

    }
}
