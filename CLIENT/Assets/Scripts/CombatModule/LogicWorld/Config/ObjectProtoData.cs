using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ObjectProtoData
    {
        public string m_name;
        public Dictionary<string, string> m_component_variables = new Dictionary<string, string>();
        public SortedDictionary<int, FixPoint> m_attributes = new SortedDictionary<int, FixPoint>();
        //下面的都未定，随便写的
        public List<int> m_skills;
    }
}