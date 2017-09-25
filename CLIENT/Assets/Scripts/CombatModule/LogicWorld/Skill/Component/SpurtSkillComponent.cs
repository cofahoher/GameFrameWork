using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SpurtSkillComponent : SkillComponent, INeedTaskService
    {
        //配置数据
        FixPoint m_distance = FixPoint.Zero;
        FixPoint m_time = FixPoint.Zero;
        int m_collision_target_generator_cfgid = 0;
        bool m_backward = false;

        //运行数据
        EffectGenerator m_collision_target_generator;
        List<int> m_collided_targets;
        ComponentCommonTaskWithLastingTime m_task;

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            if (m_collision_target_generator_cfgid != 0)
                m_collision_target_generator = GetLogicWorld().GetEffectManager().CreateGenerator(m_collision_target_generator_cfgid, GetOwnerEntity());
            if (m_collision_target_generator != null)
                m_collided_targets = new List<int>();
        }

        protected override void OnDestruct()
        {
            if (m_collision_target_generator != null)
            {
                GetLogicWorld().GetEffectManager().DestroyGenerator(m_collision_target_generator.ID, GetOwnerEntityID());
                m_collision_target_generator = null;
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
            if (m_task == null)
                m_task = LogicTask.Create<ComponentCommonTaskWithLastingTime>();
            m_task.Construct(this, m_time);
            var schedeler = GetLogicWorld().GetTaskScheduler();
            schedeler.Schedule(m_task, GetCurrentTime(), LOGIC_UPDATE_INTERVAL, LOGIC_UPDATE_INTERVAL);
#if COMBAT_CLIENT
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStartMoving(GetOwnerEntityID(), true, LocomoteRenderMessage.NotLocomotion);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        public override void Deactivate(bool force)
        {
            if (m_collision_target_generator != null)
                m_collision_target_generator.Deactivate();
            if (m_task != null)
                m_task.Cancel();
#if COMBAT_CLIENT
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStopMoving(GetOwnerEntityID(), false, LocomoteRenderMessage.NotFromCommand);
            GetLogicWorld().AddRenderMessage(msg);
#endif
            if (m_collided_targets != null)
                m_collided_targets.Clear();
        }

        public void OnTaskService(FixPoint delta_time)
        {
            Entity owner_entity = GetOwnerEntity();
            PositionComponent position_component = owner_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            Vector3FP offset = position_component.Facing3D * (m_distance * delta_time / m_time);
            if (m_backward)
                offset = -offset;
            Vector3FP new_position = position_component.CurrentPosition + offset;
            GridGraph grid_graph = position_component.GetGridGraph();
            if (grid_graph != null)
            {
                GridNode node = grid_graph.Position2Node(new_position);
                if (node == null || !node.Walkable)
                {
                    Deactivate(false);
                    return;
                }
            }
            position_component.CurrentPosition = new_position;
            if (m_collision_target_generator != null)
                DetectCollision(position_component.GetSpacePartition(), new_position, position_component.Radius);
        }

        void DetectCollision(ISpacePartition partition, Vector3FP position, FixPoint radius)
        {
            if (partition == null)
                return;
            List<int> list = partition.CollectEntity_SurroundingRing(position, radius, FixPoint.Zero, GetOwnerEntityID());
            if (list.Count == 0)
                return;
            EntityManager entity_manager = GetLogicWorld().GetEntityManager();
            for (int i = 0; i < list.Count; ++i)
            {
                if (m_collided_targets.Contains(list[i]))
                    continue;
                m_collided_targets.Add(list[i]);
                Entity entity = entity_manager.GetObject(list[i]);
                if (entity == null)
                    continue;
                PositionComponent position_component = entity.GetComponent(PositionComponent.ID) as PositionComponent;
                if (position_component.Height <= FixPoint.Zero) //ZZWTODO
                    continue;
                if (!FactionRelation.IsFactionSatisfied(GetOwnerPlayer().GetFaction(entity.GetOwnerPlayerID()), FactionRelation.NotAlly))
                    continue;
                m_current_target = entity;
                EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
                app_data.m_original_entity_id = GetOwnerEntityID();
                app_data.m_source_entity_id = app_data.m_original_entity_id;
                m_collision_target_generator.Activate(app_data, entity);
                RecyclableObject.Recycle(app_data);
            }
            m_current_target = null;
        }
    }
}
