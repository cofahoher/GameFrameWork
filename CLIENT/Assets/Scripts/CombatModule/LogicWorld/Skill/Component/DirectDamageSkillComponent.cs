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
        Entity m_current_target = null;

        #region 初始化/销毁
        protected override void OnDestruct()
        {
            RecyclableObject.Recycle(m_damage_amount);
            m_damage_amount = null;
        }
        #endregion

        public override void Inflict(FixPoint start_time)
        {
            Entity attacker = GetOwnerEntity();
            List<Target> targets = GetOwnerSkill().GetTargets();
            for (int i = 0; i < targets.Count; ++i)
            {
                m_current_target = targets[i].GetEntity();
                if (m_current_target == null)
                    continue;
                DamagableComponent damageable_component = m_current_target.GetComponent<DamagableComponent>();
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

        public override void Deactivate()
        {
        }

        public override FixPoint GetVariable(ExpressionVariable variable, int index)
        {
            if (variable[index] == ExpressionVariable.VID_Target)
            {
                if (m_current_target != null)
                    return m_current_target.GetVariable(variable, index + 1);
                else
                    return FixPoint.Zero;
            }
            return base.GetVariable(variable, index);
        }
    }
}
