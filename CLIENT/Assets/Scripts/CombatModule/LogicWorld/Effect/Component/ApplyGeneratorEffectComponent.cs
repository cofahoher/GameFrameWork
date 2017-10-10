using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class ApplyGeneratorEffectComponent : EffectComponent, INeedTaskService
    {
        //配置数据
        int m_generator_cfgid = 0;
        int m_combo_count = 1;  //-1表示无限
        FixPoint m_combo_interval = FixPoint.One;

        //运行数据
        EffectGenerator m_generator;
        int m_remain_count = 0;
        ComponentCommonTask m_combo_task;

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            m_generator = GetLogicWorld().GetEffectManager().CreateGenerator(m_generator_cfgid, GetOwnerEntity());
        }

        protected override void OnDestruct()
        {
            if (m_generator != null)
            {
                m_generator.Deactivate();
                GetLogicWorld().GetEffectManager().DestroyGenerator(m_generator.ID, GetOwnerEntityID());
            }
            CancelTask();
        }

        void CancelTask()
        {
            if (m_combo_task == null)
                return;
            m_combo_task.Cancel();
            LogicTask.Recycle(m_combo_task);
            m_combo_task = null;
        }
        #endregion

        public override void Apply()
        {
            if (m_generator == null)
                return;
            m_remain_count = m_combo_count;
            ApplyOnce();
            if (m_combo_count > 1 || m_combo_count == -1)
            {
                if (m_combo_task == null)
                {
                    m_combo_task = LogicTask.Create<ComponentCommonTask>();
                    m_combo_task.Construct(this);
                }
                var schedeler = GetLogicWorld().GetTaskScheduler();
                schedeler.Schedule(m_combo_task, GetCurrentTime(), m_combo_interval, m_combo_interval);
            }
        }

        void ApplyOnce()
        {
            if (m_remain_count > 0)
                --m_remain_count;
            if (m_remain_count == 0)
                CancelTask();
            Entity owner = GetOwnerEntity();
            EffectDefinitionComponent definition_component = ((Effect)ParentObject).GetDefinitionComponent();
            EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
            app_data.m_original_entity_id = definition_component.OriginalEntityID;
            app_data.m_source_entity_id = owner.ID;
            m_generator.Activate(app_data, owner);
            RecyclableObject.Recycle(app_data);
        }

        public override void Unapply()
        {
            CancelTask();
        }

        public void OnTaskService(FixPoint delta_time)
        {
            ApplyOnce();
        }
    }
}