using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SkillDefinitionComponent : SkillComponent
    {
        //直接对目标生效
        public const int InflictType_Immediately = 1;
        //播放朝向目标的特效，一定延迟后，对目标生效
        public const int InflictType_TargetOrientedFx = 2;
        //发射朝向目标的必中飞行物，根据速度，一定延迟后，对目标生效（可以实现为CreateObjectSkillComponent）
        public const int InflictType_ObjectTrackingMissile = 3;
        //发射朝向指定位置的飞行物，根据速度，一定延迟后，对周围目标生效（可以实现为CreateObjectSkillComponent）
        public const int InflictType_PositionTrackingMissile = 4;
        ////发射独立运行的Object，这种情况变成了CreateObjectSkillComponent，并且其配置是InflictType_Immediately，表示立即发射物体
        //public const int InflictType_CreateObject = -1;

        #region 配置数据
        int m_mana_type = 0;
        Formula m_mana_cost = RecyclableObject.Create<Formula>();
        Formula m_min_range = RecyclableObject.Create<Formula>();  //配置小于等于0表示无限制
        Formula m_max_range = RecyclableObject.Create<Formula>();  //配置小于等于0表示无限制
        Formula m_cooldown_time = RecyclableObject.Create<Formula>();
        Formula m_casting_time = RecyclableObject.Create<Formula>();
        Formula m_inflict_time = RecyclableObject.Create<Formula>();
        Formula m_expiration_time = RecyclableObject.Create<Formula>();

        bool m_starts_active = false;
        bool m_blocks_other_skills_when_active = false;
        bool m_blocks_movement_when_active = true;
        bool m_deactivate_when_moving = true;
        bool m_can_activate_while_moving = true;
        bool m_can_activate_when_disabled = false;

        int m_target_gathering_type = 0;
        FixPoint m_target_gathering_param1;
        FixPoint m_target_gathering_param2;

        int m_inflict_type = 1;
        string m_inflict_missile;
        FixPoint m_inflict_missile_speed;
        FixPoint m_impact_delay;

        public string m_casting_animation;
        public string m_main_animation;
        public string m_expiration_animation;
        #endregion

        //运行数据
        List<SkillTimer> m_timers = new List<SkillTimer>();

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            if (m_mana_type == 0)
                m_mana_type = ManaComponent.DEFAULT_MANA_TYPE_ID;

            m_timers.Clear();
            for (int i = 0; i < SkillTimer.TimerCount; ++i)
            {
                SkillTimer timer = RecyclableObject.Create<SkillTimer>();
                m_timers.Add(timer);
            }
        }

        protected override void OnDestruct()
        {
            for (int i = 0; i < SkillTimer.TimerCount; ++i)
                RecyclableObject.Recycle(m_timers[i]);
            m_timers.Clear();

            RecyclableObject.Recycle(m_mana_cost);
            m_mana_cost = null;
            RecyclableObject.Recycle(m_min_range);
            m_min_range = null;
            RecyclableObject.Recycle(m_max_range);
            m_max_range = null;
            RecyclableObject.Recycle(m_cooldown_time);
            m_cooldown_time = null;
            RecyclableObject.Recycle(m_casting_time);
            m_casting_time = null;
            RecyclableObject.Recycle(m_inflict_time);
            m_inflict_time = null;
            RecyclableObject.Recycle(m_expiration_time);
            m_expiration_time = null;
        }
        #endregion

        #region 计时器
        public void ClearTimer(int skill_timer_type)
        {
            SkillTimer timer = m_timers[skill_timer_type];
            if (timer.Active)
                timer.Reset();
        }

        public SkillTimer GetTimer(int skill_timer_type)
        {
            return m_timers[skill_timer_type];
        }

        public bool IsTimerActive(int skill_timer_type)
        {
            return m_timers[skill_timer_type].Active;
        }

        public void StartCooldownTimer(FixPoint start_time)
        {
            SkillTimer timer = m_timers[SkillTimer.CooldownTimer];
            timer.Start(start_time, CooldownTime);
        }

        public void StartCastingTimer(FixPoint start_time)
        {
            SkillTimer timer = m_timers[SkillTimer.CastingTimer];
            timer.Start(start_time, CastingTime);
        }

        public void StartInflictingTimer(FixPoint start_time)
        {
            SkillTimer timer = m_timers[SkillTimer.InflictingTimer];
            timer.Start(start_time, InflictTime);
        }

        public void StartExpirationTimer(FixPoint start_time)
        {
            SkillTimer timer = m_timers[SkillTimer.ExpirationTimer];
            timer.Start(start_time, ExpirationTime);
        }

        public FixPoint GetLowestCountdownTimerRemaining()
        {
            FixPoint lowest_time = FixPoint.MaxValue;
            FixPoint current_time = GetCurrentTime();
            for (int i = 0; i < SkillTimer.TimerCount; ++i)
            {
                SkillTimer timer = m_timers[i];
                if(timer.Active)
                {
                    FixPoint time_left = timer.GetRemaining(current_time);
                    if (time_left < lowest_time)
                        lowest_time = time_left;
                }
            }
            if (lowest_time == FixPoint.MaxValue)
                return FixPoint.Zero;
            else
                return lowest_time;
        }

        public bool IsRecharging()
        {
            return IsTimerActive(SkillTimer.CooldownTimer);
        }

        public bool IsExpiring()
        {
            return IsTimerActive(SkillTimer.ExpirationTimer);
        }
        #endregion
    }
}
