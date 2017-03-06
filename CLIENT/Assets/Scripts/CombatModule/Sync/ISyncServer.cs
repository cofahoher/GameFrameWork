using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ISyncServer : IDestruct
    {
        void Init(ILogicWorld logic_world);
        void AddPlayer(long player_pstid);
        void RemovePlayer(long player_pstid);
        void UpdatePlayerLatency(long player_pstid, int latency);
        void Start(int current_time, int latency);
        void Update(int current_time);
        void PushClientCommand(Command command);
        void AddOutputCommand(Command command);
        List<Command> GetOutputCommands();
        void ClearOutPutCommand();
    }

    public abstract class SyncServer : ISyncServer
    {
        protected ILogicWorld m_logic_world;
        protected ICommandSynchronizer m_command_synchronizer;
        protected IWorldSynchronizer m_world_syhchronizer;
        protected List<Command> m_output_commands = new List<Command>();
        public virtual void Destruct()
        {
            m_logic_world = null;
            if (m_command_synchronizer != null)
            {
                m_command_synchronizer.Destruct();
                m_command_synchronizer = null;
            }
            if (m_world_syhchronizer != null)
            {
                m_world_syhchronizer.Destruct();
                m_world_syhchronizer = null;
            }
        }
        public abstract void Init(ILogicWorld logic_world);
        public abstract void AddPlayer(long player_pstid);
        public abstract void RemovePlayer(long player_pstid);
        public abstract void UpdatePlayerLatency(long player_pstid, int latency);
        public abstract void Start(int current_time, int latency);
        public abstract void Update(int current_time);
        public abstract void PushClientCommand(Command command);
        public void AddOutputCommand(Command command)
        {
            m_output_commands.Add(command);
        }
        public List<Command> GetOutputCommands()
        {
            return m_output_commands;
        }
        public void ClearOutPutCommand()
        {
            m_output_commands.Clear();
        }
    }

    public class PlayerSyncData
    {
        public const int SYNC_STATE_OK = 0;
        public const int SYNC_STATE_IDLE = 1;
        public const int SYNC_STATE_DEAD = 2;
        int m_sync_state = SYNC_STATE_OK;
        int m_last_msg_syncturn = -1;
        int m_latency = 0;
        public int SyncState
        {
            get { return m_sync_state; }
            set { m_sync_state = value; }
        }
        public int LastMsgSyncTurn
        {
            get { return m_last_msg_syncturn; }
            set { m_last_msg_syncturn = value; }
        }
        public int Latency
        {
            get { return m_latency; }
            set { m_latency = value; }
        }
    }
}