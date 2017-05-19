using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class DirectDamageSkillComponent : SkillComponent
    {
        //配置数据
        int m_damage_type_id = 0;
        Formula m_damage_amount = RecyclableObject.Create<Formula>();
        int m_combo_attack_cnt = 1;
        FixPoint m_combo_interval = FixPoint.Zero;

        //运行数据
        int m_remain_attack_cnt = 0;
        ComboAttackTask m_task;

        #region 初始化/销毁
        protected override void OnDestruct()
        {
            RecyclableObject.Recycle(m_damage_amount);
            m_damage_amount = null;

            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
        }
        #endregion

        public override void Inflict(FixPoint start_time)
        {
            m_remain_attack_cnt = m_combo_attack_cnt;
            Impact();
            if (m_combo_attack_cnt > 1)
            {
                if (m_task == null)
                {
                    m_task = LogicTask.Create<ComboAttackTask>();
                    m_task.Construct(this);
                }
                var schedeler = GetLogicWorld().GetTaskScheduler();
                schedeler.Schedule(m_task, GetCurrentTime(), m_combo_interval, m_combo_interval);
            }
        }

        public override void Deactivate()
        {
            if (m_task != null)
                m_task.Cancel();
        }

        public void Impact()
        {
            --m_remain_attack_cnt;
            if (m_remain_attack_cnt <= 0)
            {
                if (m_task != null)
                    m_task.Cancel();
            }
            Skill skill = GetOwnerSkill();
            if (!skill.GetDefinitionComponent().NeedGatherTargets)
                skill.BuildSkillTargets();
            Entity attacker = GetOwnerEntity();
            List<Target> targets = skill.GetTargets();
            for (int i = 0; i < targets.Count; ++i)
            {
                m_current_target = targets[i].GetEntity();
                if (m_current_target == null)
                    continue;
                DamagableComponent damageable_component = m_current_target.GetComponent(DamagableComponent.ID) as DamagableComponent;
                if (damageable_component == null)
                    continue;
                Damage damage = RecyclableObject.Create<Damage>();
                damage.m_attacker_id = attacker.ID;
                damage.m_defender_id = m_current_target.ID;
                damage.m_damage_type = m_damage_type_id;
                damage.m_damage_amount = m_damage_amount.Evaluate(this);
                damage.m_damage_amount = DamageSystem.Instance.CalculateDamageAmount(m_damage_type_id, damage.m_damage_amount, attacker, m_current_target);
                damageable_component.TakeDamage(damage);
            }
            m_current_target = null;
        }
    }

    class ComboAttackTask : Task<LogicWorld>
    {
        DirectDamageSkillComponent m_component;

        public void Construct(DirectDamageSkillComponent component)
        {
            m_component = component;
        }

        public override void OnReset()
        {
            m_component = null;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            m_component.Impact();
        }
    }
}
