using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class TestLogicWorld : ILogicWorld
    {
        IOutsideWorld m_outside_world;
        bool m_client = false;
        int m_current_time = 0;
        int m_current_frame = 0;
        bool m_game_over = false;

        uint m_game_crc = 0;

        public TestLogicWorld(IOutsideWorld outside_world, bool client)
        {
            m_outside_world = outside_world;
            m_client = client;
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
            if (m_current_time >= 10000)
                OnGameOver(false, 0);
            return m_game_over;
        }

        public bool IsGameOver()
        {
            return m_game_over;
        }

        int m_index = 0;
        public void HandleCommand(Command command)
        {
            if (command.Type == CommandType.RandomTest)
            {
                m_game_crc = CRC.Calculate(m_current_frame, m_game_crc);
                RandomTestCommand rtc = command as RandomTestCommand;
                m_game_crc = CRC.Calculate(rtc.PlayerPstid, m_game_crc);
                m_game_crc = CRC.Calculate(rtc.SyncTurn, m_game_crc);
                m_game_crc = CRC.Calculate(rtc.Random, m_game_crc);
                UnityEngine.Debug.LogError("HandleCommand, " + m_index + ", m_current_frame = " + m_current_frame + ", SyncTurn = " + rtc.SyncTurn + ", Random = " + rtc.Random + ", PlayerPstid = " + rtc.PlayerPstid);
                m_index += 1;
            }
        }

        public void CopyFrom(ILogicWorld parallel_world)
        {
        }

        public int GetCurrentFrame()
        {
            return m_current_frame;
        }

        public uint GetCRC()
        {
            return m_game_crc;
        }

        public void OnGameOver(bool is_dropout, long winner_player_pstid)
        {
            if (m_game_over)
                return;
            m_game_over = true;
            m_outside_world.OnGameOver(is_dropout, m_current_frame, winner_player_pstid);
            m_game_crc = CRC.Calculate(m_current_frame, m_game_crc);
        }
    }
}