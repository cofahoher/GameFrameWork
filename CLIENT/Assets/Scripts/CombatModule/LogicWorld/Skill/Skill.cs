using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Skill : Object
    {
        bool m_is_active = false;
        List<Target> m_skill_targets = new List<Target>();
        SkillManagerComponent m_skill_manager_cmp;
        SkillDefinitionComponent m_definition_cmp;
        EffectGeneratorSkillComponent m_effect_generator_cmp;
        WeaponSkillComponent m_weapon_cmp;
        SkillCountdownTask m_task;

        protected override void OnDestruct()
        {
            m_is_active = false;
            m_skill_targets.Clear();
            m_skill_manager_cmp = null;
            m_definition_cmp = null;
            m_effect_generator_cmp = null;
            m_weapon_cmp = null;
            m_task.Cancel();
            m_task = null;
        }

        #region 初始化
        protected override void PreInitializeObject(ObjectCreationContext context)
        {
            var ownerEntity = context.m_logic_world.GetEntityManager().GetObject(m_context.m_object_id);
            if(ownerEntity != null)
            {
                m_skill_manager_cmp = ownerEntity.GetComponent<SkillManagerComponent>();
            }
        }

        protected override void PostInitializeObject(ObjectCreationContext context)
        {
            m_definition_cmp = this.GetComponent<SkillDefinitionComponent>();
            m_effect_generator_cmp = this.GetComponent<EffectGeneratorSkillComponent>();
            m_weapon_cmp = this.GetComponent<WeaponSkillComponent>();
        }
        #endregion

        #region ILogicOwnerInfo
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
            var timer = m_definition_cmp.GetTimer(SkillTimerType.CooldownTimer);
            if(timer.Active())
            {
                return timer.GetRemaining(GetCurrentTime());
            }
            return FixPoint.Zero;
        }
        #endregion

        #region 技能的Activate流程
        public bool Activate(FixPoint start_time)
        {
            return true;
        }

        public bool PostActivate(FixPoint start_time)
        {
            return true;

        }

        public bool Deactivate()
        {

            return true;
        }

        //打断
        public void Interrupt()
        {
            if(Deactivate())
                ScheduleServiceCountdown();
        }
        #endregion
        private void ScheduleServiceCountdown()
        {

        }
        public void ServiceCountdown(FixPoint current_time, FixPoint delta_time)
        {

        }

        #region Getter
        public SkillDefinitionComponent GetSkillDefinitionComponent()
        {
            return m_definition_cmp;
        }
        #endregion
    }


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