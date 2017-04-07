using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public enum SkillTimerType
    {
        CastingTimer = 0,
        CooldownTimer,
        ExpirationTimer,

        TimerCount,
    }

    public class SkillTimer : IRecyclable, IDestruct
    {
        FixPoint m_strat_time = FixPoint.Zero;
        FixPoint m_end_time = FixPoint.Zero;
        FixPoint m_total_time = FixPoint.Zero;
        bool m_active = false;

        #region Create/Recycle
        public static SkillTimer Create()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<SkillTimer>();
        }

        public static void Recycle(SkillTimer instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
        #endregion

        public void Reset()
        {
            m_strat_time = FixPoint.Zero;
            m_end_time = FixPoint.Zero;
            m_total_time = FixPoint.Zero;
            m_active = false;
        }

        public void Destruct()
        {
        }

        public void SetStartTotalTimes(FixPoint start_time, FixPoint total_time)
        {
            m_strat_time = start_time;
            m_total_time = total_time;
            m_end_time = start_time + total_time;
            m_active = true;
        }

        public void SetStartEndTimes(FixPoint start_time, FixPoint end_time)
        {
            m_strat_time = start_time;
            m_end_time = end_time;
            m_total_time = end_time - start_time;
            m_active = true;
        }

        public FixPoint GetRemaining(FixPoint current_time)
        {
            if (m_total_time == FixPoint.Zero)
                return FixPoint.Zero;
            FixPoint remain_time = m_end_time - current_time;
            if (remain_time < FixPoint.Zero)
                remain_time = FixPoint.Zero;
            return remain_time;
        }

        public bool Active()
        {
            return m_active;
        }
    }
}
