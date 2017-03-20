using System;


namespace BaseUtil
{
	internal enum InnerCommand
    {
		NET_PING = 1,
		NET_PONG = 2,

		NET_SYSTEM_END = 100,
	};

    internal class inner_command
    {
        public InnerCommand command = 0;
        public int param32_0 = 0;
        public int param32_1 = 0;
        public int param32_2 = 0;
        public long param64 = 0;

        public void ToStream(NetOutStream outs)
        {
            int cmd = (int)command;
            outs.Write(cmd);
            outs.Write(param32_0);
            outs.Write(param32_1);
            outs.Write(param32_2);
            outs.Write(param64);
        }
        public void FromStream(NetInStream ins)
        {
            int cmd=0;
            ins.Read(ref cmd);
            command = (InnerCommand)cmd;

            ins.Read(ref param32_0);
            ins.Read(ref param32_1);
            ins.Read(ref param32_2);
            ins.Read(ref param64);
        }
    };

    internal class package_header
    {
        public bool is_inner = false;
        public int body_length = 0;
        //public ushort msg_id = 0;
        //public uint time_stamp = 0;
        //public uint checksum = 0;
        public package_header(bool _is_inner)
        {
            is_inner = _is_inner;
        }
        public void Load(NetInStream s)
        {
            s.Read(ref body_length);
            if (body_length < 0)
            {
                is_inner = true;
                body_length = -body_length;
            }
            //s.Read(ref msg_id);
            //s.Read(ref time_stamp);
            //s.Read(ref checksum);
        }

        public void Save(NetOutStream s)
        {
            int len = body_length;
            if (is_inner)
            {
                len = -len;
            }
            s.Write(len);
            //s.Write(msg_id);
            //s.Write(time_stamp);
            //s.Write(checksum);
        }
    }

    public class NetPacket
    {
        private package_header m_header;
        private NetBuffer m_header_buffer;
        private NetBuffer m_body_buffer;
        public int m_head_transfered;
        public int m_body_transfered;
        
        public ushort m_clsid; //Debug用，后续删掉

        public NetPacket(bool is_inner)
        {
            m_header = new package_header(is_inner);
            m_header_buffer = new NetBuffer(headerLength);
            m_body_buffer = new NetBuffer(bufferLength);
            m_head_transfered = 0;
            m_body_transfered = 0;
        }

        public const int headerLength = 4;
        public const int bufferLength = 512;

        public bool IsInner 
        { 
            get { return m_header.is_inner; } 
            set { m_header.is_inner = value; } 
        }

        public NetBuffer headBuffer
        {
            get
            {
                return m_header_buffer;
            }
        }

        public NetBuffer bodyBuffer
        {
            get
            {
                return m_body_buffer;
            }
        }

        public int payloadLength
        {
            get
            {
                return m_header.body_length;
            }
        }

        public void clear()
        {
            m_header.body_length = 0;
            m_header_buffer.SetLength(0);
            m_body_buffer.SetLength(0);
            m_head_transfered = 0;
            m_body_transfered = 0;
        }

        public bool decode()
        {
            NetInStream s = new NetInStream(m_header_buffer);
            try
            {
                m_header.Load(s);
                return true;
            }
            catch (NetStreamException e)
            {
                LogWrapper.Exception(e);
                return false;
            }
        }

        public bool encode()
        {
            NetOutStream s = new NetOutStream(m_header_buffer);
            try
            {
                m_header.body_length = (ushort)m_body_buffer.length;
                m_header.Save(s);
                return true;
            }
            catch (NetStreamException e)
            {
                LogWrapper.Exception(e);
                return false;
            }          
        }
    }
}
