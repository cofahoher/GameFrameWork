using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ProjectileParameters : IRecyclable, IDestruct
    {
        public int m_source_entity_id;
        public int m_target_entity_id;
        public FixPoint m_speed;
        public FixPoint m_lifetime;
        public Vector3FP m_facing;

        public void Reset()
        {
        }

        public void Destruct()
        {
        }
    }

    public partial class ProjectileComponent : EntityComponent
    {
        ProjectileParameters m_param;
        UpdateProjectileTask m_task;

        #region 初始化/销毁
        public void InitParam(ProjectileParameters param)
        {
            m_param = param;

            if (m_task == null)
                m_task = LogicTask.Create<UpdateProjectileTask>();
            m_task.Construct(this, m_param.m_lifetime);
            var schedeler = GetLogicWorld().GetTaskScheduler();
            schedeler.Schedule(m_task, GetCurrentTime(), LOGIC_UPDATE_INTERVAL, LOGIC_UPDATE_INTERVAL);

            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.StartMoving, ParentObject.ID);
        }

        protected override void OnDestruct()
        {
            if (m_param != null)
            {
                RecyclableObject.Recycle(m_param);
                m_param = null;
            }
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
        }
        #endregion

        public void UpdateProjectile(FixPoint delta_time)
        {
            PositionComponent position_component = ParentObject.GetComponent<PositionComponent>();
            Vector3FP new_position = position_component.CurrentPosition + m_param.m_facing * (m_param.m_speed * delta_time);
            position_component.CurrentPosition = new_position;
            if (DetectCollision(new_position))
                return;
            GridGraph grid_graph = GetLogicWorld().GetGridGraph();
            if (grid_graph != null)
            {
                GridNode node = grid_graph.Position2Node(new_position);
                if (node == null || !node.Walkable)
                {
                    m_task.LockRemainTime(m_param.m_speed);
                    return;
                }
            }
        }

        bool DetectCollision(Vector3FP position)
        {
            ISpaceManager space_manager = GetLogicWorld().GetSpaceManager();
            if (space_manager == null)
                return false;
            List<int> list = space_manager.CollectEntity_Point(position, m_param.m_source_entity_id);
            if (list.Count == 0)
                return false;
            EntityManager entity_manager = GetLogicWorld().GetEntityManager();
            for (int i = 0; i < list.Count; ++i)
            {
                Entity entity = entity_manager.GetObject(list[i]);
                if (entity == null)
                    continue;
            }
            return false;
        }

        public void Explode(Entity entity)
        {
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.StopMoving, ParentObject.ID);
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
            EntityUtil.KillEntity((Entity)ParentObject);
        }
    }

    public class UpdateProjectileTask : Task<LogicWorld>
    {
        ProjectileComponent m_component = null;
        FixPoint m_remain_time;
        bool m_lock = false;

        public void Construct(ProjectileComponent component, FixPoint lifetime)
        {
            m_component = component;
            m_remain_time = lifetime;
        }

        public override void OnReset()
        {
            m_component = null;
            m_lock = false;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            m_component.UpdateProjectile(delta_time);
            m_remain_time -= delta_time;
            if (m_remain_time < 0)
                m_component.Explode(null);
        }

        public void LockRemainTime(FixPoint speed)
        {
            if (m_lock)
                return;
            m_lock = true;
            FixPoint max_time = FixPoint.One / FixPoint.Two;
            if (m_remain_time > max_time)
                m_remain_time = max_time;
        }
    }
}