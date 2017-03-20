using System;
using System.Net;
using System.Net.Sockets;
//using UnityEngine;

namespace BaseUtil
{
    public interface INetConnector : IDisposable
    {
        void Connect(string address, ushort port);
        void OnClose(INetConnection conn);
    }

    internal class NetConnector : INetConnector
    {
        public static int PingCycle = 30 * 1000;
        const int MaxNoPackTick = 120 * 1000;

#if UNITY_EDITOR
        const int ConnectTimeout = 1 * 1000;
#else
        const int ConnectTimeout = 8 * 1000;
#endif

        private System.Timers.Timer timerKeepAlive;
        private System.Timers.Timer connectTimeoutTimer;

        private bool m_disposed = false;
        private NetConnection m_conn;

        IEventDispatcher m_notifier;

        bool m_force_ipv6 = false;

        public bool ForceV6
        {
            get { return m_force_ipv6; }
            set { m_force_ipv6 = value; }
        }

        public NetConnector(IEventDispatcher notifier)
        {
            m_notifier = notifier;
            timerKeepAlive = new System.Timers.Timer(PingCycle);
            timerKeepAlive.Elapsed += this.CheckKeepAlive;

            connectTimeoutTimer = new System.Timers.Timer(ConnectTimeout);
            connectTimeoutTimer.Elapsed += this.CheckConnectTimeout;
        }

        ~NetConnector()
        {
            ClearUp();
        }

        public void Dispose()
        {
            ClearUp();
            GC.SuppressFinalize(this);
        }

        protected void ClearUp()
        {
            if (m_disposed) return;
            m_disposed = true;

            timerKeepAlive.Dispose();
            timerKeepAlive = null;

            connectTimeoutTimer.Dispose();
            connectTimeoutTimer = null;
        }

		void SetForceIpv6(SocketError socket_err, Socket curr_socket)
		{
            if (curr_socket != null && curr_socket.AddressFamily == AddressFamily.InterNetwork &&
                (socket_err == SocketError.AddressFamilyNotSupported || socket_err == SocketError.AccessDenied || socket_err == SocketError.NetworkUnreachable))
			{
				m_force_ipv6 = true;
			}
		}

        public void Connect(string address, ushort port)
        {
            try
            {
                LogWrapper.LogInfo("Connecting to " + address + ":" + port);

                bool force_ipv6 = m_force_ipv6;
                m_force_ipv6 = false;

                NetConnection conn = new NetConnection(m_notifier);
                //必须先赋值，BeginConnect可能在返回之前就调用HandleConnect
                m_conn = conn;
                connectTimeoutTimer.Enabled = true;
                conn.BeginConnect(address, port, new AsyncCallback(this.HandleConnct), force_ipv6);
            }
            catch (Exception e)
            {
                LogWrapper.Exception(e);

                if (e is SocketException)
                {
                    SocketError socket_err = (e as SocketException).SocketErrorCode;
                    LogWrapper.LogInfo("Socket err: " + socket_err);
					SetForceIpv6(socket_err, m_conn.socket);
                }

                ConnectionFailClose();
            }
        }
        public void OnClose(INetConnection conn)
        {
            if((conn != null) && (conn == m_conn))
            {
                StopTimer();
                m_conn = null;
            }
        }
        private void StopTimer()
        {
            timerKeepAlive.Stop();
            connectTimeoutTimer.Stop();
        }
        private void CloseConn()
        {
            if (null == m_conn) return;
            NetConnection conn = m_conn;
            m_conn = null;

            StopTimer();
            conn.CloseConnection();
        }

        private void ConnectionFailClose()
        {
            lock (this)
            {
                if (m_conn == null)
                {
                    LogWrapper.LogInfo("NetConnector ConnectionFailClose conn null");
                    return;
                }
                LogWrapper.LogInfo("NetConnector ConnectionFailClose");

                try
                {
                    StopTimer();
                    m_conn.socket.Close();

                }
                catch(Exception e)
                {
                    LogWrapper.LogError("ConnectionFailClose. ", e.Message);
                }

                NetEvent ev = new NetEvent(NetEvent.Type.ConnectFail, null);
                m_notifier.PostEvent(ev);
                m_conn = null;
            }
        }

        private void CheckKeepAlive(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_conn == null)
            {
                LogWrapper.LogError("CheckKeepAlive null ptr");
                return;
            }
            //decide if this is dead
            int current = Environment.TickCount;
            int last = m_conn.last_pack_tick;

            if (last != 0)
            {
                int s = current - last;
                if (s > MaxNoPackTick)
                {
                    CloseConn();
                    return;
                }
            }

            //send the ping command
            NetEvent ev = new NetEvent(NetEvent.Type.KeepAlive, m_conn);
            m_notifier.PostEvent(ev);
        }

        private void CheckConnectTimeout(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_conn == null)
            {
                return;
            }

            int current = Environment.TickCount;
            int last = m_conn.last_conn_tick;

            if (last != 0)
            {
                int s = current - last;
                if (s > 5000)
                {
                    LogWrapper.LogDebug("Connect timout");
                    ConnectionFailClose();
                    return;
                }
            }
        }

        private void HandleConnct(IAsyncResult async_res)
        {
            NetConnection conn = async_res.AsyncState as NetConnection;
            if (conn != m_conn)
            {
                LogWrapper.LogError("HandleConnct while conn null.");
                return;
            }

            try
            {
                conn.socket.EndConnect(async_res);
                LogWrapper.LogInfo("raw connect established");

                timerKeepAlive.Enabled = true;
                connectTimeoutTimer.Stop();

                NotifyEvent(NetEvent.Type.ConnectionEstablish);

                conn.PostRecv();
            }
            catch (Exception e)
            {
                LogWrapper.LogInfo("raw connect fail: " + e.ToString());
                if (e is SocketException)
                {
                    SocketError socket_err = (e as SocketException).SocketErrorCode;
                    LogWrapper.LogInfo("Socket err: " + socket_err);
					SetForceIpv6(socket_err, conn.socket);
                }

                ConnectionFailClose();
            }
        }
        private void NotifyEvent(NetEvent.Type ev)
        {
            m_notifier.PostEvent(new NetEvent(ev, m_conn));
        }
    }
}
