using System;

namespace BaseUtil
{
    internal interface IEventDispatcher
    {
        void PostEvent(NetEvent ev);
    }

    public interface IEventNotifier
    {
        void OnConnectFail(INetConnection conn);
        void OnConnectionEstablish(INetConnection conn);
        void OnConnectionClose(INetConnection conn);

        void HandleMessage(INetConnection conn, NetMessage msg);
    }
}
