using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectApplicationData : IRecyclable
    {
        public int m_original_entity_id = 0;
        public int m_source_entity_id = 0;
        public int m_target_entity_id = 0;
        public int m_generator_id = 0;
        public int m_entry_index = -1;

        public void Reset()
        {
            m_original_entity_id = 0;
            m_source_entity_id = 0;
            m_target_entity_id = 0;
            m_generator_id = 0;
            m_entry_index = -1;
        }

        public void CopyFrom(EffectApplicationData rhs)
        {
            m_original_entity_id = rhs.m_original_entity_id;
            m_source_entity_id = rhs.m_source_entity_id;
            m_target_entity_id = rhs.m_target_entity_id;
            m_generator_id = rhs.m_generator_id;
            m_entry_index = rhs.m_entry_index;
        }
    }
}