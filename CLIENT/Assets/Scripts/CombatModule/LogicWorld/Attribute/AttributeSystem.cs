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

        public static void RegisterAttribute(int id)
        {
            ms_all_ids.Add(id);
        }

        public AttributeDefinition GetDefinitionByName(string attribute_name)
        {
            AttributeDefinition definition;
            m_definitions_by_name.TryGetValue(attribute_name, out definition);
            return definition;
        }

        public AttributeDefinition GetDefinitionByID(int attribute_id)
        {
            AttributeDefinition definition;
            m_definitions_by_id.TryGetValue(attribute_id, out definition);
            return definition;
        }

        public void InitializeAllDefinition(IConfigProvider config_provider)
        {
            if (m_initialized)
                return;
            ms_all_ids.Sort();
            int pre_id = 0;
            for (int i = 0; i < ms_all_ids.Count; ++i)
            {
                if (ms_all_ids[i] == pre_id)
                    continue;
                pre_id = ms_all_ids[i];
                AttributeData data = config_provider.GetAttributeData(ms_all_ids[i]);
                AttributeDefinition definition = new AttributeDefinition(data);
                m_definitions_by_id[data.m_attribute_id] = definition;
                m_definitions_by_name[data.m_attribute_name] = definition;
            }
            BuildStaticDependency();
            m_initialized = true;
        }

        public void BuildStaticDependency()
        {
            var enumerator = m_definitions_by_name.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AttributeDefinition definition = enumerator.Current.Value;
                List<string> referenced_attributes = definition.BuildReferencedAttributes();
                for (int i = 0; i < referenced_attributes.Count; ++i)
                {
                    AttributeDefinition referenced_definition;
                    if (m_definitions_by_name.TryGetValue(referenced_attributes[i], out referenced_definition))
                        referenced_definition.AddStaticDependentAttribute(definition.Name);
                }
            }
        }
    }
}