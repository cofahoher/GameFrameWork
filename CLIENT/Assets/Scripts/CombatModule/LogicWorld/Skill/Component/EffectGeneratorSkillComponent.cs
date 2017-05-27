using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EffectGeneratorSkillComponent : SkillComponent, INeedTaskService
    {
        //配置数据
        int m_generator_cfgid = 0;
        FixPoint m_delay_time = FixPoint.Zero;

        //运行数据
        EffectGenerator m_generator;
        ComponentCommonTask m_task;

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
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
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
                if (m_task == null)
                {
                    m_task = LogicTask.Create<ComponentCommonTask>();
                    m_task.Construct(this);
                }
                var schedeler = GetLogicWorld().GetTaskScheduler();
                schedeler.Schedule(m_task, GetCurrentTime(), m_delay_time);
            }
        }

        void Impact()
        {
            List<Target> targets = GetOwnerSkill().GetTargets();
            Entity caster = GetOwnerEntity();
            EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
            app_data.m_original_entity_id = caster.ID;
            app_data.m_source_entity_id = caster.ID;
            m_generator.Activate(app_data, targets);
            m_current_target = null;
            RecyclableObject.Recycle(app_data);
        }

        public override void Deactivate()
        {
            m_generator.Deactivate();
        }

        public void OnTaskService(FixPoint delta_time)
        {
            Impact();
        }
    }
}
