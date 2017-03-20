using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeDefinition : IDestruct
    {
        AttributeData m_config;
        AttributeFormula m_formula;
        List<string> m_static_dependent_attributes = new List<string>();

        public AttributeDefinition(AttributeData config)
        {
            m_config = config;
            m_formula = new AttributeFormula(config.m_formula);
        }

        public void Destruct()
        {
            m_config = null;
            m_formula.Destruct();
            m_formula = null; 
        }

        #region GETTER
        public string Name
        {
            get { return m_config.m_attribute_name; }
        }

        public List<string> GetStaticDependentAttributes()
        {
            return m_static_dependent_attributes;
        }
        #endregion

        public void AddStaticDependentAttribute(string attribute_name)
        {
            m_static_dependent_attributes.Add(attribute_name);
        }

        public void BuildDependentAttribuites(List<string> output)
        {
            m_formula.BuildReferencedList(output);
        }

        public int ComputeValue(AttributeFormulaEvaluationContext context)
        {
            return m_formula.ComputeValue(context);
        }

        public int GetDefaultValue(Object obj)
        {
            AttributeFormulaEvaluationContext context = AttributeFormulaEvaluationContext.Create();
            context.Initialize(obj, null);
            int result = m_formula.ComputeValue(context);
            AttributeFormulaEvaluationContext.Recycle(context);
            return result;
        }

        public void Reflect(Object obj, Attribute attribute)
        {
        }
    }
}