using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Attribute : IRecyclable, IDestruct
    {
        #region Create/Recycle
        public static Attribute Create()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<Attribute>();
        }

        public static void Recycle(Attribute instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
        #endregion

        AttributeManagerComponent m_owner_component;
        AttributeDefinition m_definition;
        FixPoint m_base_value = default(FixPoint);
        FixPoint m_value = default(FixPoint);
        SortedDictionary<string, int> m_dynamic_dependent_attributes;
        SortedDictionary<int, AttributeModifier> m_modifiers;

        public Attribute()
        {
        }

        public void Construct(AttributeManagerComponent owner_component, AttributeDefinition definition, FixPoint base_value)
        {
            m_owner_component = owner_component;
            m_definition = definition;
            m_base_value = base_value;
            ComputeValue();
        }

        public void Destruct()
        {
            Reset();
        }

        public void Reset()
        {
            m_owner_component = null;
            m_definition = null;
            m_base_value = default(FixPoint);
            m_value = default(FixPoint);
            if (m_dynamic_dependent_attributes != null)
                m_dynamic_dependent_attributes.Clear();
            if (m_modifiers != null)
            {
                var enumerator = m_modifiers.GetEnumerator();
                while (enumerator.MoveNext())
                    AttributeModifier.Recycle(enumerator.Current.Value);
                m_modifiers.Clear();
            }
        }

        #region GETTER
        public FixPoint BaseValue
        {
            get { return m_base_value; }
        }
        public FixPoint Value
        {
            get { return m_value; }
        }
        #endregion

        #region DynamicDependent
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
        #endregion

        #region Modifier
        public void AddModifier(AttributeModifier modifier)
        {
            if (m_modifiers == null)
                m_modifiers = new SortedDictionary<int, AttributeModifier>();
            m_modifiers[modifier.ID] = modifier;
            MarkDirty();
        }

        public void RemoveModifier(int modifier_id)
        {
            AttributeModifier modifier;
            if (!m_modifiers.TryGetValue(modifier_id, out modifier))
                return;
            m_modifiers.Remove(modifier_id);
            AttributeModifier.Recycle(modifier);
            MarkDirty();
        }
        #endregion

        #region Update
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
            AttributeDefinition definition = AttributeSystem.Instance.GetDefinitionByName(attribute_name);
            Object owner = owner_component.ParentObject;
            definition.Reflect(owner, attribute);
            List<string> static_dependent_attributes = definition.GetStaticDependentAttributes();
            for (int i = 0; i < static_dependent_attributes.Count; ++i)
                MarkDirtyStatic(owner_component, static_dependent_attributes[i]);
            if (attribute.m_dynamic_dependent_attributes != null)
            {
                var enumerator = attribute.m_dynamic_dependent_attributes.GetEnumerator();
                while (enumerator.MoveNext())
                    MarkDirtyStatic(owner_component, enumerator.Current.Key);
            }
        }
        #endregion

        void ComputeValue()
        {
            AttributeFormulaEvaluationContext context = AttributeFormulaEvaluationContext.Create();
            context.Initialize(m_owner_component.ParentObject, this);
            m_value = m_definition.ComputeValue(context);
            AttributeFormulaEvaluationContext.Recycle(context);
        }
    }
}