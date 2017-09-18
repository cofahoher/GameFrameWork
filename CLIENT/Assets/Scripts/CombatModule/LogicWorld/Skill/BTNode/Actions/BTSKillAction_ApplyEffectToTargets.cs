using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTSKillAction_ApplyEffectToTargets : BTAction
    {
        //配置数据
        int m_generator_cfgid = 0;

        //运行数据
        EffectGenerator m_generator;

        public BTSKillAction_ApplyEffectToTargets()
        {
        }

        public BTSKillAction_ApplyEffectToTargets(BTSKillAction_ApplyEffectToTargets prototype)
            : base(prototype)
        {
            m_generator_cfgid = prototype.m_generator_cfgid;
        }

        protected override void ResetRuntimeData()
        {
            if (m_generator != null)
            {
                SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
                GetLogicWorld().GetEffectManager().DestroyGenerator(m_generator.ID, skill_component.GetOwnerEntityID());
                m_generator = null;
            }
        }

        public override void ClearRunningTrace()
        {
            if (m_generator != null)
                m_generator.Deactivate();
        }

        protected override void OnActionEnter()
        {
            if (m_generator == null)
            {
                SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
                m_generator = GetLogicWorld().GetEffectManager().CreateGenerator(m_generator_cfgid, skill_component.GetOwnerEntity());
            }
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
            if (m_generator == null)
                return;
            SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
            Skill skill = skill_component.GetOwnerSkill();
            List<Target> targets = skill.GetTargets();
            if (targets.Count == 0)
                return;
            Entity caster = skill.GetOwnerEntity();
            EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
            app_data.m_original_entity_id = caster.ID;
            app_data.m_source_entity_id = caster.ID;
            m_generator.Activate(app_data, targets);
            RecyclableObject.Recycle(app_data);
        }

        protected override void OnActionExit()
        {
        }
    }
}