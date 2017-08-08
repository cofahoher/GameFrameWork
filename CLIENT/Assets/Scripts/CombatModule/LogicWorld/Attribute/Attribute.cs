using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Attribute : IExpressionVariableProvider, IRecyclable
    {
        AttributeManagerComponent m_owner_component;
        AttributeDefinition m_definition;
        Formula m_base_value_formula;
        bool m_is_level_based = false;
        FixPoint m_base_value = FixPoint.Zero;
        FixPoint m_value = FixPoint.Zero;
        SortedDictionary<int, int> m_dynamic_dependent_attributes;
        SortedDictionary<int, AttributeModifier> m_modifiers;

        #region 初始化/销毁
        public void Construct(AttributeManagerComponent owner_component, AttributeDefinition definition, string base_value)
        {
            m_owner_component = owner_component;
            m_definition = definition;
            m_base_value_formula = RecyclableObject.Create<Formula>();
            m_base_value_formula.Compile(base_value);

            int count = 0;
            List<ExpressionVariable> variables = m_base_value_formula.GetAllVariables();
            if (variables != null)
                count = variables.Count;
            for (int i = 0; i < count; ++i)
            {
                ExpressionVariable variable = variables[i];
                if (variable.MaxIndex == 1 && variable[0] == ExpressionVariable.VID_LevelTable)
                    m_is_level_based = true;
            }

            ComputeValue();
            m_definition.Reflect(m_owner_component.ParentObject, this, true);
        }

        public void Reset()
        {
            m_owner_component = null;
            m_definition = null;
            RecyclableObject.Recycle(m_base_value_formula);
            m_base_value_formula = null;
            m_is_level_based = false;
            m_base_value = FixPoint.Zero;
            m_value = FixPoint.Zero;
            if (m_dynamic_dependent_attributes != null)
                m_dynamic_dependent_attributes.Clear();
            if (m_modifiers != null)
            {
                var enumerator = m_modifiers.GetEnumerator();
                while (enumerator.MoveNext())
                    RecyclableObject.Recycle(enumerator.Current.Value);
                m_modifiers.Clear();
            }
        }
        #endregion

        #region GETTER
        public FixPoint BaseValue
        {
            get { return m_base_value; }
        }
        public FixPoint Value
        {
            get { return m_value; }
        }
        public bool LevelBased
        {
            get { return m_is_level_based || m_definition.LevelBased; }
        }
        #endregion

        #region DynamicDependent
        public void AddDynamicDependentAttribute(int attribute_id)
        {
            if (m_dynamic_dependent_attributes == null)
                m_dynamic_dependent_attributes = new SortedDictionary<int, int>();
            int ref_cnt = 0;
            if (!m_dynamic_dependent_attributes.TryGetValue(attribute_id, out ref_cnt))
                m_dynamic_dependent_attributes[attribute_id] = 1;
            else
                m_dynamic_dependent_attributes[attribute_id] = ref_cnt + 1;
        }

        public void RemoveDynamicDependentAttribute(int attribute_id)
        {
            if (m_dynamic_dependent_attributes == null)
                return;
            int ref_cnt = 0;
            if (!m_dynamic_dependent_attributes.TryGetValue(attribute_id, out ref_cnt))
                return;
            ref_cnt -= 1;
            if (ref_cnt == 0)
                m_dynamic_dependent_attributes.Remove(attribute_id);
            else
                m_dynamic_dependent_attributes[attribute_id] = ref_cnt;
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
            RecyclableObject.Recycle(modifier);
            MarkDirty();
        }

        FixPoint GetModifierValueOfOneCategory(int category)
        {
            FixPoint value = FixPoint.Zero;
            if (m_modifiers == null)
                return value;
            var enumerator = m_modifiers.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AttributeModifier modifier = enumerator.Current.Value;
                if (modifier.Category == category)
                {
                    FixPoint modifier_value = modifier.Value;
                    value += modifier_value;
                }
            }
            return value;
        }
        #endregion

        #region Update
        public void OnLevelChanged()
        {
            if (LevelBased)
                MarkDirty();
        }

        void MarkDirty()
        {
            MarkDirtyStatic(m_owner_component, m_definition.ID);
        }

        static void MarkDirtyStatic(AttributeManagerComponent owner_component, int attribute_id)
        {
            Attribute attribute = owner_component.GetAttributeByID(attribute_id);
            if (attribute == null)
                return;
            attribute.ComputeValue();
            AttributeDefinition definition = AttributeSystem.Instance.GetDefinitionByID(attribute_id);
            Object owner = owner_component.ParentObject;
            definition.Reflect(owner, attribute);
            List<int> static_dependent_attributes = definition.GetStaticDependentAttributes();
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
            m_base_value = m_base_value_formula.Evaluate(this);
            m_value = m_definition.ComputeValue(this);
        }

        #region Variable
        public FixPoint GetVariable(ExpressionVariable variable, int index)
        {
            int vid = variable[index];
            if (index == variable.MaxIndex)
            {
                if (vid == ExpressionVariable.VID_Value)
                    return Value;
                else if (vid == ExpressionVariable.VID_BaseValue)
                    return BaseValue;
                else
                    return ObjectUtil.GetVariable(m_owner_component.ParentObject, vid);
            }
            else if (vid == ExpressionVariable.VID_LevelTable)
            {
                return m_owner_component.GetLogicWorld().GetConfigProvider().GetLevelBasedNumber(variable[index + 1], ObjectUtil.GetLevel(m_owner_component.ParentObject));
            }
            else if (vid == ExpressionVariable.VID_ModifierList)
            {
                ++index;
                vid = variable[index];
                return GetModifierValueOfOneCategory(vid);
            }
            else
            {
                return m_owner_component.GetVariable(variable, index);
            }
        }
        #endregion
    }
}