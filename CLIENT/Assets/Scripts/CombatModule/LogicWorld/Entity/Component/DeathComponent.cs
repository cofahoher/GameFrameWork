using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class DeathComponent : EntityComponent
    {
        FixPoint m_hide_delay;
        FixPoint m_delete_delay;

        #region 初始化/销毁
        #endregion

        public void KillOwner()
        {
            ParentObject.DeletePending = true;
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.Die, ParentObject.ID);

            var schedeler = GetLogicWorld().GetTaskScheduler();
            HideEntityTask hide_task = LogicTask.Create<HideEntityTask>();
            hide_task.Construct(ParentObject.ID);
            schedeler.Schedule(hide_task, GetCurrentTime(), m_hide_delay);
            DeleteEntityTask delete_task = LogicTask.Create<DeleteEntityTask>();
            delete_task.Construct(ParentObject.ID);
            schedeler.Schedule(delete_task, GetCurrentTime(), m_delete_delay);
        }
    }

    class HideEntityTask : Task<LogicWorld>
    {
        int m_entity_id;

        public void Construct(int entity_id)
        {
            m_entity_id = entity_id;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            logic_world.AddSimpleRenderMessage(RenderMessageType.Hide, m_entity_id);
            LogicTask.Recycle(this);
        }
    }       

    class DeleteEntityTask : Task<LogicWorld>
    {
        int m_entity_id;

        public void Construct(int entity_id)
        {
            m_entity_id = entity_id;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            logic_world.GetEntityManager().DestroyObject(m_entity_id);
            LogicTask.Recycle(this);
        }
    }
}