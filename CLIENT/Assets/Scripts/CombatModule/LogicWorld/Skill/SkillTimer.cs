using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public class SkillTimer : IRecyclable, IDestruct
    {
        public const int CooldownTimer = 0;
        public const int CastingTimer = 1;
        public const int InflictingTimer = 2;
        public const int ExpirationTimer = 3;
        public const int TimerCount = 4;

        FixPoint m_strat_time = FixPoint.Zero;
        FixPoint m_end_time = FixPoint.Zero;
        bool m_active = false;

        public void Reset()
        {
            m_strat_time = FixPoint.Zero;
            m_end_time = FixPoint.Zero;
            m_active = false;
        }

        public void Destruct()
        {
        }

        public bool Active
        {
            get { return m_active; }
        }

        public void Start(FixPoint start_time, FixPoint total_time)
        {
            m_strat_time = start_time;
            m_end_time = start_time + total_time;
            m_active = true;
        }

        public FixPoint GetRemaining(FixPoint current_time)
        {
            FixPoint remain_time = m_end_time - current_time;
            if (remain_time < FixPoint.Zero)
                remain_time = FixPoint.Zero;
            return remain_time;
        }
    }
}
