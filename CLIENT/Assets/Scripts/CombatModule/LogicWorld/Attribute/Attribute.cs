using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Attribute : IDestruct
    {
        AttributeManagerComponent m_owner_component;
        AttributeDefinition m_definition;
        int m_base_value = 0;
        int m_value = 0;
        SortedDictionary<string, int> m_dynamic_dependent_attributes;
        SortedDictionary<int, AttributeModifier> m_modifiers;

        public Attribute(AttributeManagerComponent owner_component, AttributeDefinition definition, int base_value)
        {
            m_owner_component = owner_component;
            m_definition = definition;
            m_base_value = base_value;
            ComputeValue();
        }

        public void Destruct()
        {
            m_owner_component = null;
            m_definition = null;
            var enumerator = m_modifiers.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AttributeModifier modifier = enumerator.Current.Value;
                modifier.Destruct();
            }
            m_modifiers.Clear();
        }

        #region GETTER
        public int BaseValue
        {
            get { return m_base_value; }
        }
        public int Value
        {
            get { return m_value; }
        }
        #endregion

        public void AddDynamicDependentAttribute(string attribute_name)
        {
            if (m_dynamic_dependent_attributes == null)
                m_dynamic_dependent_attributes = new SortedDictionary<string, int>();
            int ref_cnt = 0;
            if (!m_dynamic_dependent_attributes.TryGetValue(attribute_name, out ref_cnt))
                m_dynamic_dependent_attributes[attribute_name] = 1;
            else
                m_dynamic_dependent_attributes[attribute_name] = ref_cnt + 1;
        }

        public void RemoveDynamicDependentAttribute(string attribute_name)
        {
            if (m_dynamic_dependent_attributes == null)
                return;
            int ref_cnt = 0;
            if (!m_dynamic_dependent_attributes.TryGetValue(attribute_name, out ref_cnt))
                return;
            ref_cnt -= 1;
            if (ref_cnt == 0)
                m_dynamic_dependent_attributes.Remove(attribute_name);
            else
                m_dynamic_dependent_attributes[attribute_name] = ref_cnt;
        }

        public void AddModifier(AttributeModifier modifier)
        {
            if (m_modifiers == null)
                m_modifiers = new SortedDictionary<int, AttributeModifier>();
            m_modifiers[modifier.ID] = modifier;
            MarkDirty();
        }

        public void RemoveModifier(int modifier_id)
        {
            m_modifiers.Remove(modifier_id);
            MarkDirty();
        }

        void MarkDirty()
        {
            MarkDirtyStatic(m_owner_component, m_definition.Name);
        }

        static void MarkDirtyStatic(AttributeManagerComponent owner_component, string attribute_name)
        {
            Attribute attribute = owner_component.GetAttribute(attribute_name);
            if (attribute == null)
                return;
            attribute.ComputeValue();
            AttributeDefinition definition = AttributeSystem.Instance.GetDefinition(attribute_name);
            Object owner = owner_component.ParentObject;
            definition.Reflect(owner, attribute);
            List<string> static_dependent_attributes = definition.GetStaticDependentAttributes();
            for (int i = 0; i < static_dependent_attributes.Count; ++i)
                MarkDirtyStatic(owner_component, static_dependent_attributes[i]);
            if (attribute.m_dynamic_dependent_attributes != null)
            {
                var enumerator = attribute.m_dynamic_dependent_attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    MarkDirtyStatic(owner_component, enumerator.Current.Key);
                }
            }
        }

        void ComputeValue()
        {
            AttributeFormulaEvaluationContext context = AttributeFormulaEvaluationContext.Create();
            context.Initialize(m_owner_component.ParentObject, this);
            m_value = m_definition.ComputeValue(context);
            AttributeFormulaEvaluationContext.Recycle(context);
        }
    }
}