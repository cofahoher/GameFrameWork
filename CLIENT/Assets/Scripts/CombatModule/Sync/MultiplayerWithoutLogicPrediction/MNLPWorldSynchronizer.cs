using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class MNLPPlayerWorldSynchronizer : WorldSynchronizer
    {
        public MNLPPlayerWorldSynchronizer(ILogicWorld logic_world, ICommandSynchronizer command_synchronizer)
            : base(logic_world, command_synchronizer)
        {
        }
    }

    public class MNLPServerWorldSynchronizer : WorldSynchronizer
    {
        public MNLPServerWorldSynchronizer(ILogicWorld logic_world, ICommandSynchronizer command_synchronizer)
            : base(logic_world, command_synchronizer)
        {
        }
    }
}