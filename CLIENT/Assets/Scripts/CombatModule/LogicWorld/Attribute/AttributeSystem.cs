using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class PredefinedAttribute
    {
        public static readonly int MaxSpeed = (int)CRC.Calculate("MaxSpeed");
    }

    public class AttributeSystem : Singleton<AttributeSystem>
    {
        static SortedDictionary<int, string> ms_all_ids = new SortedDictionary<int, string>();
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

        public static void RegisterAttribute(AttributeData data)
        {
#if UNITY_EDITOR
            string existed_name;
            if (ms_all_ids.TryGetValue(data.m_attribute_id, out existed_name))
                LogWrapper.LogError("AttributeSystem, attribute ", data.m_attribute_name, " has same crcid with existed attribute ", existed_name);
#endif
            ms_all_ids[data.m_attribute_id] = data.m_attribute_name;
        }

        public static bool IsAttributeID(int crcid)
        {
            if (ms_all_ids.ContainsKey(crcid))
                return true;
            else
                return false;
        }

        public AttributeDefinition GetDefinitionByID(int attribute_id)
        {
            AttributeDefinition definition;
            m_definitions_by_id.TryGetValue(attribute_id, out definition);
            return definition;
        }

        public AttributeDefinition GetDefinitionByName(string attribute_name)
        {
            AttributeDefinition definition;
            m_definitions_by_name.TryGetValue(attribute_name, out definition);
            return definition;
        }

        public string AttributeID2Name(int attribute_id)
        {
            AttributeDefinition definition;
            if(m_definitions_by_id.TryGetValue(attribute_id, out definition))
                return definition.Name;
            else
                return null;
        }

        public int AttributeName2ID(string attribute_name)
        {
            AttributeDefinition definition;
            if (m_definitions_by_name.TryGetValue(attribute_name, out definition))
                return definition.ID;
            else
                return 0;
        }

        public void InitializeAllDefinition(IConfigProvider config_provider)
        {
            if (m_initialized)
                return;
            var enumerator = ms_all_ids.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AttributeData data = config_provider.GetAttributeData(enumerator.Current.Key);
                if (data == null)
                    continue;
                AttributeDefinition definition = new AttributeDefinition(data);
                m_definitions_by_id[data.m_attribute_id] = definition;
                m_definitions_by_name[data.m_attribute_name] = definition;
            }
            BuildStaticDependency();
            m_initialized = true;
        }

        public void BuildStaticDependency()
        {
            var enumerator = m_definitions_by_id.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AttributeDefinition definition = enumerator.Current.Value;
                List<int> referenced_attributes = definition.BuildReferencedAttributes();
                for (int i = 0; i < referenced_attributes.Count; ++i)
                {
                    AttributeDefinition referenced_definition;
                    if (m_definitions_by_id.TryGetValue(referenced_attributes[i], out referenced_definition))
                        referenced_definition.AddStaticDependentAttribute(definition.ID);
                }
            }
        }
    }
}