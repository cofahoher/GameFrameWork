using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    /*
     * 假设客户端和服务器已经建立连接
     */
    public class SyncModelClient : MonoBehaviour
    {
        DateTime m_dt_original = DateTime.Now;
        long m_local_player_pstid = 0L;
        int m_latency = 50;
        TestCombatClient m_combat_client;

        public SyncModelClient()
        {
            System.Random ran = new System.Random();
            m_local_player_pstid = ran.Next();
        }

        public int GetCurrentTime()
        {
            TimeSpan ts = DateTime.Now - m_dt_original;
            return (int)(ts.TotalMilliseconds);
        }

        void Update()
        {
            if (m_combat_client != null)
                m_combat_client.OnUpdate(GetCurrentTime());
        }

        #region 模拟服务器消息处理
        public void OnNetworkMessage_StartLoading(NetworkMessages_StartLoading msg)
        {
            m_combat_client = new TestCombatClient(this);
            m_combat_client.Initializa(m_local_player_pstid);
            for (int i = 0; i < msg.m_player_pstids.Count; ++i)
                m_combat_client.AddPlayer(msg.m_player_pstids[i]);
        }

        public void OnNetworkMessage_StartGame(NetworkMessages_StartGame msg)
        {
            m_combat_client.StartCombat(GetCurrentTime(), m_latency);
        }

        public void OnNetworkMessage_SyncCommands(List<Command> commands)
        {
            for (int i = 0; i < commands.Count; ++i)
                m_combat_client.GetSyncClient().PushServerCommand(commands[i]);
        }
        #endregion
        
        #region 供CombatClient调用的接口
        public void OnLoadingComplete()
        {
        }

        public void SendSyncCommands(List<Command> commands)
        {
        }

        public void OnGameOver()
        {
        }
        #endregion
    }
}