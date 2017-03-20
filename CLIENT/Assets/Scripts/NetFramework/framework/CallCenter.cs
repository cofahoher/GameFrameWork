using System;
using System.Collections;
using System.Collections.Generic;

public class RequestInfo
{
    public int serial;
    public RequestMessage reqmessage;
    public CallCenter.ReplyDelegate callback;
    public object param;
    public int start_tick;
    public int send_count;
    public int send_tick;   //统计时间需要扣除连接时间
    public bool busy;
    public bool sended;     //压测数据收集，是否真的已经发出去了
    public long start_frame;

    public RequestInfo(int _serial, RequestMessage req, CallCenter.ReplyDelegate _callback, object _param, int _start_tick)
    {
        serial = _serial;
        reqmessage = req;
        callback = _callback;
        param = _param;
        start_tick = _start_tick;
        send_tick = _start_tick;
        send_count = 0;
        busy = false;
        sended = false;
        start_frame = GameGlobal.GameLogic.FrameCount;
    }
    public void OnSend(bool connected)
    {
        ++send_count;
        sended = connected;
        send_tick = Environment.TickCount;
    }

    public int ReponseTick()
    {
        if (!sended) return 0;

        return Environment.TickCount - send_tick;
    }
}

public class ReplyInfo
{
    public CallCenter.ResultType res;
    public ReplyMessage msg;
    public bool Succ { get { return (res == CallCenter.ResultType.Normal); } }
}

//Callcenter提供一个消息队列机
//将消息排队、等待回答后回调、断线重连重
//同时超时等待等事件，与MainStateManager配合实现转菊花、锁UI等功
public class CallCenter
{
    public delegate int ReplyDelegate(CallCenter.ResultType res, ReplyMessage msg, object param);
    public delegate void PushHandlerDelegate(ServerPushMessage msg);
    public delegate void InGameHandlerDelegate(InGameMessage msg);

    #region 成员变量
    public enum ResultType
    {
        Normal = 0,
        ConnectionClosed = -1,
        TimeOut = -2,
        OtherErr = -3,
    }

    private int m_serial = 100;
    private int m_ack = 0;
    private List<RequestInfo> m_requests = new List<RequestInfo>();

    protected ClientToServer m_to_server = null;

    bool m_started = false;

#if CONSOLE_CLIENT
	const int s_TTL = 20 * 1000;//time to live
    const int s_TTR1 = 15 * 1000;//time to reconnect
    const int s_TTR2 = 15 * 1000;//time to reconnect
#else
    const int s_TTL = 20 * 1000;//time to live
    const int s_TTR1 = 2 * 1000;//time to reconnect
    const int s_TTR2 = 15 * 1000;//time to reconnect
#endif
    bool m_timeout = false;
    public bool Timeout
    {
        get { return m_timeout; }
    }
	public delegate void OnRequestBegin(CallCenter cc, RequestInfo req);
    public OnRequestBegin RequestBeginFunc = null;
    public delegate void OnRequestTimeLong(CallCenter cc, RequestInfo req);
    public OnRequestTimeLong RequestTimeLongFunc = null;
    public delegate void OnRequestTimeOut(CallCenter cc, bool silent);
    public OnRequestTimeOut RequestTimeOutFunc = null;
    public delegate void OnRequestComplete(CallCenter cc, RequestInfo req, ResultType res);
    public OnRequestComplete RequestCompleteFunc = null;
    public Action<CallCenter, BaseUtil.NetMessage> OnReceiveMessage = null;

    public delegate void OnServerLogout(int err, string reason);
    public OnServerLogout ServerLogoutFunc = null;


    bool m_is_login = false;
    public bool IsLogin
    {
        get { return m_is_login; }
    }

    bool m_close_wait = false;
    public bool CloseWait
    {
        get { return m_close_wait; }
    }

    private static int SysNetworkNotReachable
    {
        get
        {
            #if CONSOLE_CLIENT
            return 0; //UnityEngine.NetworkReachability.ReachableViaLocalAreaNetwork
            #else
            return (int)UnityEngine.NetworkReachability.NotReachable;
            #endif
        }
    }

    int m_network_reachability = SysNetworkNotReachable;

    private int SysNetworkRechabiltiy
    {
        get
        {
        #if CONSOLE_CLIENT
            return 2; //UnityEngine.NetworkReachability.ReachableViaLocalAreaNetwork
        #else
            return (int)UnityEngine.Application.internetReachability;
        #endif
        }
    }

