using System;
using System.Collections.Generic;

public class ReqRepHead
{
    public int serial = 0;
    public int seq_or_ack = 0;
    public void ToStream(BaseUtil.NetOutStream outs)
    {
        outs.Write(serial);
        outs.Write(seq_or_ack);
    }
    public void FromStream(BaseUtil.NetInStream ins)
    {
        ins.Read(ref serial);
        ins.Read(ref seq_or_ack);
    }
}

public abstract class CEventMobileReqRepBase : BaseUtil.NetMessage
{
    ReqRepHead m_head = new ReqRepHead();
    public ReqRepHead Head { get { return m_head; } }
    public override sealed void FromStream(BaseUtil.NetInStream ins)
    {
        m_head.FromStream(ins);
        BaseUtil.ProtoBufMessage.FromStream(ins, this, (uint)m_head.serial);
    }
    public override sealed void ToStream(BaseUtil.NetOutStream outs)
    {
        m_head.ToStream(outs);
        BaseUtil.ProtoBufMessage.ToStream(outs, this, (uint)m_head.serial);
        //BaseUtil.ProtoBufSerializer.SaveStream(this, outs);
    }

    public override bool ENCRYPT { get { return true; } }

    public CEventMobileReqRepBase()
    {
        RecvTick = Environment.TickCount;
    }
}

public class UnknownMessage : CEventMobileReqRepBase
{
    int m_clsid;

    public UnknownMessage(int clsid)
    {
        m_clsid = clsid;
    }

    public override int CLSID 
    { 
        get
        {
            return m_clsid;
        }
    }

}

public abstract class RequestMessage : CEventMobileReqRepBase
{
    public virtual bool IsSilent()
    {
        return false;
    }
}

public abstract class ReplyMessage : CEventMobileReqRepBase
{
}

public abstract class ServerPushMessage : CEventMobileReqRepBase
{
}

public abstract class InGameMessage : CEventMobileReqRepBase
{
    public override bool ENCRYPT { get { return false; } }

}
