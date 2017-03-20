using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    enum CombatServerState
    {
        NotRunning = 0,
        Running,
    }
    public class CombatServer : IOutsideWorld
    {
        ICombatFactory m_combat_factory;
        CombatServerState m_state = CombatServerState.NotRunning;
        int m_start_time = -1;
        int m_last_update_time = -1;

        LogicWorld m_logic_world;
        ISyncServer m_sync_server;

        public CombatServer(ICombatFactory combat_factory)
        {
            m_combat_factory = combat_factory;
        }

        public void Destruct()
        {
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
        public void Initializa(CombatStartInfo combat_start_info)
        {
            AttributeSystem.Instance.InitializeAllDefinition();
            m_logic_world = m_combat_factory.CreateLogicWorld();
            m_logic_world.Initialize(this, false);
            m_sync_server = m_combat_factory.CreateSyncServer();
            m_sync_server.Init(m_logic_world);
            WorldCreationContext world_context = m_combat_factory.CreateWorldCreationContext(combat_start_info);
            m_logic_world.BuildLogicWorld(world_context);
        }
        
        public void AddPlayer(long player_pstid)
        {
            m_sync_server.AddPlayer(player_pstid);
        }

        public void StartCombat(int current_time_int)
        {
            m_state = CombatServerState.Running;
            m_start_time = current_time_int;
            m_last_update_time = 0;
            m_sync_server.Start(0, 0);
        }
        #endregion

        #region IOutsideWorld
        public int GetCurrentTime()
        {
            return m_last_update_time;
        }

        public void OnGameStart()
        {
            //NOUSE
        }

        public void OnGameOver(GameResult game_result)
        {
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
            m_last_update_time = current_time;
        }
    }
}