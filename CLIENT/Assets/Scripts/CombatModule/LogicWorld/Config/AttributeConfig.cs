using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeData
    {
    }

    public class AttrubuteConfig
    {
        public Dictionary<string, AttributeData> m_attributes_data;

        public AttrubuteConfig()
        {
            InitDummyConfigData();
        }

        public AttributeData GetAttributeData(string attribute_name)
        {
            AttributeData attribute_data = null;
            if (!m_attributes_data.TryGetValue(attribute_name, out attribute_data))
                return null;
            return attribute_data;
        }

        public void InitDummyConfigData()
        {
            //假装有配置
        }
    }
}