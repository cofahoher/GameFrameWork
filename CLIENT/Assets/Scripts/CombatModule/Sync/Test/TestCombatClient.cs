using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    enum TestCombatClientState
    {
        None = 0,
        Loading = 1,
        Loaded,
        WaitingForStart,
        Running,
        GameOver,
        Ending,
    }

    public class TestCombatClient : IOutsideWorld
    {
        SyncModelClient m_sync_model;
        long m_local_player_pstid = -1;

        bool m_is_dropout = false;
        int m_end_frame = -1;
        long m_winner_player_pstid = 0;

        TestCombatClientState m_state = TestCombatClientState.None;
        int m_state_start_time = -1;
        int m_last_update_time = -1;
        int m_waiting_cnt = 0;

        TestLogicWorld m_logic_world;
        TestRenderWorld m_render_world;
        ISyncClient m_sync_client;

        public TestCombatClient(SyncModelClient sync_model)
        {
            m_sync_model = sync_model;
        }

        public void Destruct()
        {
            m_sync_client.Destruct();
            m_sync_client = null;
            m_render_world.Destruct();
            m_render_world = null;
            m_logic_world.Destruct();
            m_logic_world = null;
        }

        #region GETTER
        public TestLogicWorld GetLogicWorld()
        {
            return m_logic_world;
        }
        public TestRenderWorld GetRenderWorld()
        {
            return m_render_world;
        }
        public ISyncClient GetSyncClient()
        {
            return m_sync_client;
        }
        #endregion

        public void Initializa(long local_player_pstid)
        {
            m_local_player_pstid = local_player_pstid;
            m_state = TestCombatClientState.Loading;
            m_waiting_cnt = 0;

            m_logic_world = new TestLogicWorld(this);
            m_render_world = new TestRenderWorld(this, m_logic_world);
            m_sync_client = new MNLPSyncClient();
            m_sync_client.Init(m_logic_world, this);

            ++m_waiting_cnt;
            m_render_world.LoadScene();
        }

        public void AddPlayer(long player_pstid)
        {
            m_sync_client.AddPlayer(player_pstid);
        }

        public void StartCombat(int current_time_int, int latency)
        {
            m_state = TestCombatClientState.Running;
            m_state_start_time = current_time_int;
            m_last_update_time = 0;
            m_sync_client.Start(0, m_local_player_pstid, latency);
        }

        #region RESOURCE
        public void OnSceneLoaded()
        {
            CacheResources();
            OnOneResourceCached();
        }

        void CacheResources()
        {
        }

        void OnOneResourceCached()
        {
            --m_waiting_cnt;
            if (m_waiting_cnt == 0)
                OnAllResourceCached();
        }

        void OnAllResourceCached()
        {
            m_render_world.OnUpdate(0, 0);
            m_state = TestCombatClientState.Loaded;
        }
        #endregion

        public int GetCurrentTime()
        {
            return m_sync_model.GetCurrentTime();
        }

        public void OnGameStart()
        {
            m_render_world.OnGameStart();
        }

        public void OnGameOver(bool is_dropout, int end_frame, long winner_player_pstid)
        {
            m_is_dropout = is_dropout;
            m_end_frame = end_frame;
            m_winner_player_pstid = winner_player_pstid;
            m_state = TestCombatClientState.GameOver;
        }

        #region UPDATE
        public void OnUpdate(int current_time_int)
        {
            switch (m_state)
            {
            case TestCombatClientState.None:
                OnUpdateNone(current_time_int);
                break;
            case TestCombatClientState.Loading:
                OnUpdateLoading(current_time_int);
                break;
            case TestCombatClientState.Loaded:
                OnUpdateLoaded(current_time_int);
                break;
            case TestCombatClientState.WaitingForStart:
                OnUpdateWaitingForStart(current_time_int);
                break;
            case TestCombatClientState.Running:
                OnUpdateRunning(current_time_int);
                break;
            case TestCombatClientState.GameOver:
                OnUpdateGameOver(current_time_int);
                break;
            case TestCombatClientState.Ending:
                OnUpdateEnding(current_time_int);
                break;
            default:
                break;
            }
        }

        void OnUpdateNone(int current_time_int)
        {
        }

        void OnUpdateLoading(int current_time_int)
        {
        }

        void OnUpdateLoaded(int current_time_int)
        {
            m_state = TestCombatClientState.WaitingForStart;
            m_state_start_time = current_time_int;
            m_last_update_time = current_time_int;
        }

        void OnUpdateWaitingForStart(int current_time_int)
        {
            int delta_ms = current_time_int - m_last_update_time;
            m_render_world.OnUpdate(delta_ms, current_time_int);
            m_last_update_time = current_time_int;
            m_sync_model.OnLoadingComplete();
        }

        void OnUpdateRunning(int current_time_int)
        {
            current_time_int -= m_state_start_time;
            int delta_ms = current_time_int - m_last_update_time;
            if (delta_ms < 0)
                return;
            m_sync_client.Update(current_time_int);
            List<Command> commands = m_sync_client.GetOutputCommands();
            if (commands.Count > 0)
            {
                m_sync_model.SendSyncCommands(commands);
                m_sync_client.ClearOutputCommand();
            }
            m_render_world.OnUpdate(delta_ms, current_time_int);
            m_last_update_time = current_time_int;
        }

        void OnUpdateGameOver(int current_time_int)
        {
            m_sync_client.Stop();
            List<Command> commands = m_sync_client.GetOutputCommands();
            if (commands.Count > 0)
            {
                m_sync_model.SendSyncCommands(commands);
                m_sync_client.ClearOutputCommand();
            }
            m_state = TestCombatClientState.Ending;
            m_sync_model.OnGameOver();
        }

        void OnUpdateEnding(int current_time_int)
        {
        }
        #endregion
    }
}