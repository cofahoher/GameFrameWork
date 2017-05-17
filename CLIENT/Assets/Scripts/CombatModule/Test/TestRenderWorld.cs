using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class TestRenderWorld : IDestruct
    {
        TestCombatClient m_combat_client;
        TestLogicWorld m_logic_world;
        RandomGeneratorI m_random_generator = new RandomGeneratorI();
        bool m_started = false;
        int m_time = 0;
        int m_interval = 50;

        public TestRenderWorld(TestCombatClient combat_client, TestLogicWorld logic_world)
        {
            m_combat_client = combat_client;
            m_logic_world = logic_world;
            m_random_generator.ResetSeed((int)(m_combat_client.LocalPlayerPstid));
            RandomInterval();
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
            //while (m_time > m_interval)
            //{
            //    m_time -= m_interval;
            //    GenerateRandomCommand();
            //    RandomInterval();
            //}
            m_time = 0;
            GenerateRandomCommand();
            RandomInterval();
        }

        void RandomInterval()
        {
            m_interval = m_random_generator.RandBetween(10, 70);
        }

        void GenerateRandomCommand()
        {
            RandomTestCommand command = new RandomTestCommand();
            command.m_random = m_random_generator.Rand();
            m_combat_client.GetSyncClient().PushLocalCommand(command);
        }
    }
}