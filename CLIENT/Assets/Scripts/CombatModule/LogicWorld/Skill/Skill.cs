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
        CheatedExternalData,
        NotEnoughTargets,
        TargetTooNear,
        TargetTooFar,
        ComponnetUnavailable,
    }

    public class Skill : Object, ISkill
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
            Entity owner_entity = context.m_logic_world.GetEntityManager().GetObject(m_context.m_owner_id);
            if (owner_entity != null)
            {
                m_owner_component = owner_entity.GetComponent(SkillManagerComponent.ID) as SkillManagerComponent;
                m_mana_component = owner_entity.GetComponent(ManaComponent.ID) as ManaComponent;
            }
        }

        protected override void PostInitializeObject(ObjectCreationContext context)
        {
            m_definition_component = GetComponent(SkillDefinitionComponent.ID) as SkillDefinitionComponent;
        }

        protected override void PreDestruct()
        {
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
            if (m_is_active)
                Deactivate(true);
        }

        protected override void OnDestruct()
        {
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
            if (m_definition_component.m_target_gathering_param.m_type == TargetGatheringType.SpecifiedTarget)
            {
                if (m_definition_component.ExternalDataType == SkillDefinitionComponent.NeedExternalTarget)
                {
                    int specified_target_id = m_definition_component.SpecifiedTargetID;
                    if (specified_target_id > 0)
                    {
                        Target target = RecyclableObject.Create<Target>();
                        target.Construct();
                        target.SetEntityTarget(specified_target_id);
                        m_skill_targets.Add(target);
                    }
                }
                else if (m_definition_component.ExternalDataType == SkillDefinitionComponent.NeedExternalOffset)
                {
                    PositionComponent position_component = GetOwnerEntity().GetComponent(PositionComponent.ID) as PositionComponent;
                    Target target = RecyclableObject.Create<Target>();
                    target.Construct();
                    target.SetPositionTarget(position_component.CurrentPosition + m_definition_component.ExternalVector);
                    m_skill_targets.Add(target);
                }
            }
            else
            {
                TargetGatheringManager target_gathering_manager = GetLogicWorld().GetTargetGatheringManager();
                target_gathering_manager.BuildTargetList(GetOwnerEntity(), m_definition_component.m_target_gathering_param, m_skill_targets);
            }
        }

        public void BuildSkillTargets(TargetGatheringParam target_gathering_param)
        {
            ClearTargets();
            TargetGatheringManager target_gathering_manager = GetLogicWorld().GetTargetGatheringManager();
            target_gathering_manager.BuildTargetList(GetOwnerEntity(), target_gathering_param, m_skill_targets);
        }

        public void AddTarget(Target target)
        {
            if (target != null)
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

        CastSkillResult CheckActivate()
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

            if (m_definition_component.ExternalDataType == SkillDefinitionComponent.NeedExternalOffset)
            {
                if (m_definition_component.ExternalVector.Length() > m_definition_component.AimParam1)
                    return CastSkillResult.CheatedExternalData;
            }

            if (m_definition_component.TargetsMinCountForActivate > 0)
            {
                BuildSkillTargets();
                //ZZWTODO clear targets?
                if (m_skill_targets.Count < m_definition_component.TargetsMinCountForActivate)
                    return CastSkillResult.NotEnoughTargets;
            }

            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SkillComponent cmp = enumerator.Current.Value as SkillComponent;
                if (cmp != null && !cmp.CanActivate())
                    return CastSkillResult.ComponnetUnavailable;
            }

            return CastSkillResult.Success;
        }

        CastSkillResult CheckTargetRange(Entity entity)
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
            Entity entity = target.GetEntity(GetLogicWorld());
            if (entity == null)
                return true;
            return CheckTargetRange(entity) == CastSkillResult.Success;
        }

        void AdjustDirection()
        {
            int external_data_type = m_definition_component.ExternalDataType;

            if (external_data_type == SkillDefinitionComponent.NeedExternalTarget)
            {
                Entity target = GetLogicWorld().GetEntityManager().GetObject(m_definition_component.SpecifiedTargetID);
                if (target != null)
                {
                    PositionComponent owner_position_component = GetOwnerEntity().GetComponent(PositionComponent.ID) as PositionComponent;
                    PositionComponent target_position_component = target.GetComponent(PositionComponent.ID) as PositionComponent;
                    owner_position_component.SetFacing(target_position_component.CurrentPosition - owner_position_component.CurrentPosition);
                }
            }
            else if (external_data_type == SkillDefinitionComponent.NeedExternalDirection || external_data_type == SkillDefinitionComponent.NeedExternalOffset)
            {
                Vector3FP vector = m_definition_component.ExternalVector;
                if (vector.IsAllZero())
                    return;
                PositionComponent position_component = GetOwnerEntity().GetComponent(PositionComponent.ID) as PositionComponent;
                if (position_component == null)
                    return;
                position_component.SetFacing(vector);
            }
            else
            {
                Vector3FP vector = m_definition_component.ExternalVector;
                if (!vector.IsAllZero())
                {
                    Entity owner_entity = GetOwnerEntity();
                    PositionComponent owner_pos_cmp = owner_entity.GetComponent(PositionComponent.ID) as PositionComponent;
                    owner_pos_cmp.SetFacing(vector);
                }
                else
                {
                    int auto_face = m_definition_component.AutoFaceType;
                    if (auto_face == SkillDefinitionComponent.AutoFaceNearestEnemy)
                    {
                        Entity owner_entity = GetOwnerEntity();
                        PositionComponent target_pos_cmp = GetLogicWorld().GetTargetGatheringManager().GetNearestEnemy(owner_entity);
                        if (target_pos_cmp == null)
                            return;
                        PositionComponent owner_pos_cmp = owner_entity.GetComponent(PositionComponent.ID) as PositionComponent;
                        Vector3FP offset = target_pos_cmp.CurrentPosition - owner_pos_cmp.CurrentPosition;
                        owner_pos_cmp.SetFacing(offset);
                    }
                }
            }
        }

        public bool Activate(FixPoint start_time)
        {
            if (!CanActivate())
                return false;

            Deactivate(false);

            AdjustDirection();
            
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
                m_definition_component.StartCastingTimer(start_time, m_owner_component);
            else
                PostActivate(start_time);

            ScheduleTimers();

#if COMBAT_CLIENT
            if (m_definition_component.m_casting_animation != null)
            {
                PlayAnimationRenderMessage msg = RenderMessage.Create<PlayAnimationRenderMessage>();
                FixPoint speed = m_definition_component.NormalAttack ? m_owner_component.AttackSpeedRate : FixPoint.One;
                msg.Construct(GetOwnerEntityID(), m_definition_component.m_casting_animation, null, true, speed);
                GetLogicWorld().AddRenderMessage(msg);
            }
#endif
            return true;
        }

        bool PostActivate(FixPoint start_time)
        {
            if (m_definition_component.NeedGatherTargets)
            {
                BuildSkillTargets();
                if (!CheckTargetRange())
                {
                    Deactivate(false);
                    return false;
                }
            }

            FixPoint mana_cost = m_definition_component.ManaCost;
            if (mana_cost > FixPoint.Zero)
            {
                if (!m_mana_component.ChangeMana(m_definition_component.ManaType, -mana_cost))
                {
                    Deactivate(false);
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

            m_definition_component.StartCooldownTimer(start_time, m_owner_component);

            if (m_definition_component.InflictTime > FixPoint.Zero)
                m_definition_component.StartInflictingTimer(start_time, m_owner_component);
            else
                Inflict(start_time);

            if (m_definition_component.NormalAttack)
                GetOwnerEntity().SendSignal(SignalType.CastNormalAttack);
            else if (!m_definition_component.StartsActive)
                GetOwnerEntity().SendSignal(SignalType.CastSkill);

            string main_animation = m_definition_component.GenerateMainActionNameLogicly();
#if COMBAT_CLIENT
            if (main_animation != null)
            {
                PlayAnimationRenderMessage msg = RenderMessage.Create<PlayAnimationRenderMessage>();
                FixPoint speed = m_definition_component.NormalAttack ? m_owner_component.AttackSpeedRate : FixPoint.One;
                if (m_definition_component.m_expiration_animation == null)
                    msg.Construct(GetOwnerEntityID(), main_animation, AnimationName.IDLE, true, speed);
                else
                    msg.Construct(GetOwnerEntityID(), main_animation, m_definition_component.m_expiration_animation, true, speed);
                GetLogicWorld().AddRenderMessage(msg);
            }
            if (m_definition_component.m_main_render_effect_cfgid > 0)
            {
                PlayRenderEffectMessage msg = RenderMessage.Create<PlayRenderEffectMessage>();
                msg.ConstructAsPlay(GetOwnerEntityID(), m_definition_component.m_main_render_effect_cfgid, FixPoint.MinusOne);
                GetLogicWorld().AddRenderMessage(msg);
            }
            if (m_definition_component.m_main_sound > 0)
            {
                PlaySoundMessage msg = RenderMessage.Create<PlaySoundMessage>();
                msg.Construct(GetOwnerEntityID(), m_definition_component.m_main_sound, FixPoint.MinusOne);
                GetLogicWorld().AddRenderMessage(msg);
            }
#endif
            return true;
        }

        bool Inflict(FixPoint start_time)
        {
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SkillComponent cmp = enumerator.Current.Value as SkillComponent;
                if (cmp != null)
                    cmp.Inflict(start_time);
            }

            if (m_definition_component.ExpirationTime > FixPoint.Zero)
                m_definition_component.StartExpirationTimer(start_time, m_owner_component);
            else
                Deactivate(false);
            return true;
        }

        bool Deactivate(bool force)
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
                    cmp.Deactivate(force);
            }

            SetSkillActive(false);

#if COMBAT_CLIENT
            if (m_definition_component.m_main_render_effect_cfgid > 0)
            {
                PlayRenderEffectMessage msg = RenderMessage.Create<PlayRenderEffectMessage>();
                msg.ConstructAsStop(GetOwnerEntityID(), m_definition_component.m_main_render_effect_cfgid);
                GetLogicWorld().AddRenderMessage(msg);
            }
#endif
            return true;
        }

        public void Interrupt()
        {
            if (Deactivate(true))
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
            if (m_task == null)
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
                    Deactivate(false);
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
            if (m_definition_component.m_main_animation != null)
            {
                PlayAnimationRenderMessage msg = RenderMessage.Create<PlayAnimationRenderMessage>();
                msg.Construct(GetOwnerEntityID(), AnimationName.IDLE, null, true, FixPoint.One);
                GetLogicWorld().AddRenderMessage(msg);
            }
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
