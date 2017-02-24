using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class SPPredictedCommandSynchronizer : CommandSynchronizer
    {
        public SPPredictedCommandSynchronizer()
        {
        }
    }

    public class SPTrustedCommandSynchronizer : CommandSynchronizer
    {
        SortedDictionary<long, PlayerTurnState> m_turn_state_of_players = new SortedDictionary<long, PlayerTurnState>();
        int m_top_turn = 0;
        int m_ready_turn = -1;

        const int MAX_SYNCTURN_INDEX = 10000;
        const int MAX_INT = int.MaxValue;

        public SPTrustedCommandSynchronizer()
        {
        }

        public override void Destruct()
        {
            m_turn_state_of_players.Clear();
            base.Destruct();
        }

        public override void AddPlayer(long player_pstid)
        {
            PlayerTurnState pts = new PlayerTurnState();
            m_turn_state_of_players[player_pstid] = pts;
        }

        public override void RemovePlayer(long player_pstid)
        {
            PlayerTurnState pts;
            if (m_turn_state_of_players.TryGetValue(player_pstid, out pts))
                pts.Valid = false;
        }

        public override bool AddCommand(Command command)
        {
            int syncturn = command.SyncTurn;
            if (syncturn < 0 || syncturn > MAX_SYNCTURN_INDEX)
                return false;
            long player_pstid = command.PlayerPstid;
            if (player_pstid == -1)
            {
                if (command.Type != CommandType.SyncTurnDone)
                    return false;
                syncturn += 1;
                SortedDictionary<long, PlayerTurnState>.Enumerator enumerator = m_turn_state_of_players.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    PlayerTurnState pts = enumerator.Current.Value;
                    if (syncturn > pts.SyncTurn)
                        pts.SyncTurn = syncturn;
                }
                if (syncturn > m_top_turn)
                    m_top_turn = syncturn;
                if (syncturn - 1 > m_ready_turn)
                    m_ready_turn = syncturn - 1;
                return true;
            }
            else
            {
                PlayerTurnState pts;
                if (!m_turn_state_of_players.TryGetValue(player_pstid, out pts))
                    return false;
                if (command.Type == CommandType.SyncTurnDone)
                    syncturn += 1;
                if (syncturn > pts.SyncTurn)
                {
                    pts.SyncTurn = syncturn;
                    CalculateReadyTurn();
                }
                if (syncturn > m_top_turn)
                    m_top_turn = syncturn;
                return base.AddCommand(command);
            }
        }

        public override int GetReadyTurn()
        {
            return m_ready_turn;
        }

        void CalculateReadyTurn()
        {
            int turn = MAX_INT;
            SortedDictionary<long, PlayerTurnState>.Enumerator enumerator = m_turn_state_of_players.GetEnumerator();
            while (enumerator.MoveNext())
            {
                PlayerTurnState pts = enumerator.Current.Value;
                if (pts.Valid && pts.SyncTurn < turn)
                    turn = pts.SyncTurn;
            }
            if (turn == MAX_INT)
                m_ready_turn = - 1;
            else
                m_ready_turn = turn - 1;
        }

        int GetPlayerCurrentTurn(long player_pstid)
        {
            PlayerTurnState pts;
            if (!m_turn_state_of_players.TryGetValue(player_pstid, out pts))
                return -1;
            else
                return pts.SyncTurn;
        }
    }
}