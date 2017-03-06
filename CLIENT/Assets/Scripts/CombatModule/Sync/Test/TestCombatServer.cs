using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class TestCombatServer : IOutsideWorld
    {        
        int m_start_time = -1;
        int m_last_update_time = -1;
        TestLogicWorld m_logic_world;
        ISyncServer m_sync_server;

        public TestCombatServer()
        {
        }

        public void Destruct()
        {
            m_sync_server.Destruct();
            m_sync_server = null;
            m_logic_world.Destruct();
            m_logic_world = null;
        }

        #region GETTER
        public TestLogicWorld GetLogicWorld()
        {
            return m_logic_world;
        }
        public ISyncServer GetSyncServer()
        {
            return m_sync_server;
        }
        #endregion

        public void Initializa()
        {
            m_logic_world = new TestLogicWorld(this);
            m_sync_server = new MNLPSyncServer();
            m_sync_server.Init(m_logic_world);
        }
        
        public void AddPlayer(long player_pstid)
        {
            m_sync_server.AddPlayer(player_pstid);
        }

        public void StartCombat(int current_time_int, int latency)
        {
            m_start_time = current_time_int;
            m_last_update_time = 0;
            m_sync_server.Start(0, latency);
        }

        public int GetCurrentTime()
        {
            return m_last_update_time;
        }

        public void OnGameStart()
        {
            //NOUSE
        }

        public void OnGameOver(bool is_dropout, int end_frame, long winner_player_pstid)
        {
        }

        public void OnUpdate(int current_time_int)
        {
            int current_time = current_time_int - m_start_time;
            int delta_ms = current_time - m_last_update_time;
            if (delta_ms < 0)
                return;
            m_sync_server.Update(current_time);
            m_last_update_time = current_time;
        }
    }
}