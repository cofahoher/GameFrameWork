using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTSKillAction_CreateObject : BTAction
    {
        //配置数据
        int m_object_type_id = 0;
        int m_object_proto_id = 0;
        FixPoint m_object_life_time = FixPoint.Zero;
        int m_generator_cfgid = 0;
        Vector3FP m_offset;
        FixPoint m_angle_offset = FixPoint.Zero;

        //运行数据
        EffectGenerator m_generator;

        public BTSKillAction_CreateObject()
        {
        }

        public BTSKillAction_CreateObject(BTSKillAction_CreateObject prototype)
            : base(prototype)
        {
            m_object_type_id = prototype.m_object_type_id;
            m_object_proto_id = prototype.m_object_proto_id;
            m_object_life_time = prototype.m_object_life_time;
            m_generator_cfgid = prototype.m_generator_cfgid;
            m_offset = prototype.m_offset;
            m_angle_offset = prototype.m_angle_offset;
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
            if (m_generator_cfgid > 0 && m_generator == null)
            {
                SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
                m_generator = GetLogicWorld().GetEffectManager().CreateGenerator(m_generator_cfgid, skill_component.GetOwnerEntity());
            }
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
            SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
            Skill owner_skill = skill_component.GetOwnerSkill();
            Entity owner_entity = owner_skill.GetOwnerEntity();
            Target projectile_target = owner_skill.GetMajorTarget();
            EntityUtil.CreateEntityForSkillAndEffect(skill_component, owner_entity, projectile_target, m_offset, m_angle_offset, m_object_type_id, m_object_proto_id, m_object_life_time, m_generator);
        }

        protected override void OnActionExit()
        {
        }
    }
}