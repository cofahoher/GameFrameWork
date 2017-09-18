using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTSKillAction_Spurt : BTAction, INeedTaskService
    {
        //配置数据
        FixPoint m_distance = FixPoint.Zero;
        FixPoint m_time = FixPoint.Zero;
        int m_collision_target_generator_cfgid = 0;
        bool m_backward = false;

        //运行数据
        EffectGenerator m_collision_target_generator;
        List<int> m_collided_targets;
        ComponentCommonTask m_task;
        FixPoint m_remain_time = FixPoint.Zero;

        public BTSKillAction_Spurt()
        {
        }

        public BTSKillAction_Spurt(BTSKillAction_Spurt prototype)
            : base(prototype)
        {
            m_distance = prototype.m_distance;
            m_time = prototype.m_time;
            m_collision_target_generator_cfgid = prototype.m_collision_target_generator_cfgid;
            m_backward = prototype.m_backward;
        }

        protected override void ResetRuntimeData()
        {
            if (m_collision_target_generator != null)
            {
                SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
                GetLogicWorld().GetEffectManager().DestroyGenerator(m_collision_target_generator.ID, skill_component.GetOwnerEntityID());
                m_collision_target_generator = null;
            }
            if (m_collided_targets != null)
                m_collided_targets.Clear();
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
            m_remain_time = FixPoint.Zero;
        }

        public override void ClearRunningTrace()
        {
            StopSpurt();
        }

        protected override void OnActionEnter()
        {
            if (m_collision_target_generator_cfgid > 0 && m_collision_target_generator == null)
            {
                SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
                m_collision_target_generator = GetLogicWorld().GetEffectManager().CreateGenerator(m_collision_target_generator_cfgid, skill_component.GetOwnerEntity());
                if (m_collision_target_generator != null && m_collided_targets == null)
                    m_collided_targets = new List<int>();
            }
            if (m_task == null)
            {
                m_task = LogicTask.Create<ComponentCommonTask>();
                m_task.Construct(this);
            }
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
            LogicWorld logic_world = GetLogicWorld();
            var schedeler = logic_world.GetTaskScheduler();
            schedeler.Schedule(m_task, logic_world.GetCurrentTime(), LOGIC_UPDATE_INTERVAL, LOGIC_UPDATE_INTERVAL);
            m_remain_time = m_time;
#if COMBAT_CLIENT
            Skill skill = GetOwnerSkill();
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStartMoving(skill.GetOwnerEntityID(), true, LocomoteRenderMessage.NotLocomotion);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        protected override void OnActionExit()
        {
        }

        public void OnTaskService(FixPoint delta_time)
        {
            m_remain_time -= delta_time;
            if (m_remain_time <= FixPoint.Zero)
                m_task.Cancel();
            UpdateSpurt(delta_time);
        }

        void StopSpurt()
        {
            if (m_collision_target_generator != null)
                m_collision_target_generator.Deactivate();
            if (m_task != null)
                m_task.Cancel();
#if COMBAT_CLIENT
            Skill skill = GetOwnerSkill();
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStopMoving(skill.GetOwnerEntityID(), false, LocomoteRenderMessage.NotFromCommand);
            GetLogicWorld().AddRenderMessage(msg);
#endif
            if (m_collided_targets != null)
                m_collided_targets.Clear();
            m_remain_time = FixPoint.Zero;
        }

        public void UpdateSpurt(FixPoint delta_time)
        {
            Skill skill = GetOwnerSkill();
            Entity owner_entity = skill.GetOwnerEntity();
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
                    StopSpurt();
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
            SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
            Skill skill = skill_component.GetOwnerSkill();
            List<int> list = partition.CollectEntity_SurroundingRing(position, radius, FixPoint.Zero, skill.GetOwnerEntityID());
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
                if (!FactionRelation.IsFactionSatisfied(skill.GetOwnerPlayer().GetFaction(entity.GetOwnerPlayerID()), FactionRelation.NotAlly))
                    continue;
                Entity current_target = entity;
                skill_component.CurrentTarget = current_target;
                EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
                app_data.m_original_entity_id = skill.GetOwnerEntityID();
                app_data.m_source_entity_id = app_data.m_original_entity_id;
                m_collision_target_generator.Activate(app_data, entity);
                RecyclableObject.Recycle(app_data);
            }
            skill_component.CurrentTarget = null;
        }
    }
}