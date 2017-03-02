﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum CommandType
    {
        Invalid = 0,
        SyncTurnDone = 1,
    }

    public class Command
    {
        protected long m_player_pstid = -1;
        protected CommandType m_type = CommandType.Invalid;
        protected int m_syncturn = -1;
        public long PlayerPstid
        {
            get { return m_player_pstid; }
            set { m_player_pstid = value; }
        }
        public CommandType Type
        {
            get { return m_type; }
        }
        public int SyncTurn
        {
            get { return m_syncturn; }
            set { m_syncturn = value; }
        }
    }

    public class SyncTurnDoneCommand : Command
    {
        public SyncTurnDoneCommand()
        {
            m_type = CommandType.SyncTurnDone;
        }
    }
}