    #endregion
    //-------------------------------------

    public CallCenter()
    {
        Init();
    }

    public void Reset()
    {
        m_timeout = false;
        m_serial = 100;
        m_ack = 0;
        ClearAllRequests();
        m_started = false;
        m_svr_msg_list.Clear();
        m_msg_state = MsgState.None;
    }

    protected void Init()
    {
        m_to_server = new ClientToServer(this);
    }
    //------- callbacks ---------------
    public delegate void OnConnectEstablishHandler();
    public event OnConnectEstablishHandler ConnCallback;
    public delegate void OnConnectFailHandler();
    public event OnConnectFailHandler ConnFailCallback;
    public delegate void OnConnectCloseHandler();
    public event OnConnectCloseHandler ConnCloseCallback;


    public void OnConnectFail()
    {
        LogWrapper.LogInfo("CallCenter.OnConnectFail");
        if (ConnFailCallback != null)
        {
            ConnFailCallback();
        }
    }

    public void OnConnectionEstablish()
    {
        LogWrapper.LogInfo("CallCenter.OnConnectionEstablish");
        if (ConnCallback != null)
        {
            ConnCallback();
        }

        m_network_reachability = SysNetworkRechabiltiy; //UnityEngine.Application.internetReachability;
    }
    public void OnConnectionClose()
    {
        m_is_login = false;
        m_close_wait = false;
        if (ConnCloseCallback != null)
        {
            ConnCloseCallback();
        }
    }
    //---------------------------------
    //--------
    #region 到底层的函数转发
	
	public void ClearServerAddress()
    {
        m_to_server.ClearServerAddress();
    }
	
	public void AddServerAddress(string addr, ushort port)
    {
        m_to_server.AddServerAddress(addr, port);
    }

    public void Connect()
    {
        m_to_server.Connect();
    }

    public void Update(int maxwaitms)
    {
        UpdateRawMessage();
        m_to_server.Update(maxwaitms);
        UpdateCallCenter();

    }

    public void Disconnect()
    {
        m_is_login = false;

        if (Connected)
        {
            m_to_server.Disconnect();
            m_close_wait = true;
        }
    }

    public bool Connected
    {
        get
        {
            return m_to_server.Connection != null;
        }
    }

    public bool Connecting
    {
        get
        {
            return m_to_server.State == ClientToServer.Status.Connecting;
        }
    }

