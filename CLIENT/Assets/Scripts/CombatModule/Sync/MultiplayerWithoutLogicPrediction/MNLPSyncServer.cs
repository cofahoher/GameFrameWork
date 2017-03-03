using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class MNLPSyncServer : SyncServer
    {
        Dictionary<long, PlayerSyncData> m_sync_data_of_players = new Dictionary<long, PlayerSyncData>();
        int m_start_time = 0;

        public MNLPSyncServer()
        {
        }

        public override void Init(ILogicWorld logic_world)
        {
            m_logic_world = logic_world;
            m_command_synchronizer = new MNLPServerCommandSynchronizer();
            m_world_syhchronizer = new MNLPServerWorldSynchronizer(logic_world, m_command_synchronizer);
        }

        public override void AddPlayer(long player_pstid)
        {
            PlayerSyncData psd = new PlayerSyncData();
            m_sync_data_of_players[player_pstid] = psd;
            m_command_synchronizer.AddPlayer(player_pstid);
        }

        public override void RemovePlayer(long player_pstid)
        {
            m_sync_data_of_players.Remove(player_pstid);
        }

        public override void UpdatePlayerLatency(long player_pstid, int latency)
        {
            PlayerSyncData psd;
            if (!m_sync_data_of_players.TryGetValue(player_pstid, out psd))
                return;
            psd.Latency = latency;
        }

        public override void Start(int current_time)
        {
        }

        public override void Update(int current_time)
        {
        }

        public override void PushClientCommand(Command command)
        {
        }
    }
}