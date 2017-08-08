using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Damage : IRecyclable
    {
        public int m_attacker_id = 0;
        public int m_defender_id = 0;
        public int m_damage_type = 0;
        public FixPoint m_damage_amount = FixPoint.Zero;
        public int m_render_effect_cfgid = 0;
        public int m_sound_cfgid = 0;

        public void Reset()
        {
            m_attacker_id = 0;
            m_defender_id = 0;
            m_damage_type = 0;
            m_damage_amount = FixPoint.Zero;
            m_render_effect_cfgid = 0;
            m_sound_cfgid = 0;
        }
    }
}