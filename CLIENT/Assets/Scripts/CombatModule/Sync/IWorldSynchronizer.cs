using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IWorldSynchronizer : IDestruct
    {
        int GetSynchronizedTurn();
        void Start(int start_time);
        bool ForwardFrame(int forward_end_time);
        bool ForwardTurn();
    }

    public abstract class WorldSynchronizer : IWorldSynchronizer
    {
        protected ILogicWorld m_logic_world;
        protected ICommandSynchronizer m_command_synchronizer;

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
        public abstract int GetSynchronizedTurn();
        public abstract void Start(int start_time);
        public abstract bool ForwardFrame(int forward_end_time);
        public abstract bool ForwardTurn();
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