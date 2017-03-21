using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum CombatClientState
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
        ICombatFactory m_combat_factory;
        long m_local_player_pstid = -1;

        CombatClientState m_state = CombatClientState.None;
        int m_state_start_time = -1;
        int m_last_update_time = -1;
        int m_waiting_cnt = 0;

        LogicWorld m_logic_world;
        RenderWorld m_render_world;
        ISyncClient m_sync_client;

        public CombatClient(ICombatFactory combat_factory)
        {
            m_combat_factory = combat_factory;
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
        public CombatClientState CurrentState
        {
            get { return m_state; }
        }
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
        public long LocalPlayerPstid
        {
            get { return m_local_player_pstid; }
        }
        #endregion

        #region 和局外的接口
        public void Initializa(long local_player_pstid, CombatStartInfo combat_start_info)
        {
            m_local_player_pstid = local_player_pstid;
            m_state = CombatClientState.Loading;
            m_state_start_time = -1;
            m_last_update_time = -1;
            m_waiting_cnt = 0;

            AttributeSystem.Instance.InitializeAllDefinition(m_combat_factory.GetConfigProvider());
            m_combat_factory.RegisterComponents();
            m_combat_factory.RegisterCommands();
            m_combat_factory.RegisterRenderMessages();

            m_logic_world = m_combat_factory.CreateLogicWorld();
            m_logic_world.Initialize(this, true);
            m_render_world = m_combat_factory.CreateRenderWorld();
            m_render_world.Initialize(this, m_logic_world);
            m_sync_client = m_combat_factory.CreateSyncClient();
            m_sync_client.Init(m_logic_world);

            WorldCreationContext world_context = m_combat_factory.CreateWorldCreationContext(combat_start_info);
            m_logic_world.BuildLogicWorld(world_context);
            ++m_waiting_cnt;
            LevelData level_data = GetConfigProvider().GetLevelData(combat_start_info.m_level_id);
            m_render_world.LoadScene(level_data.m_scene_name);
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
        #endregion

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
            m_state_start_time = -1;
            m_last_update_time = -1;
        }
        #endregion

        #region IOutsideWorld
        public IConfigProvider GetConfigProvider()
        {
            return m_combat_factory.GetConfigProvider();
        }

        public int GetCurrentTime()
        {
            float float_time = UnityEngine.Time.unscaledTime;
            return (int)(float_time * 1000);
        }

        public void OnGameStart()
        {
            m_render_world.OnGameStart();
        }

        public void OnGameOver(GameResult game_result)
        {
            m_state = CombatClientState.GameOver;
        }
        #endregion

        #region UPDATE
        public void OnUpdate(int current_time_int)
        {
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
            //ZZWTODO 通知服务器
            m_state = CombatClientState.WaitingForStart;
            m_state_start_time = current_time_int;
            m_last_update_time = current_time_int;
            m_render_world.OnUpdate(0, 0);
        }

        void OnUpdateWaitingForStart(int current_time_int)
        {
            m_last_update_time = current_time_int;
            //ZZWTODO 等待服务器或者自己直接开始
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
                //ZZWTODO 这里发送或者局外自己取
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
                //ZZWTODO 这里发送或者局外自己取
                m_sync_client.ClearOutputCommand();
            }
            m_state = CombatClientState.Ending;
            //ZZWTODO 通知局外
        }

        void OnUpdateEnding(int current_time_int)
        {
        }
        #endregion
    }
}