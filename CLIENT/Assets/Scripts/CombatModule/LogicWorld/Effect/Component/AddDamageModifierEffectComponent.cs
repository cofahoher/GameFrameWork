using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class AddDamageModifierEffectComponent : EffectComponent
    {
        //配置数据
        int m_modifier_type = 0;
        Dictionary<string, string> m_variables;

        //运行数据
        int m_modifier_id = 0;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("modifier_type", out value))
                m_modifier_type = (int)CRC.Calculate(value);
            m_variables = variables;
        }

        public override void Apply()
        {
            if (m_modifier_type == 0)
                return;
            Entity owner_entity = GetOwnerEntity();
            DamageModificationComponent damage_modification_component = owner_entity.GetComponent(DamageModificationComponent.ID) as DamageModificationComponent;
            if (damage_modification_component == null)
                return;
            DamageModifier modifier = DamageModifier.Create(m_modifier_type);
            if (modifier == null)
                return;
            m_modifier_id = GetLogicWorld().GenerateDamageModifierID();
            modifier.Contruct(m_modifier_id, m_variables);
            damage_modification_component.AddModifier(modifier);
        }

        public override void Unapply()
        {
            if (m_modifier_id == 0)
                return;
            Entity owner_entity = GetOwnerEntity();
            DamageModificationComponent damage_modification_component = owner_entity.GetComponent(DamageModificationComponent.ID) as DamageModificationComponent;
            if (damage_modification_component == null)
                return;
            damage_modification_component.RemoveModifier(m_modifier_id);
            m_modifier_id = 0;
        }
    }
}