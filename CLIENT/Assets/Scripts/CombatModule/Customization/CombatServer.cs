using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum CombatServerState
    {
        NotRunning = 0,
        Running,
    }
    public class CombatServer : IOutsideWorld
    {
        protected ICombatFactory m_combat_factory;
        protected CombatServerState m_state = CombatServerState.NotRunning;
        protected int m_start_time = -1;
        protected int m_last_update_time = -1;

        protected LogicWorld m_logic_world;
        protected ISyncServer m_sync_server;

        protected GameResult m_game_result;

        public CombatServer(ICombatFactory combat_factory)
        {
            m_combat_factory = combat_factory;
        }

        public virtual void Destruct()
        {
            m_combat_factory = null;
            m_sync_server.Destruct();
            m_sync_server = null;
            m_logic_world.Destruct();
            m_logic_world = null;
        }

        #region GETTER
        public LogicWorld GetLogicWorld()
        {
            return m_logic_world;
        }
        public ISyncServer GetSyncServer()
        {
            return m_sync_server;
        }
        #endregion

        #region 和局外的接口
        public virtual void Initializa(CombatStartInfo combat_start_info)
        {
            AttributeSystem.Instance.InitializeAllDefinition(m_combat_factory.GetConfigProvider());
            m_logic_world = m_combat_factory.CreateLogicWorld();
            m_logic_world.Initialize(this, false);
            m_sync_server = m_combat_factory.CreateSyncServer();
            m_sync_server.Init(m_logic_world);
            WorldCreationContext world_context = m_combat_factory.CreateWorldCreationContext(combat_start_info);
            m_logic_world.BuildLogicWorld(world_context);
        }

        public virtual void AddPlayer(long player_pstid)
        {
            m_sync_server.AddPlayer(player_pstid);
        }

        public virtual void StartCombat(int current_time_int)
        {
            m_state = CombatServerState.Running;
            m_start_time = current_time_int;
            m_last_update_time = 0;
            m_sync_server.Start(0, 0);
        }
        #endregion

        #region IOutsideWorld
        public IConfigProvider GetConfigProvider()
        {
            return m_combat_factory.GetConfigProvider();
        }

        public virtual int GetCurrentTime()
        {
            return m_last_update_time;
        }

        public virtual void OnGameStart()
        {
        }

        public virtual void OnGameOver(GameResult game_result)
        {
            m_game_result = game_result;
            ProcessGameOver();
        }

        public long GetLocalPlayerPstid()
        {
            return 0L;
        }
        #endregion

        public void OnUpdate(int current_time_int)
        {
            if (m_state != CombatServerState.Running)
                return;
            int current_time = current_time_int - m_start_time;
            int delta_ms = current_time - m_last_update_time;
            if (delta_ms < 0)
                return;
            m_sync_server.Update(current_time);
            List<Command> commands = m_sync_server.GetOutputCommands();
            if (commands.Count > 0)
            {
                SendCommands(commands);
                m_sync_server.ClearOutputCommand();
            }
            m_last_update_time = current_time;
        }

        #region 继承类自己实现
        protected virtual void SendCommands(List<Command> commands)
        {
        }

        protected virtual void ProcessGameOver()
        {
        }
        #endregion
    }
}