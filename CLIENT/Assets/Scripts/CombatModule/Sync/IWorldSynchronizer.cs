using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IWorldSynchronizer : IDestruct
    {
        int GetSynchronizedTurn();
        void Start(int start_time);
        bool PushLocalCommand(Command command);
        bool PushClientCommand(Command command);
        bool PushServerCommand(Command command);
        bool ForwardFrame(int forward_end_time);
        bool ForwardTurn();
    }

    public abstract class WorldSynchronizer : IWorldSynchronizer
    {
        protected ILogicWorld m_logic_world;
        protected ICommandSynchronizer m_command_synchronizer;
        protected int m_synchronized_turn = -1;

        public WorldSynchronizer(ILogicWorld logic_world, ICommandSynchronizer command_synchronizer)
        {
            m_logic_world = logic_world;
            m_command_synchronizer = command_synchronizer;
        }

        public virtual void Destruct()
        {
            m_logic_world = null;
            m_command_synchronizer = null;
        }
        public int GetSynchronizedTurn()
        {
            return m_synchronized_turn;
        }
        public virtual void Start(int start_time)
        {
            m_logic_world.OnStart();
        }
        public virtual bool PushLocalCommand(Command command)
        {
            return false;
        }
        public virtual bool PushClientCommand(Command command)
        {
            return false;
        }
        public virtual bool PushServerCommand(Command command)
        {
            return false;
        }
        public virtual bool ForwardFrame(int forward_end_time)
        {
            return false;
        }
        public virtual bool ForwardTurn()
        {
            return false;
        }
        protected bool UpdateLogicFrame()
        {
            return m_logic_world.OnUpdate(SyncParam.FRAME_TIME);
        }
        protected bool UpdateLogicTurn()
        {
            for (int i = 0; i < SyncParam.FRAME_COUNT_PER_SYNCTURN; ++i)
            {
                if (m_logic_world.OnUpdate(SyncParam.FRAME_TIME))
                    return true;
            }
            return false;
        }
    }
}