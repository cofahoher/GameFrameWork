using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using BaseUtil;

public class HostInfo
{
    private string m_net_address = "127.0.0.1";
    private ushort m_net_port = 33502;

    public string Address { get { return m_net_address; } }
    public ushort Port { get { return m_net_port; } }
    public HostInfo(string addr, ushort port)
    {
        m_net_address = addr;
        m_net_port = port;
    }
}

public class ClientToServer : BaseUtil.IEventNotifier
{
	public enum Status
    {
        Idle,
        Connecting,
        Connected,
    }

    Status m_state = Status.Idle;
    public Status State { get { return m_state; } set { m_state = value; LogWrapper.LogInfo("State set to " + value); } }
    List<HostInfo> m_server_hosts = new List<HostInfo>();
    int m_host_index = 0;
		
    public BaseUtil.INetConnection Connection { set { m_conn = value; } get { return m_conn; } }
    private BaseUtil.INetConnection m_conn = null;

    CallCenter m_callcenter = null;
    NetInterface m_netintf = null;

    HostInfo m_current_host;

    public ClientToServer(CallCenter call_center)
    {
        m_netintf = new NetInterface(this);
        m_callcenter = call_center;
    }

    public int Ping
    {
        get
        {
            if (m_conn != null)
            {
                return m_conn.Ping;
            }
            return -1;
        }
    }


    #region IConnectionNotifier 成员

    protected virtual bool NeedChangeServer()
    {
        return false;
    }
    public virtual void OnConnectFail(BaseUtil.INetConnection conn)
    {
        LogWrapper.LogDebug("ClientToServer.OnConnectFail");
        Connection = null;
		State = Status.Idle;

        m_host_index++;

        if (m_host_index >= m_server_hosts.Count)
        {
            m_host_index = 0;
            m_callcenter.OnConnectFail();
        }
        else
        {
            Connect();
        }
    }

    public void OnConnectionEstablish(BaseUtil.INetConnection conn)
    {
		LogWrapper.LogDebug("ClientToServer.OnConnectionEstablish");
        Connection = conn;
        State = Status.Connected;
        m_callcenter.OnConnectionEstablish();
		
    }

    public void OnConnectionClose(BaseUtil.INetConnection conn)
    {
        LogWrapper.LogDebug("ClientToServer.OnConnectionClose");
        if (conn == Connection)
        {
            Connection = null;
            State = Status.Idle;
        }
        m_callcenter.OnConnectionClose();
    }

    public void HandleMessage(INetConnection conn, NetMessage msg)
    {
        LogWrapper.LogErrorDebugWrapper("HandleMessage:", msg.CLSID);
        m_callcenter.HandleRawMessage(msg);
    }

#endregion

#region Request相关

    public void SendMessage(CEventMobileReqRepBase msg, bool auto_connect = true)
    {
        if (Connection != null)
        {
            Connection.SendMessage(msg);
        }
        else if (auto_connect)
        {
			Connect();
        }
    }
#endregion
	
    private void TryConnect()
    {
		if (m_host_index < m_server_hosts.Count)
		{
            State = Status.Connecting;
            m_current_host = m_server_hosts[m_host_index];
#if CONSOLE_CLIENT || UNITY_IOS || UNITY_IPHONE
            m_netintf.Connect(m_current_host.Address, m_current_host.Port);
#else
            GameGlobal.MSDK.DnsInvoker.WGGetHostByNameAsync(m_current_host.Address, HttpDnsLookupCallBack);
#endif
		}
        else 
        {
            LogWrapper.LogError("TryConnect Error");
        }
    }
	
	
	public void ClearServerAddress()
    {
		m_server_hosts.Clear();
        m_host_index = 0;
    }
	
    public void AddServerAddress(string addr, ushort port)
    {
		m_server_hosts.Add(new HostInfo(addr, port));
    }

    public void Connect()
    {
        //退出游戏时候，不再重连了
        if( AppQuit ) return;

		if (State == Status.Idle)
        {
            TryConnect();
        }
    }

    protected bool AppQuit = false;
    public void OnAppQuit()
    {
        AppQuit = true;
        if (null != m_conn)
        {
            m_conn.Quit();
            m_conn = null;
        }
        State = Status.Idle;
    }
    public void Disconnect()
    {
        if (m_conn != null)
        {
            m_conn.CloseConnection();
            m_conn = null;
        }
        State = Status.Idle;
    }
    public void Update(int maxwaitms)
    {
#if CONSOLE_CLIENT
        m_netintf.OnProcessEvents();
#else
        m_netintf.ProcessEvents(maxwaitms);//最多等待maxwaitms毫秒
#endif // CONSOLE_CLIENT
        
    }

    public void RegisterPingHandler(BaseUtil.PingResultHandler cb)
    {
        m_netintf.RegisterPingHandler(cb);
    }
    public void UnRegisterPingHandler(BaseUtil.PingResultHandler cb)
    {
        m_netintf.UnRegisterPingHandler(cb);
    }

    void HttpDnsLookupCallBack(List<string> host_ips)
    {
        if(State == Status.Connecting && m_current_host != null)
        {
            string resolved_ip = null;
            if (host_ips != null && host_ips.Count > 0)
            {
                resolved_ip = host_ips[0];
            }
            if (string.IsNullOrEmpty(resolved_ip))
            {
                resolved_ip = m_current_host.Address;
            }
            m_netintf.Connect(resolved_ip, m_current_host.Port);
        }
        else
        {
            LogWrapper.LogError("dns callback error, state=", State.ToString());
        }

        m_current_host = null;
    }
}