    public bool Busy
    {
        get 
        {
            if (m_requests.Count > 0)
            {
                foreach (RequestInfo ri in m_requests)
                {
                    if (ri.busy)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    //--------
    #endregion
    #region Request管理


    public IEnumerator Request(RequestMessage msg, ReplyInfo reply)
    {
        if (!m_started)
        {
            if (msg is RequestMobileLogin)
            {
                m_started = true;
            }
            else
            {
                reply.res = ResultType.OtherErr;
                LogWrapper.LogCritical("request while callcenter stopped.");
                yield break;
            }
        }
        Task task = TaskManager.Instance.CurrTask;
        if (task == null)
        {
            reply.res = ResultType.OtherErr;
            LogWrapper.LogCritical("request not from corotine."); 
            yield break;
        }

        task.Data = reply;

        if (SendRequest(msg, RequestCallBack, task.Id) == 0)
        {
            reply.res = ResultType.ConnectionClosed;
            yield break;
        }

        yield return task.Suspend();
        if (reply.Succ)
        {
            task.FinishCallback = RequestTaskFinishCallback;
        }

        m_msg_state = MsgState.None;
    }

    int RequestCallBack(CallCenter.ResultType res, ReplyMessage msg, object param)
    {
        Task task = TaskManager.Instance.FindTask((int)param);
        if (task != null)
        {
            ReplyInfo reply = task.Data as ReplyInfo;
            if (reply != null)
            {
                reply.msg = msg;
                reply.res = res;
                task.Resume();
                return 0;
            }
            else
            {
                LogWrapper.LogCritical("request callback reply data error!");
            }
        }
        else
        {
            LogWrapper.LogCritical("request callback cant find task!");
        }
        return -1;
    }

    int SendRequest(RequestMessage msg, ReplyDelegate callback, object param)
    {
        int serial = -100;
        if (!(msg is RequestMobileLogin))
        {
            serial = generateSerial();
        }

        return SendRequestImpl(serial, msg, callback, param);
    }

    void RequestTaskFinishCallback(int task_id)
    {
        GameGlobal.GameEventManager.Dispatch(new RequestTaskFinishEvent(task_id));
    }

    int generateSerial()
    {
        int serial = m_serial++;
        if (m_serial == 0x7fffffff)
        {
            m_serial = 100;
        }
        return serial;
    }

    int SendRequestImpl(int serial, RequestMessage msg, ReplyDelegate callback, object param)
    {
        int tick = Environment.TickCount;
        msg.Head.serial = serial;
        msg.Head.seq_or_ack = m_ack;

        RequestInfo req = new RequestInfo(serial, msg, callback, param, tick);        
        req.OnSend( Connected );
        m_to_server.SendMessage(msg);
        m_requests.Add(req);

        if (!req.reqmessage.IsSilent() && RequestBeginFunc != null)
        {
            RequestBeginFunc(this, req);
        }

        LogWrapper.LogInfo("SendRequest: serial=" + serial + ", type=" + msg);
        return serial;
    }

    void HandleReply(ReplyMessage msg)
    {
        int serial = msg.Head.serial;
        LogWrapper.LogInfo("HandleReply: serial=" + serial + ", type=" + msg);

        if (msg is ReplyMobileLoginResult)
        {
            ReplyMobileLoginResult reply_msg = msg as ReplyMobileLoginResult;
            if (reply_msg.m_n_ret == 0)
            {
                m_is_login = true;
            }
        }
        else if (msg is ReplyMobileLogoutResult)
        {
            if (ServerLogoutFunc != null)
            {
                ReplyMobileLogoutResult ev = msg as ReplyMobileLogoutResult;
                ServerLogoutFunc(ev.err, ev.reason);
            }
        }

        bool found = false;
        foreach (RequestInfo info in m_requests)
        {
            if (info.serial == serial)
            {
                m_requests.Remove(info);

                CollectProfData(info, true, msg.QueueTick);

				found = true;
                if (!info.reqmessage.IsSilent() && RequestCompleteFunc != null)
                {
                    RequestCompleteFunc(this, info, ResultType.Normal);
                }
                if (info.callback != null)
                {
                    try
                    {
                        if (info.callback(ResultType.Normal, msg, info.param) == -1)
                        {
                            m_msg_state = MsgState.None;
                        }
                    }
                    catch (Exception e)
                    {
                        LogWrapper.LogError("callback exception " + e.ToString());
                    }
                }
                break;
            }
        }

        if (!found)
        {
            LogWrapper.LogInfo("reply not registered: serial=" + serial + ",type=" + msg);
            m_msg_state = MsgState.None;
        }
    }

    public void UpdateCallCenter()
    {
        CheckTimeOut();
    }

    public int Ping
    {
        get
        {
            return m_to_server.Ping;
        }
    }

    public void ProcessTimeLong()
    {
        m_timeout = false;

        int tick = Environment.TickCount;

        foreach (var req in m_requests)
        {
            req.start_tick = req.send_tick = tick;
            req.start_frame = GameGlobal.GameLogic.FrameCount;


            if (!req.reqmessage.IsSilent() && RequestTimeLongFunc != null)
            {
                RequestTimeLongFunc(this, req);
            }
        }
    }

    public void CheckResend()
    {

        ProcessTimeLong();

        foreach (var req in m_requests)
        {
            if (req.reqmessage is RequestMobileLogin)
            {
                CollectProfData(req, false, 0);
                LogWrapper.LogInfo("Resending serial " + req.serial + ":" + req.reqmessage);
                req.OnSend( Connected );
                req.reqmessage.Head.seq_or_ack = m_ack;
                m_to_server.SendMessage(req.reqmessage);
                return;
            }
        }

        foreach (var req in m_requests)
        {
            CollectProfData(req, false, 0);
            LogWrapper.LogInfo("Resending serial " + req.serial + ":" + req.reqmessage);
            req.OnSend( Connected );
            req.reqmessage.Head.seq_or_ack = m_ack;
            m_to_server.SendMessage(req.reqmessage);
        }
    }

    public void OnAppResume()
    {
        m_network_reachability = SysNetworkNotReachable;
    }

    void CheckTimeOut()
    {
        if (m_timeout)
        {
            return;
        }
        int current = Environment.TickCount;
        long frame = GameGlobal.GameLogic.FrameCount;

        for (int i = 0, max = m_requests.Count; i < max; i++ )
        {
            RequestInfo req = m_requests[i];
            int wait_time = current - req.start_tick;
            long wait_frame = frame - req.start_frame;
            int connect_time = current - req.send_tick;

            bool reconnect = false;
            if (Connected && wait_frame >= 30)
            {
                int ttr = (m_network_reachability != SysNetworkRechabiltiy) ? s_TTR1 : s_TTR2;
                #if UNITY_EDITOR
                ttr = s_TTR1;
                #endif
                if (connect_time > ttr)
                {
                    reconnect = true;
                }
            }
            bool time_out = wait_time > s_TTL;

            if (!req.reqmessage.IsSilent())
            {
                if (time_out)
                {
                    m_timeout = true;
                    if (RequestTimeOutFunc != null)
                    {
                        RequestTimeOutFunc(this, req.reqmessage is RequestMobileLogin);
                    }
                    break;
                }
                else if (!req.busy && current - req.start_tick > 500)
                {
                    req.busy = true;
                    if (RequestTimeLongFunc != null)
                    {
                        RequestTimeLongFunc(this, req);
                    }
                }
            }

            if (reconnect)
            {
                LogWrapper.LogInfo("disconnect. connect_time time = ", connect_time);
                Disconnect();
                break;
            }
        }

    }

    public void ClearAllRequests()
    {
        List<RequestInfo> req_list = new List<RequestInfo>();
        req_list.AddRange(m_requests);
        m_requests.Clear();

        foreach (RequestInfo req in req_list)
        {
            if (null != RequestCompleteFunc)
            {
                RequestCompleteFunc(this, req, ResultType.OtherErr);
            }
            if (req.callback != null)
            {
                req.callback(ResultType.OtherErr, null, req.param);
            }
        }
        m_timeout = false;
    }


    public void SendMessageOnly(InGameMessage msg)
    {
        m_to_server.SendMessage(msg, false);
    }

    private void CollectProfData(RequestInfo req, bool succ, int parseTime)
    {
        if( !req.sended ) return;

        #if CONSOLE_CLIENT
        GameGlobal.OnReply(req.reqmessage, req.ReponseTick(), succ, parseTime);
        #endif
    }

    #endregion

    #region Push消息管理

    Dictionary<Type, PushHandlerDelegate> m_push_handler_map = new Dictionary<Type,PushHandlerDelegate>();
    public void RegisterPushHandler(Type msg_type, PushHandlerDelegate callback)
    {
        if (callback != null)
        {
			if(m_push_handler_map.ContainsKey(msg_type))
			{
				m_push_handler_map[msg_type] = callback;
			}
			else 
			{
				m_push_handler_map.Add(msg_type, callback);
			}
        }
        else
        {
            if (m_push_handler_map.ContainsKey(msg_type))
            {
                m_push_handler_map.Remove(msg_type);
            }
        }
    }
    public void UnRegisterPushHandler(Type msg_type)
    {
        if( m_push_handler_map.ContainsKey(msg_type) )
        {
            m_push_handler_map.Remove( msg_type );
        }
    }

    public bool IsRegisterHandler(Type msg_type)
    {
        if (m_push_handler_map.ContainsKey(msg_type))
        {
            return true;
        }
        return false;
    }

    public void ClearHandlers()
    {
        m_push_handler_map.Clear();
    }

    void HandleServerPushMessage(ServerPushMessage msg)
    {
        //GameGlobal.TaskManager.StartTask(HandleServerPushMessageTask(msg, yield_frame));
        PushHandlerDelegate handler = null;
        m_push_handler_map.TryGetValue(msg.GetType(), out handler);
        if (handler != null)
        {
            handler(msg);
        }
        else
        {
            LogWrapper.LogWarning("PushHandler not found for " + msg);
        }
    }

    IEnumerator HandleServerPushMessageTask(ServerPushMessage msg, bool yield_frame)
    {
        if (yield_frame)
        {
            yield return null;
        }

        PushHandlerDelegate handler = null;
        m_push_handler_map.TryGetValue(msg.GetType(), out handler);
        if (handler != null)
        {
            handler(msg);
        }
        else
        {
            LogWrapper.LogWarning("PushHandler not found for " + msg);
        }
    }

    #endregion

    #region     Raw消息处理

    public void HandleRawMessage(BaseUtil.NetMessage msg)
    {
        CEventMobileReqRepBase mobile_msg = msg as CEventMobileReqRepBase;
        if (mobile_msg == null)
        {
            return;
        }
        int seq = mobile_msg.Head.seq_or_ack;
        if (seq > 0 && seq != m_ack + 1)
        {
            LogWrapper.LogError("msg not on demand: serial=", mobile_msg.Head.serial, ", type=", msg, ", seq=", seq, ", ack=", m_ack);
            return;
        }
        else if (seq > 0)
        {
            m_ack = seq;
        }

        if (DoHandleRawMessage(msg) == -1)
        {
            m_svr_msg_list.Add(msg);
            LogWrapper.LogDebug("handle raw message next frame.");
        }
    }

    List<BaseUtil.NetMessage> m_svr_msg_list = new List<BaseUtil.NetMessage>();
    enum MsgState
    {
        None,
        Reply,
    }
    MsgState m_msg_state = MsgState.None;

    void UpdateRawMessage()
    {
        int i = 0;
        for (; i < m_svr_msg_list.Count; i++)
        {
            if (DoHandleRawMessage(m_svr_msg_list[i]) == -1)
            {
                LogWrapper.LogDebug("update raw message next frame.");
                break;
            }
        }
        if (i > 0)
        {
            m_svr_msg_list.RemoveRange(0, i);
        }
    }

    int DoHandleRawMessage(BaseUtil.NetMessage msg)
    {
        if (msg is InGameMessage)
        {
            HandleInGameMessage(msg as InGameMessage);
            return 0;
        }

        if (m_msg_state >= MsgState.Reply)
        {
            return -1;
        }
        if (msg is ReplyMessage)
        {
            m_msg_state = MsgState.Reply;

            DispatchRecvEventNotify(msg);
            HandleReply(msg as ReplyMessage);
        }
        else if (msg is ServerPushMessage)
        {
            DispatchRecvEventNotify(msg);
            HandleServerPushMessage(msg as ServerPushMessage);
        }

        return 0;
    }

    private void DispatchRecvEventNotify(BaseUtil.NetMessage msg)
    {
        if (null == OnReceiveMessage) return;
        OnReceiveMessage(this, msg);
    }

    public void RegisterPingHandler(BaseUtil.PingResultHandler cb)
    {
        m_to_server.RegisterPingHandler(cb);
    }
    public void UnRegisterPingHandler(BaseUtil.PingResultHandler cb)
    {
        m_to_server.UnRegisterPingHandler(cb);
    }
    #endregion

    #region InGame消息处理
    Dictionary<Type, InGameHandlerDelegate> m_ingame_handler_map = new Dictionary<Type, InGameHandlerDelegate>();
    public void RegisterInGameHandler(Type msg_type, InGameHandlerDelegate callback)
    {
        if (callback != null)
        {
            if (m_ingame_handler_map.ContainsKey(msg_type))
            {
                m_ingame_handler_map[msg_type] = callback;
            }
            else
            {
                m_ingame_handler_map.Add(msg_type, callback);
            }
        }
        else
        {
            if (m_ingame_handler_map.ContainsKey(msg_type))
            {
                m_ingame_handler_map.Remove(msg_type);
            }
        }
    }
    public void UnRegisterInGameHandler(Type msg_type)
    {
        if (m_ingame_handler_map.ContainsKey(msg_type))
        {
            m_ingame_handler_map.Remove(msg_type);
        }
    }

    public bool IsRegisterInGameHandler(Type msg_type)
    {
        if (m_ingame_handler_map.ContainsKey(msg_type))
        {
            return true;
        }
        return false;
    }

    public void ClearInGameHandlers()
    {
        m_ingame_handler_map.Clear();
    }

    void HandleInGameMessage(InGameMessage msg)
    {
        InGameHandlerDelegate handler = null;
        m_ingame_handler_map.TryGetValue(msg.GetType(), out handler);
        if (handler != null)
        {
            handler(msg);
        }
        else
        {
            LogWrapper.LogWarning("InGameHandler not found for " + msg);
        }
    }
    #endregion
}
