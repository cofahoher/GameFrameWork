using System.Collections;
using System.Collections.Generic;
namespace Combat
{
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