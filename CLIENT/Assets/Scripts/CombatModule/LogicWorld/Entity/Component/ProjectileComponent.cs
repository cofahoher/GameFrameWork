using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ProjectileParameters : IRecyclable
    {
        public FixPoint m_life_time = FixPoint.Zero;
        public int m_source_entity_id = 0;
        public int m_target_entity_id = 0;
        public Vector3FP m_facing;
        public int m_generator_id = 0;

        public void Reset()
        {
            m_source_entity_id = 0;
            m_target_entity_id = 0;
            m_facing.MakeZero();
            m_generator_id = 0;
        }
    }

    public partial class ProjectileComponent : EntityComponent
    {
        //配置数据
        FixPoint m_speed;
        FixPoint m_lifetime = FixPoint.Ten;
        int m_collision_sound_cfgid = 0;
        int m_collision_faction = FactionRelation.NotAlly;
        bool m_can_cross_obstacle = true;

        //运行数据
        bool m_previous_walkable = true;
        ProjectileParameters m_param;
        UpdateProjectileTask m_task;

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            ObjectProtoData proto_data = ParentObject.GetCreationContext().m_proto_data;
            if (proto_data == null)
                return;
            var dic = proto_data.m_component_variables;
            if (dic == null)
                return;
            string value;
            if (dic.TryGetValue("speed", out value))
                m_speed = FixPoint.Parse(value);
            if (dic.TryGetValue("lifetime", out value))
                m_lifetime = FixPoint.Parse(value);
            if (dic.TryGetValue("collision_sound", out value))
                m_collision_sound_cfgid = int.Parse(value);
            if (dic.TryGetValue("collision_faction", out value))
                m_collision_faction = (int)CRC.Calculate(value);
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

            PositionComponent position_component = ParentObject.GetComponent(PositionComponent.ID) as PositionComponent;
            GridGraph grid_graph = position_component.GetGridGraph();
            GridNode node = grid_graph.Position2Node(position_component.CurrentPosition);
            m_previous_walkable = (node != null && node.Walkable);

            if (m_task == null)
                m_task = LogicTask.Create<UpdateProjectileTask>();
            m_task.Construct(this, m_param.m_life_time > FixPoint.Zero ? m_param.m_life_time : m_lifetime);
            var schedeler = GetLogicWorld().GetTaskScheduler();
            schedeler.Schedule(m_task, GetCurrentTime(), LOGIC_UPDATE_INTERVAL, LOGIC_UPDATE_INTERVAL);
#if COMBAT_CLIENT
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStartMoving(ParentObject.ID, true);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }
        #endregion

        public bool UpdateProjectile(FixPoint delta_time)
        {
            PositionComponent position_component = ParentObject.GetComponent(PositionComponent.ID) as PositionComponent;
            Vector3FP new_position = position_component.CurrentPosition + m_param.m_facing * (m_speed * delta_time);
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
                Entity entity = entity_manager.GetObject(list[i]);
                if (entity == null)
                    continue;
                PositionComponent position_component = entity.GetComponent(PositionComponent.ID) as PositionComponent;
                if (position_component.Height <= FixPoint.Zero) //ZZWTODO
                    continue;
                if (!FactionRelation.IsFactionSatisfied(GetOwnerPlayer().GetFaction(entity.GetOwnerPlayerID()), m_collision_faction))
                    continue;
                Explode(entity);
                return true;
            }
            return false;
        }

        public void Explode(Entity entity)
        {
#if COMBAT_CLIENT
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStopMoving(ParentObject.ID, true);
            GetLogicWorld().AddRenderMessage(msg);
            if (m_collision_sound_cfgid > 0)
            {
                PlaySoundMessage sound_msg = RenderMessage.Create<PlaySoundMessage>();
                sound_msg.Construct(GetOwnerEntityID(), m_collision_sound_cfgid, FixPoint.MinusOne);
                GetLogicWorld().AddRenderMessage(sound_msg);
            }
#endif
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
            if (entity != null)
                ApplyGenerator(entity);
            EntityUtil.KillEntity((Entity)ParentObject, ParentObject.ID);
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