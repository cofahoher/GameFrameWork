using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SpurtSkillComponent : SkillComponent
    {
        //配置数据
        FixPoint m_distance = FixPoint.Zero;
        FixPoint m_time = FixPoint.Zero;

        //运行数据
        UpdateSpurtTask m_task;

        #region 初始化/销毁
        protected override void OnDestruct()
        {
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
            if (m_task == null)
                m_task = LogicTask.Create<UpdateSpurtTask>();
            m_task.Construct(this, m_time);
            var schedeler = GetLogicWorld().GetTaskScheduler();
            schedeler.Schedule(m_task, GetCurrentTime(), LOGIC_UPDATE_INTERVAL, LOGIC_UPDATE_INTERVAL);
#if COMBAT_CLIENT
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStartMoving(GetOwnerEntityID(), true, LocomoteRenderMessage.NotLocomotion);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        public override void Deactivate()
        {
            m_task.Cancel();
#if COMBAT_CLIENT
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStopMoving(GetOwnerEntityID(), false, LocomoteRenderMessage.NotFromCommand);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        public void UpdateSpurt(FixPoint delta_time)
        {
            Entity owner_entity = GetOwnerEntity();
            PositionComponent position_component = owner_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            Vector3FP new_position = position_component.CurrentPosition + position_component.Facing3D * (m_distance * delta_time / m_time);
            GridGraph grid_graph = GetLogicWorld().GetGridGraph();
            if (grid_graph != null)
            {
                GridNode node = grid_graph.Position2Node(new_position);
                if (node == null || !node.Walkable)
                {
                    Deactivate();
                    return;
                }
            }
            position_component.CurrentPosition = new_position;
        }
    }

    public class UpdateSpurtTask : Task<LogicWorld>
    {
        SpurtSkillComponent m_component = null;
        FixPoint m_remain_time = FixPoint.Zero;

        public void Construct(SpurtSkillComponent component, FixPoint lasting_time)
        {
            m_component = component;
            m_remain_time = lasting_time;
        }

        public override void OnReset()
        {
            m_component = null;
            m_remain_time = FixPoint.Zero;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            m_component.UpdateSpurt(delta_time);
            m_remain_time -= delta_time;
            if (m_remain_time < 0)
                Cancel();
        }
    }
}
