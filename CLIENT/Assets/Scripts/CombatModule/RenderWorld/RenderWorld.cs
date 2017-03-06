using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class RenderWorld : IDestruct
    {
        CombatClient m_combat_client;
        LogicWorld m_logic_world;
        TaskScheduler m_scheduler;


        public RenderWorld(CombatClient combat_client, LogicWorld logic_world)
        {
            m_combat_client = combat_client;
            m_logic_world = logic_world;
            m_scheduler = new TaskScheduler(this);
        }

        public void Destruct()
        {
        }

        public void LoadScene()
        {
            m_combat_client.OnSceneLoaded();
        }

        public void OnGameStart()
        {
        }

        public void OnUpdate(int delta_ms, int current_time)
        {
        }
    }
}