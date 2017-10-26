using System;
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
        protected DateTime m_dt_original = DateTime.Now;
        protected ICombatFactory m_combat_factory;
        protected long m_local_player_pstid = -1;
        protected LevelData m_level_data = null;

        protected CombatClientState m_state = CombatClientState.None;
        protected int m_state_frame_cnt = 0;
        protected int m_state_start_time = -1;
        protected int m_last_update_time = -1;
        protected int m_waiting_cnt = 0;
#if UNITY_EDITOR
        protected bool m_is_first_frame = true;
#endif

        protected LogicWorld m_logic_world;
        protected RenderWorld m_render_world;
        protected ISyncClient m_sync_client;

        protected GameResult m_game_result;

        public CombatClient(ICombatFactory combat_factory)
        {
            m_combat_factory = combat_factory;
        }

        public virtual void Destruct()
        {
            m_combat_factory = null;
            m_level_data = null;
            m_sync_client.Destruct();
            m_sync_client = null;
            m_render_world.Destruct();
            m_render_world = null;
            m_logic_world.Destruct();
            m_logic_world = null;
        }

        #region GETTER
        public ICombatFactory GetCombatFactory()
        {
            return m_combat_factory;
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
        public CombatClientState CurrentState
        {
            get { return m_state; }
        }
        public long LocalPlayerPstid
        {
            get { return m_local_player_pstid; }
        }
        #endregion

        #region 和局外的接口
        public virtual void Initializa(long local_player_pstid, CombatStartInfo combat_start_info)
        {
            //真正的local_player_pstid在局外，传进来就好；combat_start_info是给所有玩家、观战者、以及录像回放时，都一致的消息
            m_local_player_pstid = local_player_pstid;
            m_level_data = GetConfigProvider().GetLevelData(combat_start_info.m_level_id);
            m_state = CombatClientState.Loading;
            m_state_frame_cnt = 0;
            m_state_start_time = -1;
            m_last_update_time = -1;
            m_waiting_cnt = 0;
#if UNITY_EDITOR
            m_is_first_frame = true;
#endif

            AttributeSystem.Instance.InitializeAllDefinition(m_combat_factory.GetConfigProvider());
            ComponentTypeRegistry.RegisterDefaultComponents();
            BehaviorTreeNodeTypeRegistry.RegisterDefaultNodes();
            DamageModifier.RegisterDefaultModifiers();
            m_combat_factory.RegisterCommands();

            BehaviorTreeFactory.Instance.SetConfigProvider(m_combat_factory.GetConfigProvider());

            m_logic_world = m_combat_factory.CreateLogicWorld();
            m_logic_world.Initialize(this, combat_start_info.m_world_seed, true);
            m_render_world = m_combat_factory.CreateRenderWorld();
            m_render_world.Initialize(this, m_logic_world);
            m_logic_world.SetIRenderWorld(m_render_world);
            m_sync_client = m_combat_factory.CreateSyncClient();
            m_sync_client.Init(m_logic_world);

            BuildLogicWorld(combat_start_info);
            BuildRenderWorld(m_level_data);
        }

        protected virtual void BuildLogicWorld(CombatStartInfo combat_start_info)
        {
            WorldCreationContext world_context = m_combat_factory.CreateWorldCreationContext(combat_start_info);
            m_logic_world.BuildLogicWorld(world_context);
        }

        protected virtual void BuildRenderWorld(LevelData level_data)
        {
            ++m_waiting_cnt;
            m_render_world.BuildRenderWorld(level_data);
        }

        public virtual void AddPlayer(long player_pstid)
        {
            m_sync_client.AddPlayer(player_pstid);
        }

        public virtual void StartCombat(int current_time_int)
        {
            m_state = CombatClientState.Running;
            m_state_frame_cnt = 0;
            m_state_start_time = current_time_int;
            m_last_update_time = 0;
            m_sync_client.Start(0, m_local_player_pstid, 200);
            if (Statistics.Instance != null)
                Statistics.Instance.Enabled = true;
        }
        #endregion

        #region RESOURCE，这里只是YY，具体的资源缓存策略具体处理
        public void OnRenderWorldBuilt()
        {
            CacheResources();
            OnOneResourceCached();
        }

        protected virtual void CacheResources()
        {
        }

        void OnOneResourceCached()
        {
            --m_waiting_cnt;
            if (m_waiting_cnt == 0)
                OnAllResourceCached();
        }

        protected virtual void OnAllResourceCached()
        {
            m_render_world.OnUpdate(0, 0);
            m_state = CombatClientState.Loaded;
            m_state_frame_cnt = 0;
            m_state_start_time = -1;
            m_last_update_time = -1;
        }
        #endregion

        #region IOutsideWorld
        public IConfigProvider GetConfigProvider()
        {
            return m_combat_factory.GetConfigProvider();
        }

        public LevelData GetLevelData()
        {
            return m_level_data;
        }

        public virtual int GetCurrentTime()
        {
#if CONSOLE_CLIENT
            TimeSpan ts = DateTime.Now - m_dt_original;
            return (int)(ts.TotalMilliseconds);
#else
            float float_time = UnityEngine.Time.unscaledTime;
            return (int)(float_time * 1000);
#endif
        }

        public virtual void OnGameStart()
        {
            m_render_world.OnGameStart();
        }

        public virtual void OnGameOver(GameResult game_result)
        {
            m_state = CombatClientState.GameOver;
            m_game_result = game_result;
        }

        public long GetLocalPlayerPstid()
        {
            return m_local_player_pstid;
        }

        public virtual void OnDisconnected()
        {

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
            ++m_state_frame_cnt;
        }

        protected void OnUpdateNone(int current_time_int)
        {
        }

        protected virtual void OnUpdateLoading(int current_time_int)
        {
        }

        protected virtual void OnUpdateLoaded(int current_time_int)
        {
            if (m_state_frame_cnt == 0)
            {
                m_render_world.OnUpdate(0, 0);
            }
            else if (m_state_frame_cnt == 1)
            {
                m_state = CombatClientState.WaitingForStart;
                m_state_frame_cnt = 0;
                m_state_start_time = current_time_int;
                m_last_update_time = current_time_int;
                ProcessReadyForStart();
            }
        }

        protected void OnUpdateWaitingForStart(int current_time_int)
        {
            m_last_update_time = current_time_int;
        }

        protected void OnUpdateRunning(int current_time_int)
        {
            current_time_int -= m_state_start_time;
            int delta_ms = current_time_int - m_last_update_time;
            if (delta_ms < 0)
                return;
#if UNITY_EDITOR
            if (m_is_first_frame)
            {
                m_is_first_frame = false;
                LogWrapper.LogInfo("CombatClient.OnUpdateRunning, first delta_ms = ", delta_ms);
            }
            if (delta_ms > 100)
                LogWrapper.LogInfo("CombatClient.OnUpdateRunning, delta_ms = ", delta_ms);
#endif
            m_sync_client.Update(current_time_int);
            m_render_world.OnUpdate(delta_ms, current_time_int);
            List<Command> commands = m_sync_client.GetOutputCommands();
            if (commands.Count > 0)
            {
                SendCommands(commands);
                m_sync_client.ClearOutputCommand();
            }
            m_last_update_time = current_time_int;
        }

        protected void OnUpdateGameOver(int current_time_int)
        {
            m_sync_client.Stop();
            List<Command> commands = m_sync_client.GetOutputCommands();
            if (commands.Count > 0)
            {
                SendCommands(commands);
                m_sync_client.ClearOutputCommand();
            }
            m_state = CombatClientState.Ending;
            ProcessGameOver();
            if (Statistics.Instance != null)
                Statistics.Instance.Enabled = false;
        }

        protected virtual void OnUpdateEnding(int current_time_int)
        {
        }
        #endregion

        #region 继承类自己实现
        protected virtual void ProcessReadyForStart()
        {
            StartCombat(GetCurrentTime());
        }

        protected virtual void SendCommands(List<Command> commands)
        {
        }

        protected virtual void ProcessGameOver()
        {
        }
        #endregion
    }
}