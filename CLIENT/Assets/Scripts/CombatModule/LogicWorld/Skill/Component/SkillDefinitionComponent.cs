using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SkillDefinitionComponent : SkillComponent
    {
        //A、技能额外数据
        public static readonly int NeedExternalDirection = (int)CRC.Calculate("direction");
        public static readonly int NeedExternalOffset = (int)CRC.Calculate("offset");
        public static readonly int NeedExternalTarget = (int)CRC.Calculate("target");
        //B、在没有额外数据时的自动朝向
        public static readonly int AutoFaceNearestEnemy = (int)CRC.Calculate("nearest_enemy");
        //C、技能施放提示表现上的自动瞄准，默认的A的数据
        public static readonly int AutoAimNearestEnemy = (int)CRC.Calculate("nearest_enemy");

        ////直接对目标生效
        //public const int InflictType_Immediately = 1;
        ////播放朝向目标的特效，一定延迟后，对目标生效
        //public const int InflictType_TargetOrientedFx = 2;
        ////发射朝向目标的必中飞行物，根据速度，一定延迟后，对目标生效（可以实现为CreateObjectSkillComponent）
        //public const int InflictType_ObjectTrackingMissile = 3;
        ////发射朝向指定位置的飞行物，根据速度，一定延迟后，对周围目标生效（可以实现为CreateObjectSkillComponent）
        //public const int InflictType_PositionTrackingMissile = 4;
        //////发射独立运行的Object，这种情况变成了CreateObjectSkillComponent，并且其配置是InflictType_Immediately，表示立即发射物体
        ////public const int InflictType_CreateObject = -1;

        #region 配置数据
        int m_mana_type = 0;
        Formula m_mana_cost = RecyclableObject.Create<Formula>();

        Formula m_min_range = RecyclableObject.Create<Formula>();  //配置小于等于0表示无限制
        Formula m_max_range = RecyclableObject.Create<Formula>();  //配置小于等于0表示无限制

        Formula m_cooldown_time = RecyclableObject.Create<Formula>();
        Formula m_casting_time = RecyclableObject.Create<Formula>();
        Formula m_inflict_time = RecyclableObject.Create<Formula>();
        Formula m_expiration_time = RecyclableObject.Create<Formula>();

        bool m_normal_attack = false;
        bool m_starts_active = false;
        bool m_blocks_other_skills_when_active = true;
        bool m_blocks_movement_when_active = true;
        bool m_deactivate_when_moving = true;
        bool m_can_activate_while_moving = true;
        bool m_can_activate_when_disabled = false;

        public TargetGatheringParam m_target_gathering_param = new TargetGatheringParam();
        bool m_need_gather_targets = true;
        int m_targets_min_count_for_activate = 0;

        int m_external_data_type = 0;
        int m_auto_face_type = 0;

        //int m_inflict_type = 1;
        //string m_inflict_missile;
        //FixPoint m_inflict_missile_speed;
        //FixPoint m_impact_delay;

        //瞄准的参数
        //NeedExternalDirection：长、宽
        //NeedExternalOffset：大圆半径，小圆半径
        FixPoint m_aim_param1 = FixPoint.Zero;
        FixPoint m_aim_param2 = FixPoint.Zero;

        //以下纯表现
        public int m_auto_aim_type = 0;
        public string m_icon;
        public string m_casting_animation;
        public string m_main_animation;
        public string m_expiration_animation;
        public int m_main_render_effect_cfgid = 0;
        public int m_main_sound = 0;
        #endregion

        //运行数据
        List<SkillTimer> m_timers = new List<SkillTimer>();
        Vector3FP m_external_vector;
        int m_specified_target_id = 0;


        #region GETTER
        public Vector3FP ExternalVector
        {
            get { return m_external_vector; }
            set { m_external_vector = value; }
        }

        public int SpecifiedTargetID
        {
            get { return m_specified_target_id; }
            set { m_specified_target_id = value; }
        }


        public static readonly FixPoint MIN_ESTIMATE_TIME = FixPoint.Two / FixPoint.Ten;
        public static readonly FixPoint MAX_ESTIMATE_TIME = FixPoint.One;
        public FixPoint GetEstimateBlockMovementTime()
        {
            if (m_blocks_movement_when_active)
            {
                FixPoint time = CastingTime;
                if (time > FixPoint.Zero)
                    return FixPoint.Max(MIN_ESTIMATE_TIME, time);
                time = InflictTime;
                if (time > FixPoint.Zero)
                    return FixPoint.Max(MIN_ESTIMATE_TIME, time);
                time = ExpirationTime;
                if (time > FixPoint.Zero)
                    return FixPoint.Min(MAX_ESTIMATE_TIME, time);
            }
            else
            {
                if (!m_deactivate_when_moving && m_main_animation == null)
                    return FixPoint.Zero;
                else
                    return MIN_ESTIMATE_TIME;
            }
            return MIN_ESTIMATE_TIME;
        }
        #endregion

        #region 初始化/销毁
        public SkillDefinitionComponent()
        {
            m_target_gathering_param.m_faction = FactionRelation.Enemy;
        }

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
