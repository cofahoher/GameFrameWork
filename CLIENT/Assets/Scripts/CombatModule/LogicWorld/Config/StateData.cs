using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class StateData
    {
        public int m_state_id = 0;
        public string m_state_name;
        public int m_state_category = 0;
        public int m_render_effect_cfgid = 0;
        public List<int> m_disable_componnets = new List<int>();
    }
}