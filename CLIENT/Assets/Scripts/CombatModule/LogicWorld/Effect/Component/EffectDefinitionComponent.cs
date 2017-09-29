using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EffectDefinitionComponent : EffectComponent
    {
        //配置数据
        int m_category = 0;
        int m_conflict_id = 0;
        Formula m_duration = RecyclableObject.Create<Formula>(); //0非持续，-1永远有效
        int m_render_effect_cfgid = 0;
        //运行数据
        int m_original_entity_id = 0;
        int m_source_entity_id = 0;
        int m_target_entity_id = 0;
        int m_generator_id = 0;
        int m_entry_index = -1;
        FixPoint m_expiration_time;
        EffectExpireTask m_task;

        #region GETTER

        public int OriginalEntityID
        {
            get { return m_original_entity_id; }
        }

        public int SourceEntityID
        {
            get { return m_source_entity_id; }
        }

        public int TargetEntityID
        {
            get { return m_target_entity_id; }
        }

        public FixPoint ExpirationTime
        {
            get { return m_expiration_time; }
        }
        #endregion

        #region 初始化/销毁
        public void InitializeApplicationData(EffectApplicationData app_data)
        {
            m_original_entity_id = app_data.m_original_entity_id;
            m_source_entity_id = app_data.m_source_entity_id;
            m_target_entity_id = app_data.m_target_entity_id;
            m_generator_id = app_data.m_generator_id;
            m_entry_index = app_data.m_entry_index;
            FixPoint duration = m_duration.Evaluate(this);
            if (duration < 0)
                m_expiration_time = FixPoint.MaxValue;
            else
                m_expiration_time = ParentObject.CreationTime + duration;
        }

        protected override void OnDestruct()
        {
            EffectGenerator generator = GetLogicWorld().GetEffectManager().GetGenerator(m_generator_id);
            if (generator == null)
                return;
            EffectGeneratorEntry entry = generator.GetEntry(m_entry_index);
            if (entry == null)
                return;
            entry.RemoveEffect(ParentObject.ID);

            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
        }
        #endregion

        public void MarkExpired()
        {
            m_expiration_time = GetCurrentTime();
        }

        public void StartExpirationTask()
        {
            if (m_expiration_time == FixPoint.MaxValue)
                return;
            if (m_task == null)
            {
                m_task = LogicTask.Create<EffectExpireTask>();
                m_task.Construct(GetOwnerEntityID(), ParentObject.ID);
            }
            FixPoint current_time = GetCurrentTime();
            var schedeler = GetLogicWorld().GetTaskScheduler();
            FixPoint delay = m_expiration_time - current_time;
            schedeler.Schedule(m_task, current_time, delay);
        }

        public override void Apply()
        {
#if COMBAT_CLIENT
            if (m_render_effect_cfgid > 0)
            {
                PlayRenderEffectMessage msg = RenderMessage.Create<PlayRenderEffectMessage>();
                msg.ConstructAsPlay(GetOwnerEntityID(), m_render_effect_cfgid, FixPoint.MinusOne);
                GetLogicWorld().AddRenderMessage(msg);
            }
#endif
        }

        public override void Unapply()
        {
#if COMBAT_CLIENT
            if (m_render_effect_cfgid > 0)
            {
                PlayRenderEffectMessage stop_msg = RenderMessage.Create<PlayRenderEffectMessage>();
                stop_msg.ConstructAsStop(GetOwnerEntityID(), m_render_effect_cfgid);
                GetLogicWorld().AddRenderMessage(stop_msg);
            }
#endif
        }
    }
}