using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class SPSyncClient : SyncClient
    {
        long m_local_player_pstid = 0;
        int m_latency = 0;
        int m_start_time = 0;
        int m_last_update_time = 0;
        int m_current_turn = -1;
        int m_stored_turndone_count = 0;
        bool m_send_turndone = false;

        public bool SendTurnDome
        {
            set { m_send_turndone = value; }
        }
        public long LocalPlayerPstid
        {
            get { return m_local_player_pstid; }
        }

        public SPSyncClient()
        {
        }

        public override void Init(ILogicWorld logic_world)
        {
            m_logic_world = logic_world;
            m_command_synchronizer = new SPPredictedCommandSynchronizer();
            m_world_syhchronizer = new SPPlayerWorldSynchronizer(logic_world, m_command_synchronizer);
        }

        public override void AddPlayer(long player_pstid)
        {
            m_command_synchronizer.AddPlayer(player_pstid);
        }

        public override void RemovePlayer(long player_pstid)
        {
            m_command_synchronizer.RemovePlayer(player_pstid);
        }

        public override void Start(int current_time, long local_player_pstid, int latency)
        {
            m_local_player_pstid = local_player_pstid;
            m_latency = latency;
            int delay = SyncParam.MAX_LATENCY - latency;
            if (delay < 0)
                delay = 0;
            m_start_time = current_time + delay;
            m_current_turn = -1;
            m_stored_turndone_count = 0;
        }

        public override void Stop()
        {
            int synchronized_turn = m_world_syhchronizer.GetSynchronizedTurn();
            if (SyncParam.FRAME_COUNT_PER_SYNCTURN > 1)
                synchronized_turn += 1;
            SyncTurnDoneCommand command = new SyncTurnDoneCommand();
            command.PlayerPstid = m_local_player_pstid;
            command.SyncTurn = synchronized_turn;
            AddOutputCommand(command);
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
            m_stored_turndone_count += forward_turn_count;
            m_current_turn = synchronized_turn + 1;
            if (m_stored_turndone_count >= SyncParam.SP_SYNC_INTERVAL_TURNCOUNT)
            {
                if (m_send_turndone)
                {
                    SyncTurnDoneCommand command = new SyncTurnDoneCommand();
                    command.PlayerPstid = m_local_player_pstid;
                    command.SyncTurn = synchronized_turn;
                    AddOutputCommand(command);
                }
                m_stored_turndone_count = 0;
            }
        }

        public override void PushLocalCommand(Command command)
        {
            command.PlayerPstid = m_local_player_pstid;
            command.SyncTurn = m_current_turn + 8; //ZZWTODO 模拟延迟
            if (m_command_synchronizer.AddCommand(command))
                AddOutputCommand(command);
        }

        public override void PushServerCommand(Command command)
        {
        }
        public override void ClearOutputCommand()
        {
            m_output_commands.Clear();
        }
    }
}