using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class MNLPSyncClient : SyncClient
    {
        long m_local_player_pstid = 0;
        int m_latency = 0;
        int m_start_time = 0;
        int m_last_update_time = 0;
        int m_current_turn = -1;

        public MNLPSyncClient()
        {
        }

        public override void Init(ILogicWorld logic_world, IOutsideWorld outside_world)
        {
            m_logic_world = logic_world;
            m_outside_world = outside_world;
            //m_world_syhchronizer = new MNLPPlayerWorldSynchronizer(logic_world);
        }

        public override void AddPlayer(long player_pstid)
        {
        }

        public override void RemovePlayer(long player_pstid)
        {
        }

        public override void Start(int current_time, long local_player_pstid, int latency)
        {
        }

        public override void Stop()
        {
        }

        public override void Update(int current_time)
        {
        }

        public override void PushLocalCommand(Command command)
        {
        }

        public override void PushServerCommand(Command command)
        {
        }
    }
}