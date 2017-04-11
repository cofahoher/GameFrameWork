using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IConfigProvider
    {
        FixPoint GetLevelBasedNumber(int table_id, int level);
        FixPoint GetLevelBasedNumber(string table_name, int level);
        LevelData GetLevelData(int id);
        ObjectTypeData GetObjectTypeData(int id);
        ObjectProtoData GetObjectProtoData(int id);
        AttributeData GetAttributeData(int id);
    }

    public class LevelTableData
    {
        public int m_max_level = 0;
        public FixPoint[] m_table = null;
        public FixPoint this[int level]
        {
            get
            {
                if (level < 0)
                    return m_table[0];
                if (level > m_max_level)
                    return m_table[m_max_level];
                else
                    return m_table[level];
            }
        }
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
        public Dictionary<string, string> m_component_variables;
    }

    public class ObjectProtoData
    {
        public string m_name;
        public Dictionary<string, string> m_component_variables = new Dictionary<string, string>();
        public SortedDictionary<int, FixPoint> m_attributes = new SortedDictionary<int, FixPoint>();
        //下面的都未定，随便写的
        public List<int> m_skills;
    }

    public class AttributeData
    {
        public int m_attribute_id;
        public string m_attribute_name;
        public string m_formula;
        public int m_reflection_property = 0;
        public int m_clamp_property = 0;
        public FixPoint m_clamp_min_value = FixPoint.Zero;
    }
}