using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ComponentData
    {
        public int m_component_type_id;
        public Dictionary<string, string> m_component_variables = new Dictionary<string, string>();
    }

    //Object的组件定义
    public class ObjectTypeData
    {
        public string name;
        public List<ComponentData> m_components_data = new List<ComponentData>();
    }

    //Object的原型，在组件定义的基础上，加了属性、技能、渲染数据等数据
    public class ObjectProtoData
    {
        public string name;
        public Dictionary<string, string> m_component_variables = new Dictionary<string, string>();
        public SortedDictionary<int, int> m_attribute_data = new SortedDictionary<int, int>();
        //下面的都未定，随便写的
        public List<int> m_skills;
    }

    public class ObjectConfig
    {
        public Dictionary<int, ObjectTypeData> m_object_type_data = new Dictionary<int,ObjectTypeData>();
        public Dictionary<int, ObjectProtoData> m_object_proto_data = new Dictionary<int,ObjectProtoData>();

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
            //假装有配置：这么一行行写太累了

            //ObjectTypeData：100以下是Player，100以上是Entityi
            ObjectTypeData type_data = new ObjectTypeData();
            type_data.name = "EnvironmentPlayer";
            m_object_type_data[1] = type_data;

            type_data = new ObjectTypeData();
            type_data.name = "AIEnemyPlayer";
            m_object_type_data[2] = type_data;

            type_data = new ObjectTypeData();
            type_data.name = "LocalPlayer";
            m_object_type_data[3] = type_data;

            //Entity
            type_data = new ObjectTypeData();
            type_data.name = "Hero";
            ComponentData cd = new ComponentData();
            cd.m_component_type_id = ComponentTypeRegistry.CT_PositionComponent;
            cd.m_component_variables["ext_x"] = "500";
            cd.m_component_variables["ext_y"] = "500";
            cd.m_component_variables["ext_z"] = "500";
            cd.m_component_variables["visible"] = "True";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = ComponentTypeRegistry.CT_LocomotorComponent;
            cd.m_component_variables["max_speed"] = "500";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = ComponentTypeRegistry.CT_ModelComponent;
            type_data.m_components_data.Add(cd);

            m_object_type_data[101] = type_data;


            //ObjectProtoData：100以下是Player，100以上是Entityi
            ObjectProtoData proto_data = new ObjectProtoData();
            proto_data.name = "Cube";
            proto_data.m_component_variables["asset"] = "Objects/3D/zzw_cube";
            m_object_proto_data[101001] = proto_data;

            proto_data = new ObjectProtoData();
            proto_data.name = "Sphere";
            proto_data.m_component_variables["asset"] = "Objects/3D/zzw_sphere";
            m_object_proto_data[101002] = proto_data;
        }
    }
}