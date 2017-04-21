using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public struct TargetGatheringType
    {
        //技能激活时查找的目标
        public static readonly int VID_TargetsFromSkillActivate = (int)CRC.Calculate("TargetsFromSkillActivate");

        //技能释放者
        public static readonly int VID_Source = (int)CRC.Calculate("Source");

        //当前AI主要目标
        public static readonly int VID_AITarget = (int)CRC.Calculate("AITarget");
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

        public List<Target> BuildTargetList(int target_gathering_type, FixPoint target_gathering_param1, FixPoint target_gathering_param2, Skill skill, Entity source_entity)
        {
            m_target_list.Clear();

            if (target_gathering_type == TargetGatheringType.VID_AITarget)
            {
                //yqqtodo 获取AI的主要目标，添加到m_target_list
            }

            return m_target_list;
        }

        public void BuildTargetList(int target_gathering_type, FixPoint target_gathering_param1, FixPoint target_gathering_param2, Skill skill, Entity source_entity, List<Target> targets)
        {
        }
    }
}
