using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class MyGameResult : GameResult
    {
        public bool m_is_dropout = false;
        public long m_winner_player_pstid = 0;
    }
}