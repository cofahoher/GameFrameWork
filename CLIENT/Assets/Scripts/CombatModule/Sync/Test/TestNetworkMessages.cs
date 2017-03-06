using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class NetworkMessages_StartLoading
    {
        public List<long> m_player_pstids = new List<long>();
        public void AddPlayer(long player_pstid)
        {
            m_player_pstids.Add(player_pstid);
        }
    }

    public class NetworkMessages_StartGame
    {
        public int m_latency = 0;
    }
}