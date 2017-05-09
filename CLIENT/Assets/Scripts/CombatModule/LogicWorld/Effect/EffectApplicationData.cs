using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectApplicationData : IRecyclable
    {
        public int m_original_entity_id = 0;
        public int m_source_entity_id = 0;
        public int m_target_entity_id = 0;

        public void Reset()
        {
            m_original_entity_id = 0;
            m_source_entity_id = 0;
            m_target_entity_id = 0;
        }
    }
}