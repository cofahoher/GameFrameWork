using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum NetworkMessageType
    {
        Invalid = 0,
        StartLoading,
        LoadingComplete,
        StartGame,
        SyncCommands,
        GameOver,
    }

    public class NetworkMessage
    {
        protected NetworkMessageType m_type = NetworkMessageType.Invalid;
        protected long m_player_pstid = -1L;
        public NetworkMessageType Type
        {
            get { return m_type; }
        }
        public long PlayerPstid
        {
            get { return m_player_pstid; }
            set { m_player_pstid = value; }
        }
    }

    public class NetworkMessages_StartLoading : NetworkMessage
    {
        public List<long> m_player_pstids = new List<long>();
        public NetworkMessages_StartLoading()
        {
            m_type = NetworkMessageType.StartLoading;
        }
        public void AddPlayer(long player_pstid)
        {
            m_player_pstids.Add(player_pstid);
        }
    }

    public class NetworkMessages_LoadingComplete : NetworkMessage
    {
        public NetworkMessages_LoadingComplete()
        {
            m_type = NetworkMessageType.LoadingComplete;
        }
    }

    public class NetworkMessages_StartGame : NetworkMessage
    {
        public int m_latency = 0;
        public NetworkMessages_StartGame()
        {
            m_type = NetworkMessageType.StartGame;
        }
    }

    public class NetworkMessages_SyncCommands : NetworkMessage
    {
        public List<Command> m_commands = new List<Command>();
        public NetworkMessages_SyncCommands()
        {
            m_type = NetworkMessageType.SyncCommands;
        }
        public void AddCommand(Command command)
        {
            m_commands.Add(command);
        }
    }

    public class NetworkMessages_GameOver : NetworkMessage
    {
        public uint m_crc = 0;
        public NetworkMessages_GameOver()
        {
            m_type = NetworkMessageType.GameOver;
        }
    }
}