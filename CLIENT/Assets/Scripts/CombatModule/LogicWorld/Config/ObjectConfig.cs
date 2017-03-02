using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ComponentVariable
    {
        public string m_key;
        public string m_value;
    }
    public class ComponentData
    {
        public int m_component_type_id;
        public List<ComponentVariable> m_component_variables = new List<ComponentVariable>();
    }

    //Object的组件定义
    public class ObjectTypeData
    {
        public List<ComponentData> m_components_data = new List<ComponentData>();
    }

    //Object的原型，在组件定义的基础上，加了属性、技能、渲染数据等数据
    public class ObjectProtoData
    {
        public SortedDictionary<int, int> m_attribute_data = new SortedDictionary<int, int>();
        //下面的都未定，随便写的
        public List<int> m_skills;
        public int m_render_data;
    }

    public class ObjectConfig
    {
        public Dictionary<int, ObjectTypeData> m_object_type_data;
        public Dictionary<int, ObjectProtoData> m_object_proto_data;

        public ObjectConfig()
        {
            InitDummyConfigData();
        }

        public ObjectTypeData GetTypeData(int object_type_id)
        {
            ObjectTypeData type_data = null;
            if (!m_object_type_data.TryGetValue(object_type_id, out type_data))
                return null;
            return type_data;
        }

        public ObjectProtoData GetProtoData(int object_proto_id)
        {
            ObjectProtoData proto_data = null;
            if (!m_object_proto_data.TryGetValue(object_proto_id, out proto_data))
                return null;
            return proto_data;
        }

        public void InitDummyConfigData()
        {
            //假装有配置
        }
    }
}