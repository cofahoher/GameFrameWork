using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class SPSyncServer : SyncServer
    {
        int m_mode = CHECKING_MODE;
        Dictionary<long, PlayerSyncData> m_sync_data_of_players = new Dictionary<long, PlayerSyncData>();
        int m_start_time = 0;

        const int RUNNING_MODE = 1;
        const int CHECKING_MODE = 1;

        public SPSyncServer()
        {
        }

        public override void Init(ILogicWorld logic_world)
        {
            m_logic_world = logic_world;
            m_command_synchronizer = new MNLPServerCommandSynchronizer();
            m_world_syhchronizer = new SPCheckerWorldSynchronizer(logic_world, m_command_synchronizer);
        }

        public override void AddPlayer(long player_pstid)
        {
            PlayerSyncData psd = new PlayerSyncData();
            m_sync_data_of_players[player_pstid] = psd;
            m_command_synchronizer.AddPlayer(player_pstid);
        }

        public override void RemovePlayer(long player_pstid)
        {
            PlayerSyncData psd;
            if (!m_sync_data_of_players.TryGetValue(player_pstid, out psd))
                return;
            psd.SyncState = PlayerSyncData.SYNC_STATE_DEAD;
            m_command_synchronizer.RemovePlayer(player_pstid);
        }

        public override void UpdatePlayerLatency(long player_pstid, int latency)
        {
        }

        public override void Start(int current_time)
        {
            m_start_time = current_time;
            m_world_syhchronizer.Start(current_time);
        }

        public override void Update(int current_time)
        {
            bool forward = m_world_syhchronizer.ForwardTurn();
            if (forward && m_mode == RUNNING_MODE)
            {
                SyncTurnDoneCommand command = new SyncTurnDoneCommand();
                command.PlayerPstid = -1;
                command.SyncTurn = m_world_syhchronizer.GetSynchronizedTurn();
                AddOutputCommand(command);
            }
        }

        public override void PushClientCommand(Command command)
        {
            long player_pstid = command.PlayerPstid;
            PlayerSyncData psd;
            if (!m_sync_data_of_players.TryGetValue(player_pstid, out psd))
                return;
            if (psd.SyncState == PlayerSyncData.SYNC_STATE_IDLE)
                psd.LastMsgSyncTurn = command.SyncTurn;
            if (command.SyncTurn <= m_world_syhchronizer.GetSynchronizedTurn())
                return;
            psd.LastMsgSyncTurn = command.SyncTurn;
            bool valid = m_world_syhchronizer.PushClientCommand(command);
            if (valid && m_mode == RUNNING_MODE && command.Type != CommandType.SyncTurnDone)
                AddOutputCommand(command);
        }
    }
}