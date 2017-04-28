using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public class TargetGatheringType
    {
        public static readonly int DefaultTarget = (int)CRC.Calculate("DefaultTarget");
        public static readonly int Source = (int)CRC.Calculate("Source");
    }

    public class TargetGatheringManager : IDestruct
    {
        LogicWorld m_logic_world;

        public TargetGatheringManager(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
        }

        public void Destruct()
        {
            m_logic_world = null;
        }

        public void BuildTargetList(Entity source_entity, int target_gathering_type, FixPoint target_gathering_param1, FixPoint target_gathering_param2, List<Target> targets)
        {
        }
    }
}
