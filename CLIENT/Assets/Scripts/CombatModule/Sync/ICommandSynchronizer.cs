using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ICommandSynchronizer : IDestruct
    {
        void AddPlayer(long player_pstid);
        void RemovePlayer(long player_pstid);
        bool AddCommand(Command command);
        List<Command> GetCommands(int syncturn);
        int GetReadyTurn();
    }

    public abstract class CommandSynchronizer : ICommandSynchronizer
    {
        protected SortedDictionary<int, List<Command>> m_syncturn2commands = new SortedDictionary<int, List<Command>>();

        public virtual void Destruct()
        {
            m_syncturn2commands.Clear();
        }
        public virtual void AddPlayer(long player_pstid)
        {
        }
        public virtual void RemovePlayer(long player_pstid)
        {
        }
        public virtual bool AddCommand(Command command)
        {
            if (command.Type == CommandType.SyncTurnDone)
                return true;
            int syncturn = command.SyncTurn;
            List<Command> commands;
            if (!m_syncturn2commands.TryGetValue(syncturn, out commands))
            {
                commands = new List<Command>();
                m_syncturn2commands[syncturn] = commands;
            }
            commands.Add(command);
            return true;
        }
        public List<Command> GetCommands(int syncturn)
        {
            List<Command> commands;
            m_syncturn2commands.TryGetValue(syncturn, out commands);
            return commands;
        }
        public virtual int GetReadyTurn()
        {
            return -1;
        }
    }

    public class PlayerTurnState
    {
        //已知的进行中的（尚未完成）最高的syncturn
        int m_syncturn = 0;
        //是否在同步中
        bool m_valid = true;
        public int SyncTurn
        {
            get { return m_syncturn; }
            set { m_syncturn = value; }
        }
        public bool Valid
        {
            get { return m_valid; }
            set { m_valid = value; }
        }
    }
}