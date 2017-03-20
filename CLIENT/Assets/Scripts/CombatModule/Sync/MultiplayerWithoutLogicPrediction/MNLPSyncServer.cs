using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class MNLPSyncServer : SyncServer
    {
        Dictionary<long, PlayerSyncData> m_sync_data_of_players = new Dictionary<long, PlayerSyncData>();
        int m_latency = 0;
        int m_start_time = 0;
        int m_last_update_time = 0;
        int m_current_turn = -1;

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

        public override void Start(int current_time, int latency)
        {
            m_latency = latency;
            m_start_time = current_time + latency;
            m_current_turn = -1;
        }

        public override void Update(int current_time)
        {
            if (m_current_turn == -1)
            {
                if (current_time < m_start_time)
                {
                    return;
                }
                else
                {
                    OnTurnEnd(0);
                    m_current_turn = 0;
                    m_world_syhchronizer.Start(m_start_time);
                    m_last_update_time = m_start_time;
                }
            }
            bool forward = m_world_syhchronizer.ForwardFrame(current_time);
            m_last_update_time = current_time;
            if (!forward)
                return;
            int synchronized_turn = m_world_syhchronizer.GetSynchronizedTurn();
            int forward_turn_count = synchronized_turn + 1 - m_current_turn;
            if (forward_turn_count <= 0)
                return;
            for (int i = m_current_turn + 1; i <= synchronized_turn + 1; ++i)
                OnTurnEnd(i);
            m_current_turn = synchronized_turn + 1;
            SyncTurnDoneCommand command = new SyncTurnDoneCommand();
            command.PlayerPstid = -1L;
            command.SyncTurn = synchronized_turn + 1;
            AddOutputCommand(command);
        }

        void OnTurnEnd(int turn_index)
        {
            List<Command> commands = m_command_synchronizer.GetCommands(turn_index);
            if (commands != null)
            {
                for (int j = 0; j < commands.Count; ++j)
                    AddOutputCommand(commands[j]);
            }
        }

        public override void PushClientCommand(Command command)
        {
            long player_pstid = command.PlayerPstid;
            PlayerSyncData psd;
            if (!m_sync_data_of_players.TryGetValue(player_pstid, out psd))
                return;
            int adjust_turn = m_current_turn + 1;
            int delta_turn = adjust_turn - command.SyncTurn - 1;
            if (delta_turn < 0 || delta_turn > SyncParam.MNLP_MAX_COMMAND_DELAY_SYNCTURN)
                return;
            command.SyncTurn = adjust_turn;
            m_command_synchronizer.AddCommand(command);
        }
    }
}