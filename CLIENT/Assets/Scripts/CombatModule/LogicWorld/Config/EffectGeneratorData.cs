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
        public TargetGatheringParam m_target_gathering_param = null;
        public int m_effect_id = 0;
        //public bool m_bidirectional_lookup = false;

        public EffectGeneratorEntryData()
        {
            m_target_gathering_param = new TargetGatheringParam();
            m_target_gathering_param.m_faction = FactionRelation.All;
        }
    }
}