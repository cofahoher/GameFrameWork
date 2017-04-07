using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IConfigProvider
    {
        LevelData GetLevelData(int id);
        ObjectTypeData GetObjectTypeData(int id);
        ObjectProtoData GetObjectProtoData(int id);
        AttributeData GetAttributeData(int id);
        FixPoint GetLevelBasedNumber(string table_name, int level);
        FixPoint GetLevelBasedNumber(int table_id, int level);
    }


    public class LevelData
    {
        public string m_scene_name;
    }


    public class ObjectTypeData
    {
        public string m_name;
        public List<ComponentData> m_components_data = new List<ComponentData>();
    }
    public class ComponentData
    {
        public int m_component_type_id;
        public Dictionary<string, string> m_component_variables = new Dictionary<string, string>();
    }


    public class ObjectProtoData
    {
        public string m_name;
        public Dictionary<string, string> m_component_variables = new Dictionary<string, string>();
        public SortedDictionary<int, int> m_attribute_data = new SortedDictionary<int, int>();
        //下面的都未定，随便写的
        public List<int> m_skills;
    }

    public class AttributeData
    {
        public int m_attribute_id = -1;
        public string m_attribute_name;
        public string m_formula;
        public string m_reflection_property;
        public string m_clamp_property;
        public FixPoint m_clamp_min_value = FixPoint.Zero;
    }
}