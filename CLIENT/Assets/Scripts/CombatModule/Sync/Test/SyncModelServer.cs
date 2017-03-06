using System;
using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    /*
     * 假设客户端和服务器已经建立连接
     */
    class PlayerData
    {
        public long m_pstid = 0L;
        public int m_latency = 0;
        public bool m_ready = false;
        public bool m_loaded = false;
    }
    public class SyncModelServer
    {
        DateTime m_dt_original = DateTime.Now;
        SortedDictionary<long, PlayerData> m_players = new SortedDictionary<long,PlayerData>();
        TestCombatServer m_combat_server;

        public SyncModelServer()
        {
        }

        public int GetCurrentTime()
        {
            TimeSpan ts = DateTime.Now - m_dt_original;
            return (int)(ts.TotalMilliseconds);
        }

        #region 模拟客户端消息处理
        public void OnNetworkMessage_PlayerJoin(long player_pstid)
        {
            PlayerData pd = new PlayerData();
            pd.m_pstid = player_pstid;
            pd.m_ready = false;
            m_players[player_pstid] = pd;
        }

        public void OnNetworkMessage_PlayerQuit(long player_pstid)
        {
            m_players.Remove(player_pstid);
            CheckAllReady();
        }

        public void OnNetworkMessage_PlayerReady(long player_Pstid)
        {
            PlayerData pd;
            if (!m_players.TryGetValue(player_Pstid, out pd))
                return;
            pd.m_ready = true;
            CheckAllReady();
        }

        public void OnNetworkMessage_PlayerLoadingComplete(long player_Pstid)
        {
            PlayerData pd;
            if (!m_players.TryGetValue(player_Pstid, out pd))
                return;
            pd.m_loaded = true;
            CheckAllLoaded();
        }

        public void OnNetworkMessage_SyncCommands(List<Command> commands)
        {
            for (int i = 0; i < commands.Count; ++i)
                m_combat_server.GetSyncServer().PushClientCommand(commands[i]);
        }
        #endregion

        void CheckAllReady()
        {
            if (m_players.Count <= 1)
                return;
            bool all_ready = true;
            var enumerator = m_players.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value.m_ready == false)
                {
                    all_ready = false;
                    break;
                }
            }
            if (!all_ready)
                return;
            NetworkMessages_StartLoading msg = new NetworkMessages_StartLoading();
            enumerator = m_players.GetEnumerator();
            while (enumerator.MoveNext())
            {
                msg.AddPlayer(enumerator.Current.Value.m_pstid);
            }
            //ZZWTODO 发送消息

            m_combat_server = new TestCombatServer();
            m_combat_server.Initializa();
            for (int i = 0; i < msg.m_player_pstids.Count; ++i)
                m_combat_server.AddPlayer(msg.m_player_pstids[i]);
        }

        void CheckAllLoaded()
        {
            bool all_loaded = true;
            var enumerator = m_players.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value.m_loaded == false)
                {
                    all_loaded = false;
                    break;
                }
            }
            if (!all_loaded)
                return;
            int latency = 100;
            NetworkMessages_StartGame msg = new NetworkMessages_StartGame();
            msg.m_latency = latency;
            //ZZWTODO 发送消息

            m_combat_server.StartCombat(GetCurrentTime(), latency);
        }

        public void SendSyncCommands(List<Command> commands)
        {
        }
    }
}