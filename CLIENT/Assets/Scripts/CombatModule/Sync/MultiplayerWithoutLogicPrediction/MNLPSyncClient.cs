using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class MNLPSyncClient : SyncClient
    {
        long m_local_player_pstid = 0;
        int m_latency = 0;
        int m_start_time = 0;
        int m_current_turn = -1;

        public MNLPSyncClient()
        {
        }

        public override void Init(ILogicWorld logic_world, IOutsideWorld outside_world)
        {
            m_logic_world = logic_world;
            m_outside_world = outside_world;
            m_command_synchronizer = new MNLPClientCommandSynchronizer();
            m_world_syhchronizer = new MNLPPlayerWorldSynchronizer(logic_world, m_command_synchronizer);
        }

        public override void AddPlayer(long player_pstid)
        {
        }

        public override void RemovePlayer(long player_pstid)
        {
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
        }

        public override void Stop()
        {
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
                    m_outside_world.OnGameStart();
                }
            }
            bool forward = m_world_syhchronizer.ForwardFrame(current_time);
            if (!forward)
                return;
            int synchronized_turn = m_world_syhchronizer.GetSynchronizedTurn();
            m_current_turn = synchronized_turn + 1;
        }

        public override void PushLocalCommand(Command command)
        {
            command.PlayerPstid = m_local_player_pstid;
            command.SyncTurn = m_current_turn;
            AddOutputCommand(command);
        }

        public override void PushServerCommand(Command command)
        {
            m_command_synchronizer.AddCommand(command);
        }
    }
}