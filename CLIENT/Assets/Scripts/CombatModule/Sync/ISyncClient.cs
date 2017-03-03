using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ISyncClient : IDestruct
    {
        void Init(ILogicWorld logic_world, IOutsideWorld outside_world);
        void AddPlayer(long player_pstid);
        void RemovePlayer(long player_pstid);
        void Start(int current_time, long local_player_pstid, int latency);
        void Stop();
        void Update(int current_time);
        void PushLocalCommand(Command command);
        void PushServerCommand(Command command);
        void AddOutputCommand(Command command);
        List<Command> GetOutputCommands();
        void ClearOutputCommand();
    }

    public abstract class SyncClient : ISyncClient
    {
        protected ILogicWorld m_logic_world;
        protected IOutsideWorld m_outside_world;
        protected ICommandSynchronizer m_command_synchronizer;
        protected IWorldSynchronizer m_world_syhchronizer;
        protected List<Command> m_output_commands = new List<Command>();
        public virtual void Destruct()
        {
            m_logic_world = null;
            m_outside_world = null;
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
        public abstract void Init(ILogicWorld logic_world, IOutsideWorld outside_world);
        public abstract void AddPlayer(long player_pstid);
        public abstract void RemovePlayer(long player_pstid);
        public abstract void Start(int current_time, long local_player_pstid, int latency);
        public abstract void Stop();
        public abstract void Update(int current_time);
        public abstract void PushLocalCommand(Command command);
        public abstract void PushServerCommand(Command command);
        public void AddOutputCommand(Command command)
        {
            m_output_commands.Add(command);
        }
        public List<Command> GetOutputCommands()
        {
            return m_output_commands;
        }
        public void ClearOutputCommand()
        {
            m_output_commands.Clear();
        }
    }
}