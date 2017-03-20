using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    /*
     * 假设客户端和服务器已经建立连接
     */
    public class SyncModelClient
    {
        INetwork m_network;
        long m_local_player_pstid = 0L;
        int m_latency = 50;

        DateTime m_dt_original = DateTime.Now;
        TestCombatClient m_combat_client;

        public SyncModelClient(INetwork network, long local_player_pstid, int latency)
        {
            m_network = network;
            m_local_player_pstid = local_player_pstid;
            m_latency = latency;
        }

        public long GetPstid()
        {
            return m_local_player_pstid;
        }

        public int GetLatency()
        {
            return m_latency;
        }

        public INetwork GetNetwork()
        {
            return m_network;
        }

        public void Update()
        {
            if (m_combat_client != null)
                m_combat_client.OnUpdate(m_network.GetCurrentTime());
        }

        #region 模拟服务器消息处理
        public void HandleNetworkMessages(NetworkMessage msg)
        {
            bool error = false;
            switch (msg.Type)
            {
            case NetworkMessageType.StartLoading:
                OnNetworkMessage_StartLoading(msg as NetworkMessages_StartLoading);
                break;
            case NetworkMessageType.StartGame:
                OnNetworkMessage_StartGame(msg as NetworkMessages_StartGame);
                break;
            case NetworkMessageType.SyncCommands:
                OnNetworkMessage_SyncCommands(msg as NetworkMessages_SyncCommands);
                break;
            default:
                error = true;
                break;
            }
        }

        public void OnNetworkMessage_StartLoading(NetworkMessages_StartLoading msg)
        {
            m_combat_client = new TestCombatClient(m_network);
            m_combat_client.Initializa(m_local_player_pstid);
            for (int i = 0; i < msg.m_player_pstids.Count; ++i)
                m_combat_client.AddPlayer(msg.m_player_pstids[i]);
        }

        public void OnNetworkMessage_StartGame(NetworkMessages_StartGame msg)
        {
            m_combat_client.StartCombat(m_network.GetCurrentTime(), m_latency);
        }

        public void OnNetworkMessage_SyncCommands(NetworkMessages_SyncCommands msg)
        {
            List<Command> commands = msg.m_commands;
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