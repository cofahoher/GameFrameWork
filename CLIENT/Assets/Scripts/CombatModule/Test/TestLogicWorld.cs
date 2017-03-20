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

        int m_command_count = 0;
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
            m_outside_world.OnGameStart();
        }

        public bool OnUpdate(int delta_ms)
        {
            m_current_time += delta_ms;
            ++m_current_frame;
            if (m_current_time >= 10000)
            {
                GameResult game_result = new GameResult();
                OnGameOver(game_result);
            }
            return m_game_over;
        }

        public bool IsGameOver()
        {
            return m_game_over;
        }

        public void HandleCommand(Command command)
        {
            if (m_game_over)
                return;
            if (command.Type == CommandType.RandomTest)
            {
                ++m_command_count;
                m_game_crc = CRC.Calculate(m_current_frame, m_game_crc);
                RandomTestCommand rtc = command as RandomTestCommand;
                m_game_crc = CRC.Calculate(rtc.PlayerPstid, m_game_crc);
                m_game_crc = CRC.Calculate(rtc.SyncTurn, m_game_crc);
                m_game_crc = CRC.Calculate(rtc.m_random, m_game_crc);
                /*
                if (m_client)
                    UnityEngine.Debug.LogError("Client HandleCommand, " + m_command_count + ", m_current_frame = " + m_current_frame + ", SyncTurn = " + rtc.SyncTurn + ", Random = " + rtc.Random + ", PlayerPstid = " + rtc.PlayerPstid + ", CRC = " + m_game_crc);
                else
                    UnityEngine.Debug.LogError("Server HandleCommand, " + m_command_count + ", m_current_frame = " + m_current_frame + ", SyncTurn = " + rtc.SyncTurn + ", Random = " + rtc.Random + ", PlayerPstid = " + rtc.PlayerPstid + ", CRC = " + m_game_crc);
                */
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

        public void OnGameOver(GameResult game_result)
        {
            if (m_game_over)
                return;
            m_game_over = true;
            game_result.m_end_frame = m_current_frame;
            m_game_crc = CRC.Calculate(m_current_frame, m_game_crc);
            /*
            if (m_client)
                UnityEngine.Debug.LogError("Client OnGameOver, m_current_frame = " + m_current_frame + ", CRC = " + m_game_crc + ", command_count = " + m_command_count);
            else
                UnityEngine.Debug.LogError("Server OnGameOver, m_current_frame = " + m_current_frame + ", CRC = " + m_game_crc + ", command_count = " + m_command_count);
             */
            m_outside_world.OnGameOver(game_result);
        }
    }
}