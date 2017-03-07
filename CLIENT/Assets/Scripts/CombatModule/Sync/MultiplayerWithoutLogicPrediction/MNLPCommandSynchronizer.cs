using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class MNLPClientCommandSynchronizer : CommandSynchronizer
    {
        int m_ready_turn = 0;

        public override bool AddCommand(Command command)
        {
            long player_pstid = command.PlayerPstid;
            if (player_pstid == -1)
            {
                if (command.Type != CommandType.SyncTurnDone)
                    return false;
                int syncturn = command.SyncTurn;
                if (syncturn > m_ready_turn)
                    m_ready_turn = syncturn;
            }
            return base.AddCommand(command);
        }

        public override int GetReadyTurn()
        {
            return m_ready_turn;
        }
    }

    public class MNLPServerCommandSynchronizer : CommandSynchronizer
    {
    }
}