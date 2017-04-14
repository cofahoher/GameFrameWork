using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public struct TargetGatheringType
    {
        public static readonly int VID_TargetsSetInSkillActive = (int)CRC.Calculate("TargetsSetInSkillActive");
    }
    public class TargetGatheringManager : IDestruct
    {
        LogicWorld m_logic_world;
        List<Target> m_target_list = new List<Target>();
        public TargetGatheringManager(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
        }
        public void Destruct()
        {
            m_logic_world = null;
        }

        public List<Target> BuildTargetList(int target_gathering_type, FixPoint target_gathering_param1, FixPoint target_gathering_param2, 
            Skill skill, Entity source_entity)
        {
            m_target_list.Clear();

            if(target_gathering_type == TargetGatheringType.VID_TargetsSetInSkillActive)
            {
                m_target_list = skill.GetTargets();
            }

            return m_target_list;
        }
    }
}
