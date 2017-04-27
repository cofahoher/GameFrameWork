using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum CastSkillResult
    {
        Success = 0,
        InCooldown,
        NotEnoughMana,
        ObjectIsMoving,
        SkillDisabled,
    }

    public class Skill : Object
    {
        bool m_is_active = false;
        SkillManagerComponent m_owner_component;
        SkillDefinitionComponent m_definition_component;
        ManaComponent m_mana_component;
        SkillCountdownTask m_task;
        List<Target> m_skill_targets = new List<Target>();

        #region GETTER
        public SkillDefinitionComponent GetSkillDefinitionComponent()
        {
            return m_definition_component;
        }
        #endregion

        #region 初始化/销毁
        protected override void PreInitializeObject(ObjectCreationContext context)
        {
            Entity ownerEntity = context.m_logic_world.GetEntityManager().GetObject(m_context.m_owner_id);
            if(ownerEntity != null)
            {
                m_owner_component = ownerEntity.GetComponent<SkillManagerComponent>();
                m_mana_component = ownerEntity.GetComponent<ManaComponent>();
            }
        }

        protected override void PostInitializeObject(ObjectCreationContext context)
        {
            m_definition_component = this.GetComponent<SkillDefinitionComponent>();
        }

        protected override void OnDestruct()
        {
            m_owner_component = null;
            m_definition_component = null;
            m_mana_component = null;
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
            ClearTargets();
        }
        #endregion

        #region ILogicOwnerInfo
        public override Object GetOwnerObject()
        {
            return m_owner_component.GetOwnerObject();
        }
        public override int GetOwnerPlayerID()
        {
            return m_owner_component.GetOwnerPlayerID();
        }
        public override Player GetOwnerPlayer()
        {
            return m_owner_component.GetOwnerPlayer();
        }
        public override int GetOwnerEntityID()
        {
            return m_owner_component.GetOwnerEntityID();
        }
        public override Entity GetOwnerEntity()
        {
            return m_owner_component.GetOwnerEntity();
        }
        #endregion

        #region 技能目标
        public void BuildSkillTargets()
        {
            TargetGatheringManager target_gathering_manager = GetLogicWorld().GetTargetGatheringManager();
            target_gathering_manager.BuildTargetList(m_definition_component.TargetGatheringType, m_definition_component.TargetGatheringParam1, m_definition_component.TargetGatheringParam2,
                this, GetOwnerEntity(), m_skill_targets);
        }

        public void AddTarget(Target target)
        {
            if(target != null)
            {
                m_skill_targets.Add(target);
            }
        }

        public Target GetMajorTarget()
        {
            if (m_skill_targets.Count > 0)
                return m_skill_targets[0];
            return null;
        }

        public List<Target> GetTargets()
        {
            return m_skill_targets;
        }

        private void ClearTargets()
        {
            for (int i = 0; i < m_skill_targets.Count; ++i)
                RecyclableObject.Recycle(m_skill_targets[i]);
            m_skill_targets.Clear();
        }
        #endregion

        #region 技能流程
        public bool CanActivate()
        {
            return CheckActivate() == CastSkillResult.Success;
        }

        public CastSkillResult CheckActivate()
        {
            if (!IsReady())
                return CastSkillResult.InCooldown;

            if (!m_owner_component.CanActivateSkill())
            {
                if (!m_definition_component.CanActivateWhenDisabled)
                    return CastSkillResult.SkillDisabled;
            }

            FixPoint mana_cost = m_definition_component.ManaCost;
            if (mana_cost > FixPoint.Zero)
            {
                if (m_mana_component.GetCurrentManaPoint(m_definition_component.ManaType) < mana_cost)
                    return CastSkillResult.NotEnoughMana;
            }

            if (!m_definition_component.CanActivateWhileMoving)
            {
                LocomotorComponent locomotor_cmp = GetOwnerEntity().GetComponent(LocomotorComponent.ID) as LocomotorComponent;
                if (locomotor_cmp != null && locomotor_cmp.IsMoving)
                    return CastSkillResult.ObjectIsMoving;
            }

            return CastSkillResult.Success;
        }

        public bool Activate(FixPoint start_time)
        {
            Deactivate();
            SetSkillActive(true);

            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SkillComponent cmp = enumerator.Current.Value as SkillComponent;
                if (cmp != null)
                    cmp.Activate(start_time);
            }

            m_definition_component.ClearTimer(SkillTimer.ExpirationTimer);
            if (m_definition_component.CastingTime > FixPoint.Zero)
                m_definition_component.StartCastingTimer(start_time);
            else
                PostActivate(start_time);

            ScheduleTimers();
            return true;
        }

        public bool PostActivate(FixPoint start_time)
        {
            FixPoint mana_cost = m_definition_component.ManaCost;
            if (mana_cost > FixPoint.Zero)
            {
                m_mana_component.ChangeMana(m_definition_component.ManaType, -mana_cost);
            }

            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SkillComponent cmp = enumerator.Current.Value as SkillComponent;
                if (cmp != null)
                    cmp.PostActivate(start_time);
            }

            m_definition_component.StartCooldownTimer(start_time);

            if (m_definition_component.InflictTime > FixPoint.Zero)
                m_definition_component.StartInflictingTimer(start_time);
            else
                Inflict(start_time);
            return true;
        }

        public bool Inflict(FixPoint start_time)
        {
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SkillComponent cmp = enumerator.Current.Value as SkillComponent;
                if (cmp != null)
                    cmp.Inflict(start_time);
            }

            if (m_definition_component.ExpirationTime > FixPoint.Zero)
                m_definition_component.StartExpirationTimer(start_time);
            else
                Deactivate();
            return true;
        }

        public bool Deactivate()
        {
            if (!m_is_active)
                return false;

            m_definition_component.ClearTimer(SkillTimer.CastingTimer);
            m_definition_component.ClearTimer(SkillTimer.InflictingTimer);
            m_definition_component.ClearTimer(SkillTimer.ExpirationTimer);

            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SkillComponent cmp = enumerator.Current.Value as SkillComponent;
                if (cmp != null)
                    cmp.Deactivate();
            }

            SetSkillActive(false);
            ClearTargets();
            return true;
        }

        public void Interrupt()
        {
            if (Deactivate())
                ScheduleTimers();
        }

        void SetSkillActive(bool is_active)
        {
            bool pre_is_active = m_is_active;
            m_is_active = is_active;
            if (m_is_active != pre_is_active)
            {
                if (is_active)
                    m_owner_component.OnSkillActivated(this);
                else
                    m_owner_component.OnSkillDeactivated(this);
            }
        }
        #endregion

        #region 技能计时器
        public bool IsReady()
        {
            return !m_definition_component.IsRecharging();
        }

        public FixPoint GetNextReadyTime()
        {
            SkillTimer timer = m_definition_component.GetTimer(SkillTimer.CooldownTimer);
            if (timer.Active)
                return timer.GetRemaining(GetCurrentTime());
            return FixPoint.Zero;
        }

        private void ScheduleTimers()
        {
            FixPoint time_remaining = m_definition_component.GetLowestCountdownTimerRemaining();
            if (time_remaining <= FixPoint.Zero)
                time_remaining = FixPoint.PrecisionFP;
            var task_scheduler = GetLogicWorld().GetTaskScheduler();
            if(m_task == null)
            {
                m_task = LogicTask.Create<SkillCountdownTask>();
                m_task.Construct(this);
            }
            task_scheduler.Schedule(m_task, GetCurrentTime(), time_remaining);
        }

        public void ServiceCountdownTimers(FixPoint current_time, FixPoint delta_time)
        {
            bool reschedule = false;

            SkillTimer casting_timer = m_definition_component.GetTimer(SkillTimer.CastingTimer);
            if (casting_timer.Active)
            {
                if (casting_timer.GetRemaining(current_time) == FixPoint.Zero)
                {
                    casting_timer.Reset();
                    PostActivate(current_time);
                }
                reschedule = true;
            }

            SkillTimer inflicting_timer = m_definition_component.GetTimer(SkillTimer.InflictingTimer);
            if (inflicting_timer.Active)
            {
                if (inflicting_timer.GetRemaining(current_time) == FixPoint.Zero)
                {
                    inflicting_timer.Reset();
                    Inflict(current_time);
                }
                reschedule = true;
            }

            SkillTimer expiration_timer = m_definition_component.GetTimer(SkillTimer.ExpirationTimer);
            if (expiration_timer.Active)
            {
                if (expiration_timer.GetRemaining(current_time) == FixPoint.Zero)
                {
                    expiration_timer.Reset();
                    Deactivate();
                    NotifySkillExpired();
                }
                else
                {
                    reschedule = true;
                }
            }

            SkillTimer cooldown_timer = m_definition_component.GetTimer(SkillTimer.CooldownTimer);
            if (cooldown_timer.Active)
            {
                if (cooldown_timer.GetRemaining(current_time) == FixPoint.Zero)
                {
                    cooldown_timer.Reset();
                    NotifySkillReady();
                }
                else
                {
                    reschedule = true;
                }
            }

            if (reschedule)
                ScheduleTimers();
        }

        void NotifySkillReady()
        {
        }

        void NotifySkillExpired()
        {
        }
        #endregion

    }

    class SkillCountdownTask : Task<LogicWorld>
    {
        Skill m_skill;

        public void Construct(Skill skill)
        {
            m_skill = skill;
        }
        public override void OnReset()
        {
            m_skill = null;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            m_skill.ServiceCountdownTimers(current_time, delta_time);
        }
    }
}
