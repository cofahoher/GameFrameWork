using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeDefinition : IDestruct
    {
        AttributeData m_config;
        Formula m_formula;
        bool m_is_level_based = false;
        List<int> m_static_dependent_attributes = new List<int>();
        List<int> m_referenced_attributes = new List<int>();

        public AttributeDefinition(AttributeData config)
        {
            m_config = config;
            m_formula = RecyclableObject.Create<Formula>();
            m_formula.Compile(config.m_formula);
        }

        public void Destruct()
        {
            m_config = null;
            RecyclableObject.Recycle(m_formula);
            m_formula = null; 
        }

        #region GETTER
        public int ID
        {
            get { return m_config.m_attribute_id; }
        }

        public string Name
        {
            get { return m_config.m_attribute_name; }
        }

        public bool LevelBased
        {
            get { return m_is_level_based; }
        }

        public List<int> GetStaticDependentAttributes()
        {
            return m_static_dependent_attributes;
        }

        public List<int> GetReferencedAttributes()
        {
            return m_referenced_attributes;
        }
        #endregion

        public void AddStaticDependentAttribute(int attribute_id)
        {
            m_static_dependent_attributes.Add(attribute_id);
        }

        public List<int> BuildReferencedAttributes()
        {
            int count = 0;
            List<ExpressionVariable> variables = m_formula.GetAllVariables();
            if (variables != null)
                count = variables.Count;
            for (int i = 0; i < count; ++i)
            {
                ExpressionVariable variable = variables[i];
                if (variable.MaxIndex == 1 && variable[0] == ExpressionVariable.VID_LevelTable)
                {
                    m_is_level_based = true;
                }
                else if (variable.MaxIndex >= 2 && variable[variable.MaxIndex - 2] == ExpressionVariable.VID_Attribute || variable.MaxIndex == 1)
                {
                    int attribute_id = variable[variable.MaxIndex - 1];
                    if (AttributeSystem.IsAttributeID(attribute_id))
                        m_referenced_attributes.Add(attribute_id);
                }
            }
            return m_referenced_attributes;
        }

        public FixPoint ComputeValue(IExpressionVariableProvider variable_provider)
        {
            return m_formula.Evaluate(variable_provider);
        }

        public void Reflect(Object obj, Attribute attribute, bool initialize = false)
        {
            int vid = m_config.m_reflection_property;
            if (vid != 0)
            {
                int component_type_id = ComponentTypeRegistry.GetVariableOwnerComponentID(vid);
                if (component_type_id != 0)
                {
                    Component component = obj.GetComponent(component_type_id);
                    if (component != null)
                    {
                        FixPoint old_value = component.GetVariable(vid);
                        FixPoint new_value = attribute.Value;
                        if (old_value != new_value)
                            component.SetVariable(vid, attribute.Value);
                    }
                } 
            }
            vid = m_config.m_clamp_property;
            if (vid != 0)
            {
                int component_type_id = ComponentTypeRegistry.GetVariableOwnerComponentID(vid);
                if (component_type_id != 0)
                {
                    Component component = obj.GetComponent(component_type_id);
                    if (component != null)
                    {
                        FixPoint old_value = component.GetVariable(vid);
                        if (initialize)
                        {
                            if (old_value < m_config.m_clamp_min_value)  //ZZWTODO tricky
                                component.SetVariable(vid, attribute.Value);
                        }
                        else
                        {
                            FixPoint new_value = FixPoint.Clamp(old_value, m_config.m_clamp_min_value, attribute.Value);
                            if (old_value != new_value)
                                component.SetVariable(vid, new_value);
                        }
                    }
                }
            }
            //END
        }
    }
}