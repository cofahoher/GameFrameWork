using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum CheckSkillResult
    {
        Success = 0,
        CoolDown,
        ManaNotEnough,
        RequireStillButMove,
        SkillDisabled,
    }
    public class Skill : Object
    {
        bool m_is_active = false;
        SkillManagerComponent m_skill_manager_cmp;
        SkillDefinitionComponent m_definition_cmp;
        EffectGeneratorSkillComponent m_effect_generator_cmp;
        WeaponSkillComponent m_weapon_cmp;
        LocomotorComponent m_locomotor_cmp;
        ManaComponent m_mana_cmp;

        List<Target> m_skill_targets = new List<Target>();

        SkillCountdownTask m_task;

        #region 初始化/销毁
        protected override void PreInitializeObject(ObjectCreationContext context)
        {
            Entity ownerEntity = context.m_logic_world.GetEntityManager().GetObject(m_context.m_object_id);
            if(ownerEntity != null)
            {
                m_skill_manager_cmp = ownerEntity.GetComponent<SkillManagerComponent>();
                m_locomotor_cmp = ownerEntity.GetComponent<LocomotorComponent>();
                m_mana_cmp = ownerEntity.GetComponent<ManaComponent>();
            }
        }

        protected override void PostInitializeObject(ObjectCreationContext context)
        {
            m_definition_cmp = this.GetComponent<SkillDefinitionComponent>();
            m_effect_generator_cmp = this.GetComponent<EffectGeneratorSkillComponent>();
            m_weapon_cmp = this.GetComponent<WeaponSkillComponent>();
        }

        protected override void OnDestruct()
        {
            m_is_active = false;
            m_skill_targets.Clear();
            m_skill_manager_cmp = null;
            m_definition_cmp = null;
            m_effect_generator_cmp = null;
            m_weapon_cmp = null;
            m_locomotor_cmp = null;
            m_mana_cmp = null;
            if (m_task != null)
            {
                m_task.Cancel();
                m_task = null;
            }
        }
        #endregion

        #region ILogicOwnerInfo
        public override Object GetOwnerObject()
        {
            return m_skill_manager_cmp.GetOwnerObject();
        }
        public override int GetOwnerPlayerID()
        {
            return m_skill_manager_cmp.GetOwnerPlayerID();
        }
        public override Player GetOwnerPlayer()
        {
            return m_skill_manager_cmp.GetOwnerPlayer();
        }
        public override int GetOwnerEntityID()
        {
            return m_skill_manager_cmp.GetOwnerEntityID();
        }
        public override Entity GetOwnerEntity()
        {
            return m_skill_manager_cmp.GetOwnerEntity();
        }
        #endregion

        #region 技能目标
        //for WeaponSkillComponent 默认武器技能
        public void SetTarget(Target target)
        {
            if(target != null)
            {
                m_skill_targets.Add(target);
            }
        }
        public Target GetTarget()
        {
            if (m_skill_targets.Count > 0)
                return m_skill_targets[0];
            return null;
        }

        //for EffectGeneratorSkillComponent 技能
        public void SetTargets(List<Target> targets)
        {
            if (targets != null)
                m_skill_targets = targets;
        }
        public List<Target> GetTargets()
        {
            return m_skill_targets;
        }

        private void ClearTargets()
        {
            for (int i = 0; i < m_skill_targets.Count; ++i)
            {
                RecyclableObject.Recycle(m_skill_targets[i]);
            }
            m_skill_targets.Clear();
        }
        #endregion

        #region 技能计时器
        private FixPoint GetLowestRemainingAmongActiveTimer()
        {
            return m_definition_cmp.GetLowestRemainingAmongActiveTimers();
        }

        private bool IsRecharging()
        {
            return m_definition_cmp.IsTimerActive(SkillTimerType.CooldownTimer);
        }

        public bool IsExpiring()
        {
            return m_definition_cmp.IsTimerActive(SkillTimerType.ExpirationTimer);
        }

        public bool IsReady()
        {
            return !IsRecharging();
        }

        public FixPoint GetNextReadyTime()
        {
            SkillTimer timer = m_definition_cmp.GetTimer(SkillTimerType.CooldownTimer);
            if(timer.Active)
            {
                return timer.GetRemaining(GetCurrentTime());
            }
            return FixPoint.Zero;
        }
        #endregion

        #region 技能的Activate流程
        /// <summary>
        /// 是否可以激活技能
        /// </summary>
        public bool CanActivate()
        {
            CheckSkillResult result = CheckActivate();

            if (CheckSkillResult.Success == result)
            {
                return true;
            }
            return false;
        }

        public bool BuildSkillTargets(Target ai_target = null)
        {
            if (ai_target != null)
                SetTarget(ai_target);
            //查找目标
            TargetGatheringManager target_gathering_manager = GetLogicWorld().GetTargetGatheringManager();
            List<Target> targets = target_gathering_manager.BuildTargetList(m_definition_cmp.TargetGatheringType,
                m_definition_cmp.TargetGatheringParam1, m_definition_cmp.TargetGatheringParam2, this, GetOwnerEntity());
            if (targets.Count >= m_definition_cmp.ExpectedTargetCount)
                SetTargets(targets);

            return false;
        }

        private CheckSkillResult CheckActivate()
        {
            if (!IsReady())
                return CheckSkillResult.CoolDown;

            if (!m_definition_cmp.CanActivateWhileMoving)
            {
                if (m_locomotor_cmp != null && m_locomotor_cmp.IsMoving)
                    return CheckSkillResult.RequireStillButMove;
            }

            FixPoint mana_cost = m_definition_cmp.ManaCost;
            if (mana_cost > FixPoint.Zero)
            {
                if (m_mana_cmp.GetCurrentManaPoint() < mana_cost)
                    return CheckSkillResult.ManaNotEnough;
            }

            if (!IsSkillEnable())
            {
                if (!m_definition_cmp.CanActivateWhenDisabled)
                    return CheckSkillResult.SkillDisabled;
            }

            return CheckSkillResult.Success;
        }

        private bool IsSkillEnable()
        {
            if (!m_skill_manager_cmp.CanActivateSkill())
                return false;
            if (m_effect_generator_cmp != null)
                return m_effect_generator_cmp.IsEnable();
            else if(m_weapon_cmp != null)
                return m_weapon_cmp.IsEnable();
            return false;
        }

        public bool Activate(FixPoint start_time)
        {
            SetSkillActive(true);
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SkillComponent cmp = enumerator.Current.Value as SkillComponent;
                cmp.Activate(start_time);
            }

            m_definition_cmp.ClearTimer(SkillTimerType.ExpirationTimer);

            if (m_definition_cmp.CastingTime > FixPoint.Zero)
                m_definition_cmp.StartCastingTimer(start_time);
            else
                PostActivate(start_time);
            ScheduleServiceCountdown();

            return true;
        }

        private void SetSkillActive(bool is_active)
        {
            bool pre_is_active = m_is_active;
            m_is_active = is_active;
            if (is_active)
            {
                if (!pre_is_active)
                    m_skill_manager_cmp.OnSkillActivated(this);
            }
            else
            {
                if (pre_is_active)
                    m_skill_manager_cmp.OnSkillDeactivated(this);
            }
        }

        public bool PostActivate(FixPoint start_time)
        {
            FixPoint mana_cost = m_definition_cmp.ManaCost;
            if (mana_cost > FixPoint.Zero)
            {
                m_mana_cmp.ChangeMana(-mana_cost);
            }
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SkillComponent cmp = enumerator.Current.Value as SkillComponent;
                cmp.PostActivate(start_time);
            }

            //开始CD
            m_definition_cmp.StartCooldownTimer(start_time);

            if (m_definition_cmp.ExpirationTime > FixPoint.Zero)
                m_definition_cmp.StartExpirationTimer(start_time);
            else
                Deactivate();

            return true;
        }

        public bool Deactivate()
        {
            if (!m_is_active)
                return false;

            m_definition_cmp.ClearTimer(SkillTimerType.CastingTimer);
            m_definition_cmp.ClearTimer(SkillTimerType.ExpirationTimer);
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SkillComponent cmp = enumerator.Current.Value as SkillComponent;
                cmp.Deactivate();
            }
            SetSkillActive(false);
            ClearTargets();

            return true;
        }

        //打断
        public void Interrupt()
        {
            if(Deactivate())
                ScheduleServiceCountdown();
        }
        #endregion

        #region 技能计时器Task相关
        private void ScheduleServiceCountdown()
        {
            FixPoint time_remaining = GetLowestRemainingAmongActiveTimer();
            if (time_remaining <= FixPoint.Zero)
                time_remaining = FixPoint.One;

            var task_scheduler = GetLogicWorld().GetTaskScheduler();
            if(m_task == null)
            {
                m_task = new SkillCountdownTask(this);
            }
            task_scheduler.Schedule(m_task, GetCurrentTime(), time_remaining);

        }
        public void ServiceCountdown(FixPoint current_time, FixPoint delta_time)
        {
            SkillTimer casting_timer = m_definition_cmp.GetTimer(SkillTimerType.CastingTimer);
            if(casting_timer.Active)
            {
                if(casting_timer.GetRemaining(current_time) == FixPoint.Zero)
                {
                    casting_timer.Reset();
                    PostActivate(current_time);
                }
                ScheduleServiceCountdown();
            }
            else
            {
                bool reschedule = false;
                //recharging
                SkillTimer cooldown_timer = m_definition_cmp.GetTimer(SkillTimerType.CooldownTimer);
                if(cooldown_timer.Active)
                {
                    if (cooldown_timer.GetRemaining(current_time) == FixPoint.Zero)
                    {
                        cooldown_timer.Reset();
                        NotifySkillReady();
                    }
                    else
                        reschedule = true;
                }
                //expiring
                SkillTimer expiration_timer = m_definition_cmp.GetTimer(SkillTimerType.ExpirationTimer);
                if(expiration_timer.Active)
                {
                    if (expiration_timer.GetRemaining(current_time) == FixPoint.Zero)
                    {
                        expiration_timer.Reset();
                        NotifySkillExpired();
                    }
                    else
                        reschedule = true;
                }

                if (reschedule)
                    ScheduleServiceCountdown();
            }
        }

        //通知CD结束
        private void NotifySkillReady()
        {

        }

        //通知引导结束
        private void NotifySkillExpired()
        {
            Deactivate();
        }
        #endregion

        #region Getter
        public SkillDefinitionComponent GetSkillDefinitionComponent()
        {
            return m_definition_cmp;
        }
        #endregion
    }

    //******************************************************************
    //技能倒计时的Task
    //******************************************************************
    class SkillCountdownTask : Task<LogicWorld>
    {
        Skill m_skill;
        public SkillCountdownTask(Skill skill)
        {
            m_skill = skill;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            m_skill.ServiceCountdown(current_time, delta_time);
        }
    }
}
