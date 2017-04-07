using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeDefinition : IDestruct
    {
        AttributeData m_config;
        Formula m_formula;
        List<string> m_static_dependent_attributes = new List<string>();
        List<string> m_referenced_attributes = new List<string>();

        public AttributeDefinition(AttributeData config)
        {
            m_config = config;
            m_formula = Formula.Create();
            m_formula.Compile(config.m_formula);
        }

        public void Destruct()
        {
            m_config = null;
            Formula.Recycle(m_formula);
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

        public List<string> GetStaticDependentAttributes()
        {
            return m_static_dependent_attributes;
        }

        public List<string> GetReferencedAttributes()
        {
            return m_referenced_attributes;
        }
        #endregion

        public void AddStaticDependentAttribute(string attribute_name)
        {
            m_static_dependent_attributes.Add(attribute_name);
        }

        public List<string> BuildReferencedAttributes()
        {
            int count = 0;
            List<ExpressionVariable> variables = m_formula.GetAllVariables();
            if (variables != null)
                count = variables.Count;
            for (int i = 0; i < count; ++i)
            {
            }
            return m_referenced_attributes;
        }

        public FixPoint ComputeValue(IExpressionVariableProvider variable_provider)
        {
            return m_formula.Evaluate(variable_provider);
        }

        public void Reflect(Object obj, Attribute attribute)
        {
            //ZZWTODO
        }
    }
}