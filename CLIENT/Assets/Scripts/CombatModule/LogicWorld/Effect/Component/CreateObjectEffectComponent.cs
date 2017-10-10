using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class CreateObjectEffectComponent : EffectComponent, INeedTaskService
    {
        //配置数据
        int m_object_type_id = 0;
        int m_object_proto_id = 0;
        FixPoint m_object_life_time = FixPoint.Zero;
        Vector3FP m_offset;
        int m_object_count = 1;  //-1表示无限
        FixPoint m_interval = FixPoint.Zero;
        bool m_revert_when_unapply = true;

        //运行数据
        List<int> m_objects_id;
        int m_remain_count = 0;
        ComponentCommonTask m_task;

        protected override void OnDestruct()
        {
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
        }

        public override void Apply()
        {
            m_remain_count = m_object_count;
            CreateOneObject();
            if (m_object_count > 1 || m_object_count == -1)
            {
                if (m_task == null)
                {
                    m_task = LogicTask.Create<ComponentCommonTask>();
                    m_task.Construct(this);
                }
                var schedeler = GetLogicWorld().GetTaskScheduler();
                schedeler.Schedule(m_task, GetCurrentTime(), m_interval, m_interval);
            }
        }

        public void CreateOneObject()
        {
            if (m_remain_count > 0)
                --m_remain_count;
            Entity owner_entity = GetOwnerEntity();
            Entity created = EntityUtil.CreateEntityForSkillAndEffect(this, owner_entity, null, m_offset, FixPoint.Zero, m_object_type_id, m_object_proto_id, m_object_life_time, null);
            if (created != null && m_revert_when_unapply)
            {
                if (m_objects_id == null)
                    m_objects_id = new List<int>();
                m_objects_id.Add(created.ID);
            }
        }

        public override void Unapply()
        {
            if (!m_revert_when_unapply || m_objects_id == null)
                return;
            for (int i = 0; i < m_objects_id.Count; ++i)
                GetLogicWorld().GetEntityManager().DestroyObject(m_objects_id[i]);
        }

        public void OnTaskService(FixPoint delta_time)
        {
            CreateOneObject();
            if (m_remain_count == 0)
            {
                if (m_task != null)
                    m_task.Cancel();
            }
        }
    }
}