using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTSKillAction_ApplyDamageToTargets : BTAction
    {
        //配置数据
        int m_damage_type_id = 0;
        Formula m_damage_amount = RecyclableObject.Create<Formula>();
        int m_damage_render_effect_cfgid = 0;
        int m_damage_sound_cfgid = 0;

        //运行数据

        public BTSKillAction_ApplyDamageToTargets()
        {
        }

        public BTSKillAction_ApplyDamageToTargets(BTSKillAction_ApplyDamageToTargets prototype)
            : base(prototype)
        {
            m_damage_type_id = prototype.m_damage_type_id;
            m_damage_amount.CopyFrom(m_damage_amount);
            m_damage_render_effect_cfgid = prototype.m_damage_render_effect_cfgid;
            m_damage_sound_cfgid = prototype.m_damage_sound_cfgid;
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
            Entity attacker = skill.GetOwnerEntity();
            List<Target> targets = skill.GetTargets();
            LogicWorld logic_world = skill.GetLogicWorld();
            for (int i = 0; i < targets.Count; ++i)
            {
                Entity current_target = targets[i].GetEntity(logic_world);
                if (current_target == null)
                    continue;
                skill_component.CurrentTarget = current_target;
                DamagableComponent damageable_component = current_target.GetComponent(DamagableComponent.ID) as DamagableComponent;
                if (damageable_component == null)
                    continue;
                Damage damage = RecyclableObject.Create<Damage>();
                damage.m_attacker_id = attacker.ID;
                damage.m_defender_id = current_target.ID;
                damage.m_damage_type = m_damage_type_id;
                damage.m_damage_amount = m_damage_amount.Evaluate(this);
                damage.m_damage_amount = DamageSystem.Instance.CalculateDamageAmount(m_damage_type_id, damage.m_damage_amount, attacker, current_target);
                damage.m_render_effect_cfgid = m_damage_render_effect_cfgid;
                damage.m_sound_cfgid = m_damage_sound_cfgid;
                damageable_component.TakeDamage(damage);
            }
            skill_component.CurrentTarget = null;
        }

        protected override void OnActionExit()
        {
        }
    }
}