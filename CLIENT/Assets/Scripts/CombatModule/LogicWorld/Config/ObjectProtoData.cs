using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ObjectProtoData
    {
        public string m_name;
        public Dictionary<string, string> m_component_variables = new Dictionary<string, string>();
        public SortedDictionary<int, string> m_attributes = new SortedDictionary<int, string>();
        public SortedDictionary<int, int> m_skills = new SortedDictionary<int,int>();
    }
}