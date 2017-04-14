using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SkillDefinitionComponent : SkillComponent
    {
        #region 配置数据
        //技能消耗
        Formula m_mana_cost_formula = RecyclableObject.Create<Formula>();
        //技能CD,从PostActivate开始计算
        Formula m_cooldown_time_formula = RecyclableObject.Create<Formula>();
        //从施法到生效的时间，可以用作动作开始做到动作打中人的时间
        Formula m_casting_time_formula = RecyclableObject.Create<Formula>();
        //生效后的最长引导时间
        Formula m_expiration_time_formula = RecyclableObject.Create<Formula>();
        //是否获得时就activate（适合做被动技能）
        bool m_starts_active = false;
        //是否在activate后deactivate前，阻止其他skill
        bool m_blocks_other_skills_when_active = false;
        //是否activate时停止移动
        bool m_blocks_movement_when_active = false;
        //是否在移动时deactivate
        bool m_deactivate_when_moving = false;
        //是否可以在移动时activate
        bool m_can_activate_while_moving = true;
        //是否可以在不能activate的时候（如眩晕时）activate
        bool m_can_activate_when_disabled = false;
        //技能释放期望目标数量，默认找所有
        int m_expected_target_count = -1;
        //AI技能释放期望目标数量，默认找所有
        int m_ai_expected_target_count = -1;
        //目标查找类型
        int m_target_gathering_type = -1;
        //目标查找参数
        FixPoint m_target_gathering_param1;
        FixPoint m_target_gathering_param2;
        //技能优先级（1为最高）
        int m_priority = 1;
        //表现 - 技能描述
        string m_skill_desc;
        //表现 - 施法者的动作资源
        string m_animation_res;
        //表现 - 施法者的特效资源
        string m_ps_res;
        //表现 - 施法者的特效播放延迟时间
        FixPoint m_ps_delay = FixPoint.Zero;
        #endregion

        #region 运行数据
        //各种技能时间器
        List<SkillTimer> m_timers = new List<SkillTimer>();
        #endregion

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            m_timers.Clear();
            for (int i = 0; i < (int)SkillTimerType.TimerCount; ++i)
            {
                SkillTimer timer = RecyclableObject.Create<SkillTimer>();
                m_timers.Add(timer);
            }
        }

        protected override void OnDestruct()
        {
            for (int i = 0; i < (int)SkillTimerType.TimerCount; ++i)
            {
                RecyclableObject.Recycle(m_timers[i]);
            }
            m_timers.Clear();

            RecyclableObject.Recycle(m_mana_cost_formula);
            m_mana_cost_formula = null;
            RecyclableObject.Recycle(m_cooldown_time_formula);
            m_cooldown_time_formula = null;
            RecyclableObject.Recycle(m_casting_time_formula);
            m_casting_time_formula = null;
            RecyclableObject.Recycle(m_expiration_time_formula);
            m_expiration_time_formula = null;
        }
        #endregion

        #region 计时器
        public void ClearTimer(SkillTimerType skill_timer_type)
        {
            SkillTimer timer = m_timers[(int)skill_timer_type];
            if (timer.Active)
                timer.Reset();
        }

        public SkillTimer GetTimer(SkillTimerType skill_timer_type)
        {
            return m_timers[(int)skill_timer_type];
        }

        public bool IsTimerActive(SkillTimerType skill_timer_type)
        {
            return m_timers[(int)skill_timer_type].Active;
        }

        public void StartCastingTimer(FixPoint start_time)
        {
            SkillTimer timer = m_timers[(int)SkillTimerType.CastingTimer];
            timer.SetStartTotalTimes(start_time, CastingTime);
        }

        public void StartCooldownTimer(FixPoint start_time)
        {
            SkillTimer timer = m_timers[(int)SkillTimerType.CooldownTimer];
            timer.SetStartTotalTimes(start_time, CooldownTime);
        }

        public void StartExpirationTimer(FixPoint start_time)
        {
            SkillTimer timer = m_timers[(int)SkillTimerType.ExpirationTimer];
            timer.SetStartTotalTimes(start_time, ExpirationTime);
        }

        public FixPoint GetLowestRemainingAmongActiveTimers()
        {
            //返回激活的Timer中最少的剩余时间，如果没有Timer激活，那就返回0
            FixPoint lowest_time = FixPoint.MinusOne;
            FixPoint current_time = GetCurrentTime();
            for(int i = 0; i <　(int)SkillTimerType.TimerCount; ++i)
            {
                SkillTimer timer = m_timers[i];
                if(timer.Active)
                {
                    FixPoint time_left = timer.GetRemaining(current_time);
                    if (lowest_time == FixPoint.MinusOne || time_left < lowest_time)
                        lowest_time = time_left;
                }
            }
            return lowest_time;
        }
        #endregion

        #region 技能的Activate流程
        public override void Activate(FixPoint start_time)
        {
            //可以在这里处理技能开始的渲染事件 播动作、特效 yqqtodo
        }
        #endregion
    }
}
