using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EffectGeneratorSkillComponent : SkillComponent, INeedTaskService
    {
        //配置数据
        FixPoint m_delay_time = FixPoint.Zero;
        int m_generator_cfgid = 0;
        int m_render_effect_cfgid = 0;
        FixPoint m_render_delay_time = FixPoint.Zero;  //纯粹为了一个不合适的特效，找不到美术改

        //运行数据
        EffectGenerator m_generator;
        ComponentCommonTask m_delay_task;
#if COMBAT_CLIENT
        PlayRenderEffectTask m_render_delay_task;  //纯粹为了一个不合适的特效，找不到美术改
#endif

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            m_generator = GetLogicWorld().GetEffectManager().CreateGenerator(m_generator_cfgid, GetOwnerEntity());
        }

        protected override void OnDestruct()
        {
            if (m_generator != null)
            {
                GetLogicWorld().GetEffectManager().DestroyGenerator(m_generator.ID, GetOwnerEntityID());
                m_generator = null;
            }
            if (m_delay_task != null)
            {
                m_delay_task.Cancel();
                LogicTask.Recycle(m_delay_task);
                m_delay_task = null;
            }

#if COMBAT_CLIENT
            if (m_render_delay_task != null)
            {
                m_render_delay_task.Cancel();
                LogicTask.Recycle(m_render_delay_task);
                m_render_delay_task = null;
            }
#endif
        }
        #endregion

        public override void Inflict(FixPoint start_time)
        {
            if (m_generator == null)
                return;
            if (m_delay_time == FixPoint.Zero)
            {
                Impact();
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

#if COMBAT_CLIENT
            if (m_render_effect_cfgid > 0 && m_render_delay_time > FixPoint.Zero)
            {
                if (m_render_delay_task == null)
                {
                    m_render_delay_task = LogicTask.Create<PlayRenderEffectTask>();
                    m_render_delay_task.Construct(this);
                }
                var schedeler = GetLogicWorld().GetTaskScheduler();
                schedeler.Schedule(m_render_delay_task, GetCurrentTime(), m_render_delay_time);
            }
#endif
        }

        void Impact()
        {
            Skill skill = GetOwnerSkill();
            if (!skill.GetDefinitionComponent().NeedGatherTargets)
                skill.BuildSkillTargets();
            List<Target> targets = GetOwnerSkill().GetTargets();
            Entity caster = GetOwnerEntity();
            EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
            app_data.m_original_entity_id = caster.ID;
            app_data.m_source_entity_id = caster.ID;
            m_generator.Activate(app_data, targets);
            m_current_target = null;
            RecyclableObject.Recycle(app_data);
#if COMBAT_CLIENT
            if (m_render_effect_cfgid > 0 && m_render_delay_time <= FixPoint.Zero)
                PlayRenderEffect();
#endif
        }

        public void PlayRenderEffect()
        {
            PlayRenderEffectMessage msg = RenderMessage.Create<PlayRenderEffectMessage>();
            msg.ConstructAsPlay(GetOwnerEntityID(), m_render_effect_cfgid, FixPoint.MinusOne);
            GetLogicWorld().AddRenderMessage(msg);
        }


        public override void Deactivate(bool force)
        {
            m_generator.Deactivate();
#if COMBAT_CLIENT
            if (m_render_effect_cfgid > 0)
            {
                PlayRenderEffectMessage stop_msg = RenderMessage.Create<PlayRenderEffectMessage>();
                stop_msg.ConstructAsStop(GetOwnerEntityID(), m_render_effect_cfgid);
                GetLogicWorld().AddRenderMessage(stop_msg);
            }
#endif
        }

        public void OnTaskService(FixPoint delta_time)
        {
            Impact();
        }
    }

#if COMBAT_CLIENT
    class PlayRenderEffectTask : Task<LogicWorld>
    {
        EffectGeneratorSkillComponent m_component;

        public void Construct(EffectGeneratorSkillComponent component)
        {
            m_component = component;
        }

        public override void OnReset()
        {
            m_component = null;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            m_component.PlayRenderEffect();
        }
    }
#endif
}
