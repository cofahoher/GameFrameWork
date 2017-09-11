using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ProjectileParameters : IRecyclable
    {
        public FixPoint m_start_time = FixPoint.Zero;
        public FixPoint m_life_time = FixPoint.Zero;
        public int m_source_entity_id = 0;
        public int m_target_entity_id = 0;
        public Vector3FP m_start_position;
        public Vector3FP m_target_position;
        public Vector3FP m_fixed_facing;
        public int m_generator_id = 0;
        public Vector3FP m_bezier_b;

        public void Reset()
        {
            m_start_time = FixPoint.Zero;
            m_life_time = FixPoint.Zero;
            m_source_entity_id = 0;
            m_target_entity_id = 0;
            m_start_position.MakeZero();
            m_target_position.MakeZero();
            m_fixed_facing.MakeZero();
            m_generator_id = 0;
        }
    }

    public partial class ProjectileComponent : EntityComponent
    {
        public static readonly int TrackModeNone = (int)CRC.Calculate("none");
        public static readonly int TrackModeLockTarget = (int)CRC.Calculate("lock");

        public static readonly int TrajectoryTypeLine = (int)CRC.Calculate("Line");
        public static readonly int TrajectoryTypeBezier = (int)CRC.Calculate("bezier");
        //public static readonly int TrajectoryTypeArc = (int)CRC.Calculate("arc");
        //public static readonly int TrajectoryTypeParabola = (int)CRC.Calculate("parabola");

        //配置数据
        FixPoint m_speed;  //有速度按速度飞行，没速度是固定时间到达目标
        FixPoint m_lifetime = FixPoint.Ten;
        int m_track_mode = 0;
        int m_trajectory_type = 0;
        FixPoint m_extra_hight = FixPoint.Zero;
        bool m_can_cross_obstacle = true;
        bool m_pierce_entity = false;
        int m_collision_faction = FactionRelation.NotAlly;
        int m_collision_sound_cfgid = 0;

        //运行数据
        bool m_previous_walkable = true;
        ProjectileParameters m_param;
        UpdateProjectileTask m_task;
        List<int> m_effected_entities;

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            if (m_track_mode == 0)
                m_track_mode = TrackModeNone;
            else if (m_track_mode == TrackModeLockTarget)
                m_pierce_entity = false;
            if (m_trajectory_type == 0)
                m_trajectory_type = TrajectoryTypeLine;
            ObjectProtoData proto_data = ParentObject.GetCreationContext().m_proto_data;
            if (proto_data == null)
                return;
            var variables = proto_data.m_component_variables;
            if (variables == null)
                return;
            string value;
            if (variables.TryGetValue("speed", out value))
                m_speed = FixPoint.Parse(value);
            if (variables.TryGetValue("lifetime", out value))
                m_lifetime = FixPoint.Parse(value);
            if (variables.TryGetValue("track_mode", out value))
                m_track_mode = (int)CRC.Calculate(value);
            if (variables.TryGetValue("trajectory_type", out value))
                m_trajectory_type = (int)CRC.Calculate(value);
            if (variables.TryGetValue("extra_hight", out value))
                m_extra_hight = FixPoint.Parse(value);
            if (variables.TryGetValue("can_cross_obstacle", out value))
                m_can_cross_obstacle = bool.Parse(value);
            if (variables.TryGetValue("pierce_entity", out value))
                m_pierce_entity = bool.Parse(value);
            if (variables.TryGetValue("collision_faction", out value))
                m_collision_faction = (int)CRC.Calculate(value);
            if (variables.TryGetValue("collision_sound", out value))
                m_collision_sound_cfgid = int.Parse(value);

            if (m_pierce_entity)
                m_effected_entities = new List<int>();
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

        public void InitParam(ProjectileParameters param)
        {
            m_param = param;
            if (m_trajectory_type == TrajectoryTypeBezier)
            {
                m_param.m_bezier_b = (m_param.m_start_position + m_param.m_target_position) / FixPoint.Two;
                m_param.m_bezier_b.y += m_extra_hight;
            }
            if (m_param.m_life_time <= FixPoint.Zero)
                m_param.m_life_time = m_lifetime;

            PositionComponent position_component = ParentObject.GetComponent(PositionComponent.ID) as PositionComponent;
            GridGraph grid_graph = position_component.GetGridGraph();
            GridNode node = grid_graph.Position2Node(position_component.CurrentPosition);
            m_previous_walkable = (node != null && node.Walkable);

            if (m_task == null)
                m_task = LogicTask.Create<UpdateProjectileTask>();
            m_task.Construct(this, m_param.m_life_time);
            var schedeler = GetLogicWorld().GetTaskScheduler();
            schedeler.Schedule(m_task, GetCurrentTime(), LOGIC_UPDATE_INTERVAL, LOGIC_UPDATE_INTERVAL);

#if COMBAT_CLIENT
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStartMoving(ParentObject.ID, true);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }
        #endregion

        #region GETTER/SETTER
        #endregion

        public bool UpdateProjectile(FixPoint delta_time)
        {
            PositionComponent position_component = ParentObject.GetComponent(PositionComponent.ID) as PositionComponent;
            if (m_track_mode == TrackModeLockTarget)
            {
                Entity target = null;
                PositionComponent target_position_component = null;
                if (m_param.m_target_entity_id > 0)
                {
                    target = GetLogicWorld().GetEntityManager().GetObject(m_param.m_target_entity_id);
                    if (target != null && !ObjectUtil.IsDead(target))
                    {
                        target_position_component = target.GetComponent(PositionComponent.ID) as PositionComponent;
                        if (target_position_component != null)
                            m_param.m_target_position = target_position_component.CurrentPosition;
                    }
                    else
                    {
                        target = null;
                    }
                }
                if (m_trajectory_type == TrajectoryTypeBezier)
                {
                    FixPoint t = (GetCurrentTime() - m_param.m_start_time) / m_param.m_life_time;
                    if (t < FixPoint.One)
                    {
                        FixPoint l_t = FixPoint.One - t;
                        Vector3FP new_position = l_t * l_t * m_param.m_start_position + FixPoint.Two * t * l_t * m_param.m_bezier_b + t * t * m_param.m_target_position;
                        position_component.CurrentPosition = new_position;
                        return false;
                    }
                    else
                    {
                        position_component.CurrentPosition = m_param.m_target_position;
                        Explode(target);
                        return true;
                    }
                }
                else
                {
                    m_param.m_fixed_facing = m_param.m_target_position - position_component.CurrentPosition;
                    m_param.m_fixed_facing.y = FixPoint.Zero;
                    FixPoint distance = m_param.m_fixed_facing.Normalize();
                    FixPoint delta_distance = m_speed * delta_time;
                    Vector3FP new_position = position_component.CurrentPosition + m_param.m_fixed_facing * delta_distance;
                    position_component.CurrentPosition = new_position;
                    distance = FixPoint.Abs(distance - delta_distance);
                    bool hit = false;
                    if (target_position_component != null)
                    {
                        if (distance <= position_component.Radius + target_position_component.Radius)
                            hit = true;
                    }
                    else
                    {
                        if (distance <= m_speed * LOGIC_UPDATE_INTERVAL)
                            hit = true;
                    }
                    if (hit)
                    {
                        Explode(target);
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                Vector3FP new_position = position_component.CurrentPosition + m_param.m_fixed_facing * (m_speed * delta_time);
                position_component.CurrentPosition = new_position;
                if (DetectCollision(position_component.GetSpacePartition(), new_position, position_component.Radius))
                    return true;
                GridGraph grid_graph = position_component.GetGridGraph();
                if (grid_graph != null)
                {
                    GridNode node = grid_graph.Position2Node(new_position);
                    if (node == null)
                    {
                        m_task.LockRemainTime(m_speed);
                    }
                    else if (!node.Walkable && !m_can_cross_obstacle)
                    {
                        if (m_previous_walkable)
                            m_task.LockRemainTime(m_speed);
                    }
                    else
                    {
                        m_previous_walkable = true;
                    }
                }
                return false;
            }
        }

        bool DetectCollision(ISpacePartition partition, Vector3FP position, FixPoint radius)
        {
            if (partition == null)
                return false;
            List<int> list = partition.CollectEntity_SurroundingRing(position, radius, FixPoint.Zero, m_param.m_source_entity_id);
            if (list.Count == 0)
                return false;
            EntityManager entity_manager = GetLogicWorld().GetEntityManager();
            for (int i = 0; i < list.Count; ++i)
            {
                if (m_pierce_entity)
                {
                    if (m_effected_entities.Contains(list[i]))
                        continue;
                    else
                        m_effected_entities.Add(list[i]);
                }
                Entity entity = entity_manager.GetObject(list[i]);
                if (entity == null)
                    continue;
                PositionComponent position_component = entity.GetComponent(PositionComponent.ID) as PositionComponent;
                if (position_component.Height <= FixPoint.Zero) //ZZWTODO
                    continue;
                if (!FactionRelation.IsFactionSatisfied(GetOwnerPlayer().GetFaction(entity.GetOwnerPlayerID()), m_collision_faction))
                    continue;
                Explode(entity);
                if (!m_pierce_entity)
                    return true;
            }
            return false;
        }

        public void Explode(Entity entity)
        {
#if COMBAT_CLIENT
            if (m_collision_sound_cfgid > 0)
            {
                PlaySoundMessage sound_msg = RenderMessage.Create<PlaySoundMessage>();
                sound_msg.Construct(GetOwnerEntityID(), m_collision_sound_cfgid, FixPoint.MinusOne);
                GetLogicWorld().AddRenderMessage(sound_msg);
            }
#endif
            ApplyGenerator(entity);

            if (entity == null || !m_pierce_entity)
            {
#if COMBAT_CLIENT
                LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
                msg.ConstructAsStopMoving(ParentObject.ID, true);
                GetLogicWorld().AddRenderMessage(msg);
#endif
                if (m_task != null)
                {
                    m_task.Cancel();
                    LogicTask.Recycle(m_task);
                    m_task = null;
                }
                EntityUtil.KillEntity((Entity)ParentObject, ParentObject.ID);
            }
        }

        void ApplyGenerator(Entity entity)
        {
            LogicWorld logic_world = GetLogicWorld();
            EffectGenerator generator = logic_world.GetEffectManager().GetGenerator(m_param.m_generator_id);
            if (generator == null)
                return;
            EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
            app_data.m_original_entity_id = m_param.m_source_entity_id;
            app_data.m_source_entity_id = GetOwnerEntityID();
            generator.Activate(app_data, entity);
            RecyclableObject.Recycle(app_data);
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
            m_remain_time = FixPoint.Zero;
            m_lock = false;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            if (m_component.UpdateProjectile(delta_time))
                return;
            m_remain_time -= delta_time;
            if (m_remain_time < 0)
                m_component.Explode(null);
        }

        public void LockRemainTime(FixPoint speed)
        {
            if (m_lock)
                return;
            m_lock = true;
            FixPoint max_time = FixPoint.One / speed;
            if (m_remain_time > max_time)
                m_remain_time = max_time;
        }
    }
}