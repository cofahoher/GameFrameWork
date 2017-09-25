using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class KnockbackEffectComponent : EffectComponent, INeedTaskService
    {
        //配置数据
        FixPoint m_distance = FixPoint.Zero;
        FixPoint m_time = FixPoint.Zero;

        //运行数据
        Vector3FP m_direction = new Vector3FP();
        ComponentCommonTaskWithLastingTime m_task;

        public override void Apply()
        {
            if (m_task == null)
                m_task = LogicTask.Create<ComponentCommonTaskWithLastingTime>();

            EffectDefinitionComponent definition_component = ((Effect)ParentObject).GetDefinitionComponent();
            Entity entity = GetLogicWorld().GetEntityManager().GetObject(definition_component.SourceEntityID);
            if (entity != null)
            {
                PositionComponent position_component = entity.GetComponent(PositionComponent.ID) as PositionComponent;
                if (position_component != null)
                    m_direction = position_component.Facing3D;
            }
            m_task.Construct(this, m_time);
            var schedeler = GetLogicWorld().GetTaskScheduler();
            schedeler.Schedule(m_task, GetCurrentTime(), LOGIC_UPDATE_INTERVAL, LOGIC_UPDATE_INTERVAL);
#if COMBAT_CLIENT
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStartMoving(GetOwnerEntityID(), true, LocomoteRenderMessage.NotLocomotion);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        public override void Unapply()
        {
            m_task.Cancel();
            LogicTask.Recycle(m_task);
            m_task = null;
#if COMBAT_CLIENT
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStopMoving(GetOwnerEntityID(), false, LocomoteRenderMessage.NotFromCommand);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        public void OnTaskService(FixPoint delta_time)
        {
            Entity owner_entity = GetOwnerEntity();
            PositionComponent position_component = owner_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            Vector3FP offset = m_direction * (m_distance * delta_time / m_time);
            Vector3FP new_position = position_component.CurrentPosition + offset;
            GridGraph grid_graph = position_component.GetGridGraph();
            if (grid_graph != null)
            {
                GridNode node = grid_graph.Position2Node(new_position);
                if (node == null || !node.Walkable)
                {
                    m_task.Cancel();
                    return;
                }
            }
            position_component.CurrentPosition = new_position;
        }
    }
}