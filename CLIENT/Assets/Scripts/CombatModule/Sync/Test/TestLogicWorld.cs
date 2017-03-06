using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class TestLogicWorld : ILogicWorld
    {
        IOutsideWorld m_outside_world;
        int m_current_time = 0;
        int m_current_frame = 0;
        bool m_game_over = false;

        uint m_game_crc = 0;

        public TestLogicWorld(IOutsideWorld outside_world)
        {
            m_outside_world = outside_world;
        }

        public void Destruct()
        {
            m_outside_world = null;
        }

        public void OnStart()
        {
            m_current_time = 0;
            m_current_frame = 0;
            m_game_over = false;
        }

        public bool OnUpdate(int delta_ms)
        {
            m_current_time += delta_ms;
            ++m_current_frame;
            if (m_current_time >= 60000)
                OnGameOver(false, 0);
            return m_game_over;
        }

        public bool IsGameOver()
        {
            return m_game_over;
        }

        public void HandleCommand(Command command)
        {
            if (command.Type == CommandType.RandomTest)
            {
                CRC.Calculate(m_current_frame, m_game_crc);
                RandomTestCommand rtc = command as RandomTestCommand;
                CRC.Calculate(rtc.PlayerPstid, m_game_crc);
                CRC.Calculate(rtc.SyncTurn, m_game_crc);
                CRC.Calculate(rtc.Random, m_game_crc);
            }
        }

        public void CopyFrom(ILogicWorld parallel_world)
        {
        }

        public void OnGameOver(bool is_dropout, long winner_player_pstid)
        {
            m_game_over = true;
            m_outside_world.OnGameOver(is_dropout, m_current_frame, winner_player_pstid);
            //ZZWTOD 记录下m_game_crc，共比较
        }
    }
}