using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTSKillAction_GatherTargets : BTAction
    {
        //配置数据
        TargetGatheringParam m_target_gathering_param = new TargetGatheringParam();

        //运行数据

        public BTSKillAction_GatherTargets()
        {
        }

        public BTSKillAction_GatherTargets(BTSKillAction_GatherTargets prototype)
            : base(prototype)
        {
            m_target_gathering_param.CopyFrom(prototype.m_target_gathering_param);
        }

        protected override void ResetRuntimeData()
        {
        }

        public override void ClearRunningTrace()
        {
        }

        protected override void OnActionEnter()
        {
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
            SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
            Skill owner_skill = skill_component.GetOwnerSkill();
            owner_skill.BuildSkillTargets(m_target_gathering_param);
        }

        protected override void OnActionExit()
        {
        }
    }
}