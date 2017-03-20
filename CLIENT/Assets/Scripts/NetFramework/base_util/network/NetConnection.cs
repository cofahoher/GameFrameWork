using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BaseUtil
{
    public interface INetConnection : IDisposable
    {
        void Quit();
        bool IsConnected { get; }
        void CloseConnection();
        void SendMessage(NetMessage msg);
        int Ping { get; }
    }

    internal class NetConnection : INetConnection
    {
        private LinkedList<NetPacket> m_packets_buff;
        private LinkedList<NetPacket> m_packets_send;
        private readonly Object m_error_lock;
        private Socket m_socket;
        private IEventDispatcher m_notifier;
        private int m_send_offset = 0;
        private bool m_disposed = false;
        private bool m_seterror = false;
        public int m_ping = -1;
        public int Ping
        {
            get { return m_ping; }
        }
        public int last_pack_tick = 0;
        public int last_conn_tick = 0;

        internal Socket socket { get { return m_socket; } }
        public bool IsConnected { get { if (m_socket != null) return m_socket.Connected; else return false; } }

        public NetConnection(IEventDispatcher notifier)
        {
            m_error_lock = new Object();
            m_notifier = notifier;
            m_packets_send = new LinkedList<NetPacket>();
            m_packets_buff = new LinkedList<NetPacket>();
        }

        ~NetConnection()
        {
        }

        NetPacket AllocPacket(bool is_inner)
        {
            lock (m_packets_buff)
            {
                if (m_packets_buff.Count > 0)
                {
                    NetPacket p = m_packets_buff.First.Value;
                    p.IsInner = is_inner;
                    p.clear();
                    m_packets_buff.RemoveFirst();
                    return p;
                }
                return new NetPacket(is_inner);
            }
        }

        void DeallocPacket(NetPacket p)
        {
            lock (m_packets_buff)
            {
                m_packets_buff.AddLast(p);
            }
        }

        bool IsIpv6Address(string host)
        {
            IPAddress address;
            if (IPAddress.TryParse(host, out address))
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    return true;
                }
            }
            return false;
        }

        void RefreshTick()
        {
            last_pack_tick = Environment.TickCount;
        }
        void RefreshConnTick()
        {
            last_conn_tick = Environment.TickCount;
        }
        public void Dispose()
        {
            LogWrapper.LogWarning("Dispose NetConnection");

            ClearUp();
            GC.SuppressFinalize(this);
        }

        protected virtual void ClearUp()
        {
            //可重入
            if (m_disposed) return;
            m_disposed = true;

            if (null != m_socket)
            {
                m_socket.Dispose();
                m_socket = null;
            }
            m_packets_send = null;
        }

        public IAsyncResult BeginConnect(string address, ushort port, AsyncCallback requestCallback, bool force_ipv6 = false)
        {
            RefreshTick();
            RefreshConnTick();

            if (force_ipv6 || IsIpv6Address(address))
            {
				LogWrapper.LogInfo("use ipv6 socket.");
                m_socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            }
            else
            {
                LogWrapper.LogInfo("use ipv4 socket.");
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            
            m_socket.NoDelay = true;
            return m_socket.BeginConnect(address, port, requestCallback, this);
        }

        public void Quit()
        {
            try
            {
                m_socket.Shutdown(SocketShutdown.Both);
                m_socket.Close();
            }
            catch (Exception)
            {
            }
        }
        public void CloseConnection()
        {
            m_socket.Close();
			SetError(SocketError.Success);
        }

        public void SendInnerCommand(inner_command cmd)
        {
            NetPacket pack = AllocPacket(true);
            NetOutStream outs = new NetOutStream(pack.bodyBuffer);

            cmd.ToStream(outs);

            SendPacket(pack);
        }
        public void SendMessage(NetMessage msg)
        {
            NetPacket pack = AllocPacket(false);
            NetOutStream outs = new NetOutStream(pack.bodyBuffer);
            ushort clsid = (ushort)msg.CLSID;
            pack.m_clsid = clsid;
            outs.Write(clsid);

            msg.ToStream(outs);

            SendPacket(pack);
        }

        private void SendPacket(NetPacket pack)
        {
#if !CONSOLE_CLIENT && GUICONSOLE_ENABLE
            if (LevelGlobal.getInstance() != null && LevelGlobal.GetMode() != null && LevelGlobal.GetMode() is PVPMode)
            {
                PVPMode pvp = LevelGlobal.GetMode() as PVPMode;
                if (null != pvp) pvp.net_statistic.Send(pack.bodyBuffer.length + 4);
            }
#endif
            if (!pack.encode())
            {
                SetError(SocketError.ProtocolNotSupported);
                return;
            }

            try
            {
                lock (m_packets_send)
                {
                    m_packets_send.AddLast(pack);
                    if (m_packets_send.Count == 1)//为啥要判断是1？
                        PostSend();
                }
            }
            catch (SocketException e)
            {
                SetError(e);
            }
            catch (ObjectDisposedException)
            {
                LogWrapper.LogWarning("ObjectDisposedException caught. @SendPacket");
            }
        }

        private void PostSend()
        {
            if (m_packets_send.Count != 0)
            {
                NetPacket p = m_packets_send.First.Value;
                m_send_offset = 0;
                try
                {
                    m_socket.BeginSend(p.headBuffer.buffer, 0, NetPacket.headerLength, SocketFlags.None, new AsyncCallback(this.HandleHeaderSend), p);
                }
                catch (Exception)
                {
                	
                }
            }
        }

        internal void PostRecv()
        {
            NetPacket p = AllocPacket(false);

            begin_recv(p.headBuffer.buffer, 0, p.headBuffer.capcity, SocketFlags.None, new AsyncCallback(this.HandleHeaderRecv), p);
        }

        void begin_recv(byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            m_socket.BeginReceive(buffer, offset, size, socketFlags, callback, state);
        }
        int end_recv(IAsyncResult ar)
		{
			int transfered = m_socket.EndReceive(ar);
            return transfered;
        }

        private void HandleHeaderRecv(IAsyncResult ar)
        {
            try
            {
                int transfered = end_recv(ar);
                NetPacket p = ar.AsyncState as NetPacket;
                p.m_head_transfered += transfered;
                p.headBuffer.ExpandTo(p.m_head_transfered);
                if (p.m_head_transfered < NetPacket.headerLength)
                {
                    begin_recv(
                        p.headBuffer.buffer,
                        p.m_head_transfered,
                        NetPacket.headerLength - p.m_head_transfered,
                        SocketFlags.None,
                        new AsyncCallback(this.HandleHeaderRecv), p);
                }
                else
                {
                    if (!p.decode())
                    {
                        SetError(SocketError.ProtocolNotSupported);
                    }
                    else
                    {
                        p.bodyBuffer.ExpandTo(p.payloadLength);
                        begin_recv(p.bodyBuffer.buffer, 0, p.payloadLength, SocketFlags.None, new AsyncCallback(this.HandleBodyRecv), p);
                    }
                }
                RefreshTick();
            }
            catch (SocketException e)
            {
                SetError(e);
            }
            catch (ObjectDisposedException e)
            {
                LogWrapper.LogError("ObjectDisposedException caught. @HandleHeaderRecv");
				LogWrapper.LogError(e.StackTrace);
            }
        }

        private void HandleBodyRecv(IAsyncResult ar)
        {
            try
            {
                int transferd = end_recv(ar);
                NetPacket p = ar.AsyncState as NetPacket;
                p.m_body_transfered += transferd;
                p.bodyBuffer.ExpandTo(p.m_body_transfered);
                if (p.m_body_transfered < p.payloadLength)
                {
                    begin_recv(
                        p.bodyBuffer.buffer,
                        p.m_body_transfered,
                        p.payloadLength - p.m_body_transfered,
                        SocketFlags.None,
                        new AsyncCallback(this.HandleBodyRecv), p);
                }
                else
                {
                    p.bodyBuffer.ReduceSize(p.payloadLength);
                    FinishRecvPacket(p);
                }
                RefreshTick();
            }
            catch (SocketException e)
            {
                SetError(e);
            }
            catch (ObjectDisposedException e)
            {
				LogWrapper.LogError("ObjectDisposedException caught. @HandleBodyRecv");
				LogWrapper.LogError(e.StackTrace);
            }
        }

        private void FinishRecvPacket(NetPacket p)
        {
            NetEvent ev = CreateEvent(p);
            if (ev != null)
            {
                m_notifier.PostEvent(ev);
            }
            DeallocPacket(p);

#if !CONSOLE_CLIENT && GUICONSOLE_ENABLE
            if (LevelGlobal.getInstance() != null && LevelGlobal.GetMode() != null && LevelGlobal.GetMode() is PVPMode)
            {
                (LevelGlobal.GetMode() as PVPMode).net_statistic.Recv(p.bodyBuffer.length + 4);
            }
#endif
            PostRecv();
        }

        NetEvent CreateEvent(NetPacket p)
        {
            NetEvent ev = null;
            NetInStream ins = new NetInStream(p.bodyBuffer);

            if (p.IsInner)
            {
                inner_command cmd = new inner_command();
                try
                {
                    cmd.FromStream(ins);
                    ev = new NetEvent(NetEvent.Type.InnerCommand, this);
                    ev.cmd = cmd;

                }
                catch (Exception e)
                {
                    LogWrapper.LogWarning("CreateEvent exception:" + e.ToString());
                    return null;
                }
            }
            else
            {
                ushort clsid = 0;
                try
                {
                    ins.Read(ref clsid);
                }
                catch (Exception e)
                {
                    LogWrapper.LogWarning("CreateEvent exception:" + e.ToString());
                    return null;
                }
                try
                {
                    NetMessage msg = NetMessageFactory.CreateMessage(clsid);
                    if (msg != null)
                    {
                        msg.FromStream(ins);
                        ev = new NetEvent(NetEvent.Type.IncomingMessage, this);
                        ev.message = msg;
                    }
                    else
                    {
                        LogWrapper.LogError("CreateEvent invalid clsid:" + clsid);
                    }
                }
                catch (Exception e)
                {
                    LogWrapper.LogError("CreateEvent exception of clsid:" + clsid + "," + e.ToString());
                    return null;
                }
            }
            return ev;
        }


        private void HandleHeaderSend(IAsyncResult ar)
        {
            try
            {
                int transfered = m_socket.EndSend(ar);
                NetPacket p = ar.AsyncState as NetPacket;

                m_send_offset += transfered;
                if (m_send_offset < NetPacket.headerLength)
                {
                    m_socket.BeginSend(
                        p.headBuffer.buffer,
                        m_send_offset,
                        NetPacket.headerLength - m_send_offset,
                        SocketFlags.None,
                        new AsyncCallback(this.HandleHeaderSend),
                        p);
                }
                else
                {
                    m_send_offset = 0;
                    m_socket.BeginSend(p.bodyBuffer.buffer, 0, p.payloadLength, SocketFlags.None, new AsyncCallback(this.HandleBodySend), p);
                }
            }
            catch (SocketException e)
            {
                SetError(e);
            }
            catch (ObjectDisposedException)
            {
                LogWrapper.LogWarning("ObjectDisposedException caught. @IAsyncResult");
            }
        }

        private void HandleBodySend(IAsyncResult ar)
        {
            try
            {
                int transfered = m_socket.EndSend(ar);
                NetPacket p = ar.AsyncState as NetPacket;
                m_send_offset += transfered;
                if (m_send_offset < p.payloadLength)
                {
                    m_socket.BeginSend(
                        p.bodyBuffer.buffer,
                        m_send_offset,
                        p.payloadLength - m_send_offset,
                        SocketFlags.None,
                        new AsyncCallback(this.HandleBodySend),
                        p);
                }
                else
                {
                    FinishSendPacket(p);
                }
            }
            catch (SocketException e)
            {
                SetError(e);
            }
            catch (ObjectDisposedException)
            {
                LogWrapper.LogWarning("ObjectDisposedException caught. @HandleBodySend");
            }
        }

        private void FinishSendPacket(NetPacket p)
        {
            lock (m_packets_send)
            {
                if (m_packets_send.Count != 0)
                {
                    m_packets_send.RemoveFirst();
                    PostSend();
                }
            }

            DeallocPacket(p);
        }

        internal void SetError(SocketException e)
        {
            LogWrapper.LogInfo("SetError: " + e);
            LogWrapper.LogInfo("Socket err: " + e.SocketErrorCode);
            SetError(e.SocketErrorCode);
        }
        internal void SetError(SocketError err)
        {
            if (Monitor.TryEnter(m_error_lock))
            {
                try
                {
                    if (!m_seterror)
                    {
                        m_seterror = true;

                        this.CloseConnection();

                        if ( !m_disposed )
                        {
                            lock (m_packets_send)
                            {
                                m_packets_send.Clear();
                            }

                            NetEvent ev = new NetEvent(NetEvent.Type.ConnectionClose, this);
                            m_notifier.PostEvent( ev );
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(m_error_lock);
                }
            }
        }
    }
}
