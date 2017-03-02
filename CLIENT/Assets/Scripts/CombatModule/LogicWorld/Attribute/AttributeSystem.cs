using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeSystem : Singleton<AttributeSystem>
    {
        bool m_initialized = false;
        SortedDictionary<int, AttributeDefinition> m_definitions_by_id = new SortedDictionary<int, AttributeDefinition>();
        SortedDictionary<string, AttributeDefinition> m_definitions_by_name = new SortedDictionary<string, AttributeDefinition>();

        private AttributeSystem()
        {
        }

        public override void Destruct()
        {
            var enumerator = m_definitions_by_id.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AttributeDefinition definition = enumerator.Current.Value;
                definition.Destruct();
            }
            m_definitions_by_id.Clear();
            m_definitions_by_name.Clear();
        }

        public void InitializeAllDefinition()
        {
            if (m_initialized)
                return;
            var all_attribute_config = GlobalConfigManager.Instance.GetAttrubuteConfig().GetAllAttributeData();
            var enumerator = all_attribute_config.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AttributeData config = enumerator.Current.Value;
                AttributeDefinition definition = new AttributeDefinition(config);
                m_definitions_by_id[config.m_attribute_id] = definition;
                m_definitions_by_name[config.m_attribute_name] = definition;
            }
            BuildStaticDependency();
            m_initialized = true;
        }

        public AttributeDefinition GetDefinition(string attribute_name)
        {
            AttributeDefinition definition;
            m_definitions_by_name.TryGetValue(attribute_name, out definition);
            return definition;
        }

        public void BuildStaticDependency()
        {
            var enumerator = m_definitions_by_name.GetEnumerator();
            List<string> dependencies = new List<string>();
            while (enumerator.MoveNext())
            {
                AttributeDefinition definition = enumerator.Current.Value;
                string name = definition.Name;
                definition.BuildDependentAttribuites(dependencies);
                for (int i = 0; i < dependencies.Count; ++i)
                {
                    AttributeDefinition dependent_definition;
                    m_definitions_by_name.TryGetValue(dependencies[i], out dependent_definition);
                    definition.AddStaticDependentAttribute(dependent_definition.Name);
                }
            }
        }
    }
}