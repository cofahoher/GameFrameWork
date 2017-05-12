using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectGeneratorData
    {
        public bool m_deactivate_entry_when_deactive = false;
        public bool m_remove_by_self = true;
        public List<EffectGeneratorEntryData> m_entries = new List<EffectGeneratorEntryData>();
    }

    public class EffectGeneratorEntryData
    {
        public int m_target_gathering_type = 0;
        public FixPoint m_target_gathering_param1;
        public FixPoint m_target_gathering_param2;
        public int m_effect_id = 0;
    }
}