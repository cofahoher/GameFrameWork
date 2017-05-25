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
        TargetTooNear,
        TargetTooFar,
    }

    public class Skill : Object
    {
        SkillManagerComponent m_owner_component;
        ManaComponent m_mana_component;
        SkillDefinitionComponent m_definition_component;
        SkillCountdownTask m_task;
        bool m_is_active = false;
        List<Target> m_skill_targets = new List<Target>();

        #region GETTER
        public SkillDefinitionComponent GetDefinitionComponent()
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
                m_owner_component = ownerEntity.GetComponent(SkillManagerComponent.ID) as SkillManagerComponent;
                m_mana_component = ownerEntity.GetComponent(ManaComponent.ID) as ManaComponent;
            }
        }

        protected override void PostInitializeObject(ObjectCreationContext context)
        {
            m_definition_component = GetComponent(SkillDefinitionComponent.ID) as SkillDefinitionComponent;
        }

        protected override void OnDestruct()
        {
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
            if (m_is_active)
                Deactivate();
            m_owner_component = null;
            m_mana_component = null;
            m_definition_component = null;
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
            ClearTargets();
            TargetGatheringManager target_gathering_manager = GetLogicWorld().GetTargetGatheringManager();
            target_gathering_manager.BuildTargetList(GetOwnerEntity(), m_definition_component.TargetGatheringID, m_definition_component.TargetGatheringParam1, m_definition_component.TargetGatheringParam2, m_definition_component.TargetGatheringFation, m_skill_targets);
        }

        public void BuildSkillTargets(int gathering_type, FixPoint gathering_param1, FixPoint gathering_param2, int gathering_faction)
        {
            ClearTargets();
            TargetGatheringManager target_gathering_manager = GetLogicWorld().GetTargetGatheringManager();
            target_gathering_manager.BuildTargetList(GetOwnerEntity(), gathering_type, gathering_param1, gathering_param2, gathering_faction, m_skill_targets);
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

        void ClearTargets()
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

        public CastSkillResult CheckTargetRange(Entity entity)
        {
            FixPoint min_range = m_definition_component.MinRange;
            FixPoint max_range = m_definition_component.MaxRange;
            if (min_range <= FixPoint.Zero && max_range <= FixPoint.Zero)
                return CastSkillResult.Success;
            PositionComponent target_position_cmp = entity.GetComponent(PositionComponent.ID) as PositionComponent;
            PositionComponent source_position_cmp = GetOwnerEntity().GetComponent(PositionComponent.ID) as PositionComponent;
            FixPoint distance = source_position_cmp.CurrentPosition.FastDistance(target_position_cmp.CurrentPosition) - target_position_cmp.Radius - source_position_cmp.Radius;  //ZZWTODO 多处距离计算
            if (min_range > 0 && distance < min_range)
                return CastSkillResult.TargetTooNear;
            if (max_range > 0 && distance > max_range)
                return CastSkillResult.TargetTooFar;
            return CastSkillResult.Success;
        }

        bool CheckTargetRange()
        {
            Target target = GetMajorTarget();
            if (target == null)
                return true;
            Entity entity = target.GetEntity();
            if (entity == null)
                return true;
            return CheckTargetRange(entity) == CastSkillResult.Success;
        }

        void AdjustDirection()
        {
            int external_data_type = m_definition_component.ExternalDataType;
            if (external_data_type == 0)
                return;
            Vector3FP vector = m_definition_component.ExternalVector;
            if (vector.IsAllZero())
                return;
            PositionComponent position_component = GetOwnerEntity().GetComponent(PositionComponent.ID) as PositionComponent;
            if (position_component == null)
                return;
            if (external_data_type == SkillDefinitionComponent.NeedExternalDirection)
            {
                position_component.SetAngle(FixPoint.XZToUnityRotationDegree(vector.x, vector.z));
            }
            else if (external_data_type == SkillDefinitionComponent.NeedExternalOffset)
            {
                position_component.SetAngle(FixPoint.XZToUnityRotationDegree(vector.x, vector.z));
            }
        }

        public bool Activate(FixPoint start_time)
        {
            if (!CanActivate())
                return false;

            Deactivate();
            SetSkillActive(true);

            AdjustDirection();

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

#if COMBAT_CLIENT
            if (m_definition_component.m_casting_animation != null)
            {
                PlayAnimationRenderMessage msg = RenderMessage.Create<PlayAnimationRenderMessage>();
                msg.Construct(GetOwnerEntityID(), m_definition_component.m_casting_animation, null, true);
                GetLogicWorld().AddRenderMessage(msg);
            }
#endif
            return true;
        }

        public bool PostActivate(FixPoint start_time)
        {
            if (m_definition_component.NeedGatherTargets)
            {
                BuildSkillTargets();
                if (!CheckTargetRange())
                {
                    Deactivate();
                    return false;
                }
            }

            FixPoint mana_cost = m_definition_component.ManaCost;
            if (mana_cost > FixPoint.Zero)
            {
                if (!m_mana_component.ChangeMana(m_definition_component.ManaType, -mana_cost))
                {
                    Deactivate();
                    return false;
                }
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

#if COMBAT_CLIENT
            if (m_definition_component.m_main_animation != null)
            {
                PlayAnimationRenderMessage msg = RenderMessage.Create<PlayAnimationRenderMessage>();
                if (m_definition_component.m_expiration_animation == null)
                    msg.Construct(GetOwnerEntityID(), m_definition_component.m_main_animation);
                else
                    msg.Construct(GetOwnerEntityID(), m_definition_component.m_main_animation, m_definition_component.m_expiration_animation, true);
                GetLogicWorld().AddRenderMessage(msg);
            }
#endif
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
#if COMBAT_CLIENT
            PlayAnimationRenderMessage msg = RenderMessage.Create<PlayAnimationRenderMessage>();
            msg.Construct(GetOwnerEntityID(), AnimationName.IDLE, null, true);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }
        #endregion

        #region Variable
        //ZZW no need
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
