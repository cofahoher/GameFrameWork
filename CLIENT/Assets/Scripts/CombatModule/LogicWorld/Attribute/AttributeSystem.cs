using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeSystem : Singleton<AttributeSystem>
    {
        static List<int> ms_all_ids = new List<int>();
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

        public static void AddAttributeID(int id)
        {
            ms_all_ids.Add(id);
        }

        public void InitializeAllDefinition(IConfigProvider config)
        {
            if (m_initialized)
                return;
            ms_all_ids.Sort();
            for (int i = 0; i < ms_all_ids.Count; ++i)
            {
                AttributeData data = config.GetAttributeData(ms_all_ids[i]);
                AttributeDefinition definition = new AttributeDefinition(data);
                m_definitions_by_id[data.m_attribute_id] = definition;
                m_definitions_by_name[data.m_attribute_name] = definition;
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