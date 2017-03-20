using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeData
    {
        public int m_attribute_id = -1;
        public string m_attribute_name;
        public string m_formula;
        public string m_reflection_property;
        public string m_clamp_property;
        public int m_clamp_min_value = -1;
    }

    public class AttrubuteConfig
    {
        public SortedDictionary<int, AttributeData> m_attributes_data = new SortedDictionary<int,AttributeData>();

        public AttrubuteConfig()
        {
            InitDummyConfigData();
        }

        public AttributeData GetAttributeData(int attribute_id)
        {
            AttributeData attribute_data = null;
            if (!m_attributes_data.TryGetValue(attribute_id, out attribute_data))
                return null;
            return attribute_data;
        }

        public SortedDictionary<int, AttributeData> GetAllAttributeData()
        {
            return m_attributes_data;
        }

        public void InitDummyConfigData()
        {
            //假装有配置
        }
    }
}