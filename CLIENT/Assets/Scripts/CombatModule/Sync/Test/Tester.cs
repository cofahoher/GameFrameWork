using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface INetwork
    {
        void SendToServer(NetworkMessage msg);
        void SendToClient(NetworkMessage msg);
    }

    public class DummyNetwork : INetwork
    {
        protected List<NetworkMessage> m_server_msgs = new List<NetworkMessage>();
        protected Dictionary<long,List<NetworkMessage>> m_clients_msgs = new Dictionary<long,List<NetworkMessage>>();

        public void SendToServer(NetworkMessage msg)
        {
            m_server_msgs.Add(msg);
        }

        public void SendToClient(NetworkMessage msg)
        {
            long player_pstid = msg.PlayerPstid;
            if (player_pstid == -1L)
            {
                var enumerator = m_clients_msgs.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Value.Add(msg);
                }
            }
            else
            {
                List<NetworkMessage> list;
                if (m_clients_msgs.TryGetValue(player_pstid, out list))
                    list.Add(msg);
            }
        }

        public void AddNetWorkPlayer(long player_pstid)
        {
            List<NetworkMessage> list;
            if (m_clients_msgs.TryGetValue(player_pstid, out list))
                return;
            m_clients_msgs[player_pstid] = new List<NetworkMessage>();
        }

        public List<NetworkMessage> GetServerMsgs()
        {
            return m_server_msgs;
        }

        public List<NetworkMessage> GetClientMsgs(long player_pstid)
        {
            List<NetworkMessage> list;
            m_clients_msgs.TryGetValue(player_pstid, out list);
            return list;
        }
    }




    public class Tester : DummyNetwork
    {
        bool m_init = false;
        UnityEngine.MonoBehaviour m_mono;
        System.Random m_ran;

        SyncModelServer m_server;
        List<SyncModelClient> m_clients = new List<SyncModelClient>();
        int CLIENT_COUNT = 1;

        public Tester(UnityEngine.MonoBehaviour mono)
        {
            m_mono = mono;
        }

        public void Init()
        {
            m_ran = new System.Random();
            m_server = new SyncModelServer(this);
            for (int i = 1; i <= CLIENT_COUNT; ++i)
            {
                long player_pstid = i;
                int latency = m_ran.Next();
                AddNetWorkPlayer(player_pstid);
                SyncModelClient client = new SyncModelClient(this, player_pstid, latency);
                m_clients.Add(client);
                m_server.OnNetworkMessage_PlayerJoin(player_pstid, latency);
            }
            for (int i = 0; i < m_clients.Count; ++i)
            {
                m_server.OnNetworkMessage_PlayerReady(m_clients[i].GetPstid());
            }
            m_init = true;
        }

        public void Update()
        {
            if (!m_init)
                return;

            List<NetworkMessage> msgs;
            for (int i = 0; i < m_clients.Count; ++i)
            {
                SyncModelClient client = m_clients[i];
                msgs = GetClientMsgs(client.GetPstid());
                for (int j = 0; j < msgs.Count; ++j)
                    client.HandleNetworkMessages(msgs[j]);
                msgs.Clear();
                client.Update();
            }

            msgs = GetServerMsgs();
            for (int i = 0; i < msgs.Count; ++i)
                m_server.HandleNetworkMessages(msgs[i]);
            msgs.Clear();
            m_server.Update();
        }

        IEnumerator ServerCoutine()
        {
            yield return null;
        }

        IEnumerator ClientCoutine(int client_index)
        {
            yield return null;
        }
    }
}