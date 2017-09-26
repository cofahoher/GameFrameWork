using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public class SkillTimer : IRecyclable
    {
        public const int CooldownTimer = 0;
        public const int CastingTimer = 1;
        public const int InflictingTimer = 2;
        public const int ExpirationTimer = 3;
        public const int TimerCount = 4;

        FixPoint m_start_time = FixPoint.Zero;
        FixPoint m_total_time = FixPoint.Zero;
        FixPoint m_update_rate = FixPoint.One;
        bool m_active = false;

        public void Reset()
        {
            m_start_time = FixPoint.Zero;
            m_total_time = FixPoint.Zero;
            m_update_rate = FixPoint.One;
            m_active = false;
        }

        public bool Active
        {
            get { return m_active; }
        }

        public void Start(FixPoint start_time, FixPoint total_time, FixPoint update_rate)
        {
            m_start_time = start_time;
            m_total_time = total_time;
            m_update_rate = update_rate;
            m_active = true;
        }

        public void ChangeUpdateRate(FixPoint current_time, FixPoint update_rate)
        {
            if (!m_active)
                return;
            FixPoint elapsed_time = (current_time - m_start_time) * m_update_rate;
            m_start_time = current_time;
            m_total_time -= elapsed_time;
            m_update_rate = update_rate;
        }

        public FixPoint GetRemaining(FixPoint current_time)
        {
            FixPoint remain_time = m_start_time + m_total_time / m_update_rate - current_time;
            if (remain_time < FixPoint.Zero)
                remain_time = FixPoint.Zero;
            return remain_time;
        }
    }
}
