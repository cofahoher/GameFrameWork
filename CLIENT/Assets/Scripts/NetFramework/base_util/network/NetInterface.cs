using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

#if CONSOLE_CLIENT
using System.Threading.Tasks;
using System.Collections.Concurrent;
#endif

namespace BaseUtil
{
    public delegate void PingResultHandler(int ping);
    public abstract class NetMessage
    {
        public abstract int CLSID { get; }
        public abstract void FromStream(NetInStream ins);
        public abstract void ToStream(NetOutStream outs);

        public abstract bool ENCRYPT { get; }

        public int RecvTick { set; get; }

        public int QueueTick 
        {
            get { return Environment.TickCount - RecvTick; }
        }
    }

    internal class NetEvent
    {
        internal enum Type
        {
            ConnectFail,
            ConnectionEstablish,
            ConnectionClose,
            KeepAlive,
            IncomingMessage,
            InnerCommand,
        }
        internal Type type;
        internal NetConnection conn;
        internal NetMessage message = null;
        internal inner_command cmd;

        internal NetEvent(Type t, NetConnection c)
        {
            type = t;
            conn = c;
        }
    }

    class PacketHandler
    {
        NetInterface m_netintf = null;

        public PacketHandler(NetInterface netintf)
        {
            m_netintf = netintf;
        }

        public void SendPing(NetConnection conn)
        {
            int current = Environment.TickCount;
            inner_command ping = new inner_command();
            ping.command = InnerCommand.NET_PING;
            ping.param32_0 = current;
            ping.param32_1 = conn.m_ping;

            conn.SendInnerCommand(ping);
        }
        void HandlePing(NetConnection conn, inner_command ping)
        {
            inner_command pong = new inner_command();
            pong.command = InnerCommand.NET_PONG;
            pong.param32_0 = ping.param32_0;
            pong.param32_1 = ping.param32_1;
            pong.param32_2 = ping.param32_2;
            pong.param64 = ping.param64;

            conn.SendInnerCommand(pong);

            conn.m_ping = ping.param32_1;
        }        
        internal event PingResultHandler PingResultCallback = null;
        void HandlePong(NetConnection conn, inner_command pong)
        {
            int current = Environment.TickCount;
            int ping = current - pong.param32_0;
            conn.m_ping = ping;           

            if (PingResultCallback != null)
            {
                PingResultCallback(ping);
            }
        }

        public void HandleInnerMessage(NetConnection conn, inner_command cmd)
        {
            if (cmd.command == InnerCommand.NET_PING)
            {
                HandlePing(conn, cmd);
            }
            else if (cmd.command == InnerCommand.NET_PONG)
            {
                HandlePong(conn, cmd);
            }
            else
            {
                //m_netnote->OnInnerCommand(tt, this, conn, cmd);
            }
        }

    }
#if CONSOLE_CLIENT
    
    /// <summary>
    /// 提供串联执行action的服�    /// </summary>
    public class IoService
    {
        private static readonly BlockingCollection<Action> actionQueue = new BlockingCollection<Action>();

        public static void Run()
        {
            while (RunOnce()) {

            }
        }

        public static bool RunOnce(int ms = Timeout.Infinite)
        {
            Action act;
            if (!actionQueue.TryTake(out act, ms))
            { //如果第一次取action就没有东西，则等待一个时�                return false;
            }

            act();

            while (actionQueue.TryTake(out act))
            { //看看是不是有更多的act可以取出来，有的话就取出执行，没有就直接返回�                act();
            }

            return true;
        }

        public static void Post(Action act)
        {
            actionQueue.Add(act);
        }

    }
#endif // CONSOLE_CLIENT

    class NetInterface : IEventDispatcher
    {
        private IEventNotifier m_notifier = null;
        private BaseUtil.NetConnector m_connector = null;
        private PacketHandler m_pack_handler = null;

#if CONSOLE_CLIENT
        private int m_waiting_process = 0;
#endif // CONSOLE_CLIENT

        private AutoResetEvent m_ev_msg = new AutoResetEvent(false);
        private LinkedList<NetEvent> m_events = new LinkedList<NetEvent>();
        public bool ForceV6
        {
            get { return m_connector.ForceV6; }
            set { m_connector.ForceV6 = value; }
        }

        public NetInterface(IEventNotifier notifier)
        {
            m_notifier = notifier;
            m_connector = new NetConnector(this);
            m_pack_handler = new PacketHandler(this);
        }
        public void Connect(string address, ushort port)
        {
            m_connector.Connect(address, port);
        }
        public void PostEvent(NetEvent ev)
        {
            lock (m_events)
            {
                m_events.AddLast(ev);
#if CONSOLE_CLIENT
#else
                m_ev_msg.Set();
#endif // CONSOLE_CLIENT
            }
#if CONSOLE_CLIENT
            if (Interlocked.CompareExchange(ref m_waiting_process, 1, 0) == 0)
            {
                IoService.Post(OnProcessEvents);
            }
#endif // CONSOLE_CLIENT
        }

#if CONSOLE_CLIENT
        public void OnProcessEvents()
        {
            if (Interlocked.CompareExchange(ref m_waiting_process, 0, 1) != 1)
                return;
            LinkedList<NetEvent> events = null;
            lock (m_events)
            {
                if (m_events.Count == 0)
                    return;
                events = m_events;
                m_events = new LinkedList<NetEvent>();
            }
            while (events.Count > 0)
            {
                NetEvent ev = events.First.Value;
                events.RemoveFirst();
                ProcessEvent(ev);
            }
        }
#endif
        public void ProcessEvents(int millisecondsTimeOut)
        {
            m_ev_msg.WaitOne(millisecondsTimeOut);

            LinkedList<NetEvent> events = null;
            lock (m_events)
            {
                if (m_events.Count == 0)
                    return;
                events = m_events;
                m_events = new LinkedList<NetEvent>();
            }
            while (events.Count > 0)
            {
                NetEvent ev = events.First.Value;
                events.RemoveFirst();
                ProcessEvent(ev);
            }
        }

        private void ProcessEvent(NetEvent ev)
        {
            if (ev.type == NetEvent.Type.IncomingMessage)
            {
                m_notifier.HandleMessage(ev.conn, ev.message);
            }
            else if (ev.type == NetEvent.Type.KeepAlive)
            {
                m_pack_handler.SendPing(ev.conn);
            }
            else if (ev.type == NetEvent.Type.InnerCommand)
            {
                m_pack_handler.HandleInnerMessage(ev.conn, ev.cmd);
            }
            else if (ev.type == NetEvent.Type.ConnectionClose)
            {
                m_connector.OnClose(ev.conn);
                m_notifier.OnConnectionClose(ev.conn);
            }
            else if (ev.type == NetEvent.Type.ConnectionEstablish)
            {
                m_notifier.OnConnectionEstablish(ev.conn);
            }
            else if (ev.type == NetEvent.Type.ConnectFail)
            {
                m_notifier.OnConnectFail(ev.conn);
            }
            else
            {
                //wrong!
            }
        }

        public void RegisterPingHandler(BaseUtil.PingResultHandler cb)
        {
            m_pack_handler.PingResultCallback += cb;
        }
        public void UnRegisterPingHandler(BaseUtil.PingResultHandler cb)
        {
            m_pack_handler.PingResultCallback -= cb;
        }
    }
}
