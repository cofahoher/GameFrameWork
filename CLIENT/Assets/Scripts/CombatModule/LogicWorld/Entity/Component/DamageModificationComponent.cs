using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class DamageModificationComponent : EntityComponent
    {
        List<DamageModifier> m_modifiers;

        #region 初始化/销毁
        protected override void OnDestruct()
        {
            if (m_modifiers != null)
            {
                for (int i = 0; i < m_modifiers.Count; ++i)
                    DamageModifier.Recycle(m_modifiers[i]);
                m_modifiers.Clear();
            }
        }
        #endregion


        public void AddModifier(DamageModifier modifier)
        {
            if (m_modifiers == null)
                m_modifiers = new List<DamageModifier>();
            m_modifiers.Add(modifier);
        }

        public bool RemoveModifier(int modifier_id)
        {
            if (m_modifiers == null)
                return false;
            for (int i = 0; i < m_modifiers.Count; ++i)
            {
                if (m_modifiers[i].ID == modifier_id)
                {
                    DamageModifier.Recycle(m_modifiers[i]);
                    m_modifiers.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public FixPoint ApplyModifiersToDamage(Damage damage, FixPoint damage_amount, Object opponent, bool is_attacker)
        {
            if (m_modifiers == null || m_modifiers.Count == 0)
                return damage_amount;
            Entity owner_entity = GetOwnerEntity();
            for (int i = 0; i < m_modifiers.Count; ++i)
                damage_amount = m_modifiers[i].ApplyToDamage(damage, damage_amount, owner_entity, opponent, is_attacker);
            return damage_amount;
        }
    }
}