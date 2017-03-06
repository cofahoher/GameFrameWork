using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class TestRenderWorld : IDestruct
    {
        TestCombatClient m_combat_client;
        TestLogicWorld m_logic_world;
        RandomGenerator m_random_generator = new RandomGenerator();
        bool m_started = false;
        int m_time = 0;
        int m_interval = 50;

        public TestRenderWorld(TestCombatClient combat_client, TestLogicWorld logic_world)
        {
            m_combat_client = combat_client;
            m_logic_world = logic_world;

            System.Random ran = new System.Random();
            m_random_generator.ResetSeed(ran.Next());
        }

        public void Destruct()
        {
            m_combat_client = null;
            m_logic_world = null;
        }

        public void LoadScene()
        {
            m_combat_client.OnSceneLoaded();
        }

        public void OnGameStart()
        {
            m_started = true;
        }

        public void OnUpdate(int delta_ms, int current_time)
        {
            if (!m_started)
                return;
            m_time += delta_ms;
            if (m_time < m_interval)
                return;
            m_time = 0;
            GenerateRandomCommand();
        }

        void GenerateRandomCommand()
        {
            RandomTestCommand command = new RandomTestCommand();
            command.Random = m_random_generator.Rand();
            m_combat_client.GetSyncClient().PushLocalCommand(command);
        }
    }
}