using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class DirectDamageSkillComponent : SkillComponent, INeedTaskService
    {
        //配置数据
        FixPoint m_delay_time = FixPoint.Zero;
        int m_damage_type_id = 0;
        Formula m_damage_amount = RecyclableObject.Create<Formula>();
        int m_damage_render_effect_cfgid = 0;
        int m_damage_sound_cfgid = 0;
        int m_combo_attack_cnt = 1;
        FixPoint m_combo_interval = FixPoint.Zero;
        int m_render_effect_cfgid = 0;

        //运行数据
        ComponentCommonTask m_delay_task;
        int m_remain_attack_cnt = 0;
        ComboAttackTask m_combo_task;

        #region 初始化/销毁
        protected override void OnDestruct()
        {
            RecyclableObject.Recycle(m_damage_amount);
            m_damage_amount = null;

            if (m_delay_task != null)
            {
                m_delay_task.Cancel();
                LogicTask.Recycle(m_delay_task);
                m_delay_task = null;
            }

            if (m_combo_task != null)
            {
                m_combo_task.Cancel();
                LogicTask.Recycle(m_combo_task);
                m_combo_task = null;
            }
        }
        #endregion

        public override void Inflict(FixPoint start_time)
        {
            if (m_delay_time == FixPoint.Zero)
            {
                RealInflict();
            }
            else
            {
                if (m_delay_task == null)
                {
                    m_delay_task = LogicTask.Create<ComponentCommonTask>();
                    m_delay_task.Construct(this);
                }
                var schedeler = GetLogicWorld().GetTaskScheduler();
                schedeler.Schedule(m_delay_task, GetCurrentTime(), m_delay_time);
            }
        }

        public void RealInflict()
        {
            m_remain_attack_cnt = m_combo_attack_cnt;
            Impact();
            if (m_combo_attack_cnt > 1)
            {
                if (m_combo_task == null)
                {
                    m_combo_task = LogicTask.Create<ComboAttackTask>();
                    m_combo_task.Construct(this);
                }
                var schedeler = GetLogicWorld().GetTaskScheduler();
                schedeler.Schedule(m_combo_task, GetCurrentTime(), m_combo_interval, m_combo_interval);
            }
        }

        public override void Deactivate(bool force)
        {
            if (m_combo_task != null)
                m_combo_task.Cancel();
#if COMBAT_CLIENT
            if (m_render_effect_cfgid > 0)
            {
                PlayRenderEffectMessage stop_msg = RenderMessage.Create<PlayRenderEffectMessage>();
                stop_msg.ConstructAsStop(GetOwnerEntityID(), m_render_effect_cfgid);
                GetLogicWorld().AddRenderMessage(stop_msg);
            }
#endif
        }

        public void Impact()
        {
#if COMBAT_CLIENT
            if (m_render_effect_cfgid > 0)
            {
                if (m_remain_attack_cnt != m_combo_attack_cnt)
                {
                    PlayRenderEffectMessage stop_msg = RenderMessage.Create<PlayRenderEffectMessage>();
                    stop_msg.ConstructAsStop(GetOwnerEntityID(), m_render_effect_cfgid);
                    GetLogicWorld().AddRenderMessage(stop_msg);
                }
                PlayRenderEffectMessage start_msg = RenderMessage.Create<PlayRenderEffectMessage>();
                start_msg.ConstructAsPlay(GetOwnerEntityID(), m_render_effect_cfgid, FixPoint.MinusOne);
                GetLogicWorld().AddRenderMessage(start_msg);
            }
#endif
            --m_remain_attack_cnt;
            if (m_remain_attack_cnt <= 0)
            {
                if (m_combo_task != null)
                    m_combo_task.Cancel();
            }
            Skill skill = GetOwnerSkill();
            if (!skill.GetDefinitionComponent().NeedGatherTargets)
                skill.BuildSkillTargets();
            Entity attacker = GetOwnerEntity();
            List<Target> targets = skill.GetTargets();
            LogicWorld logic_world = GetLogicWorld();
            for (int i = 0; i < targets.Count; ++i)
            {
                m_current_target = targets[i].GetEntity(logic_world);
                if (m_current_target == null)
                    continue;
                DamagableComponent damageable_component = m_current_target.GetComponent(DamagableComponent.ID) as DamagableComponent;
                if (damageable_component == null)
                    continue;
                Damage damage = RecyclableObject.Create<Damage>();
                damage.m_attacker_id = attacker.ID;
                damage.m_defender_id = m_current_target.ID;
                damage.m_damage_type = m_damage_type_id;
                damage.m_damage_amount = m_damage_amount.Evaluate(this);
                damage.m_damage_amount = DamageSystem.Instance.CalculateDamageAmount(m_damage_type_id, damage.m_damage_amount, attacker, m_current_target);
                damage.m_render_effect_cfgid = m_damage_render_effect_cfgid;
                damage.m_sound_cfgid = m_damage_sound_cfgid;
                damageable_component.TakeDamage(damage);
            }
            m_current_target = null;
        }

        public void OnTaskService(FixPoint delta_time)
        {
            RealInflict();
        }
    }

    class ComboAttackTask : Task<LogicWorld>
    {
        DirectDamageSkillComponent m_component;

        public void Construct(DirectDamageSkillComponent component)
        {
            m_component = component;
        }

        public override void OnReset()
        {
            m_component = null;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            m_component.Impact();
        }
    }
}
