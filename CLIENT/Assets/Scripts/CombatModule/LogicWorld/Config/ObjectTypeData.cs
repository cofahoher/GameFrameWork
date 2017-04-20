using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ObjectTypeData
    {
        public string m_name;
        public List<ComponentData> m_components_data = new List<ComponentData>();
    }

    public class ComponentData
    {
        public int m_component_type_id;
        public Dictionary<string, string> m_component_variables;
    }
}