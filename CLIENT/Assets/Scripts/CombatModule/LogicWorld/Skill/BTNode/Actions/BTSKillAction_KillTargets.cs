using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTSKillAction_KillTargets : BTAction
    {
        //配置数据

        //运行数据

        public BTSKillAction_KillTargets()
        {
        }

        public BTSKillAction_KillTargets(BTSKillAction_KillTargets prototype)
            : base(prototype)
        {
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
            Skill skill = skill_component.GetOwnerSkill();

            List<Target> targets = skill.GetTargets();
            LogicWorld logic_world = GetLogicWorld();
            for (int i = 0; i < targets.Count; ++i)
            {
                Entity current_target = targets[i].GetEntity(logic_world);
                if (current_target == null)
                    continue;
                skill_component.CurrentTarget = current_target;
                EntityUtil.KillEntity(current_target, skill.GetOwnerEntityID());
            }
            skill_component.CurrentTarget = null;
        }

        protected override void OnActionExit()
        {
        }
    }
}