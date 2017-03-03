using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    enum CombatClientState
    {
        None = 0,
        Loading = 1,
        Loaded,
        WaitingForStart,
        Running,
        GameOver,
        Ending,
    }

    public class CombatClient : IOutsideWorld
    {
        long m_local_player_pstid = -1;

        bool m_is_dropout = false;
        int m_end_frame = -1;
        long m_winner_player_pstid = 0;

        CombatClientState m_state = CombatClientState.None;
        int m_state_start_time = -1;
        int m_last_update_time = -1;
        int m_waiting_cnt = 0;

        LogicWorld m_logic_world;
        RenderWorld m_render_world;
        ISyncClient m_sync_client;

        public CombatClient()
        {
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
        public LogicWorld GetLogicWorld()
        {
            return m_logic_world;
        }
        public RenderWorld GetRenderWorld()
        {
            return m_render_world;
        }
        public ISyncClient GetSyncClient()
        {
            return m_sync_client;
        }
        #endregion

        public void Initializa(long local_player_pstid, CombatStartInfo combat_start_info)
        {
            m_local_player_pstid = local_player_pstid;
            m_state = CombatClientState.Loading;
            m_waiting_cnt = 0;

            AttributeSystem.Instance.InitializeAllDefinition();
            m_logic_world = new LogicWorld(this, true);
            m_render_world = new RenderWorld(this, m_logic_world);
            m_sync_client = new SPSyncClient();
            m_sync_client.Init(m_logic_world, this);

            WorldCreationContext world_context = WorldCreationContext.CreateWorldCreationContext(combat_start_info);
            m_logic_world.BuildLogicWorld(world_context);
            ++m_waiting_cnt;
            m_render_world.LoadScene();
        }

        public void AddPlayer(long player_pstid)
        {
            m_sync_client.AddPlayer(player_pstid);
        }

        public void StartCombat(int current_time_int)
        {
            m_state = CombatClientState.Running;
            m_state_start_time = current_time_int;
            m_last_update_time = 0;
            m_sync_client.Start(0, m_local_player_pstid, 200);
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
            m_state = CombatClientState.Loaded;
        }
        #endregion

        public int GetCurrentTime()
        {
            float float_time = UnityEngine.Time.unscaledTime;
            return (int)(float_time * 1000);
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
            m_state = CombatClientState.GameOver;
        }

        #region UPDATE
        public void OnUpdate(float current_time_float)
        {
            int current_time_int = (int)(current_time_float * 1000);
            switch (m_state)
            {
            case CombatClientState.None:
                OnUpdateNone(current_time_int);
                break;
            case CombatClientState.Loading:
                OnUpdateLoading(current_time_int);
                break;
            case CombatClientState.Loaded:
                OnUpdateLoaded(current_time_int);
                break;
            case CombatClientState.WaitingForStart:
                OnUpdateWaitingForStart(current_time_int);
                break;
            case CombatClientState.Running:
                OnUpdateRunning(current_time_int);
                break;
            case CombatClientState.GameOver:
                OnUpdateGameOver(current_time_int);
                break;
            case CombatClientState.Ending:
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
            m_state = CombatClientState.WaitingForStart;
            m_state_start_time = current_time_int;
            m_last_update_time = current_time_int;
        }

        void OnUpdateWaitingForStart(int current_time_int)
        {
            int delta_ms = current_time_int - m_last_update_time;
            m_render_world.OnUpdate(delta_ms, current_time_int);
            m_last_update_time = current_time_int;
            //ZZWTODO 等待
            StartCombat(current_time_int);
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
                //ZZWTODO 发送command
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
                //ZZWTODO 发送command
                m_sync_client.ClearOutputCommand();
            }
            m_state = CombatClientState.Ending;
            //ZZWTODO 发送游戏结束
        }

        void OnUpdateEnding(int current_time_int)
        {
        }
        #endregion
    }
}