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
        int m_object_count = 1;
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
            if (m_object_count > 1)
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
            --m_remain_count;
            LogicWorld logic_world = GetLogicWorld();
            EntityManager entity_manager = logic_world.GetEntityManager();
            IConfigProvider config = logic_world.GetConfigProvider();
            Player owner_player = GetOwnerPlayer();
            Entity owner_entity = GetOwnerEntity();
            PositionComponent owner_position_cmp = owner_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            Vector3FP source_pos = owner_position_cmp.CurrentPosition;

            Vector2FP xz_facing = owner_position_cmp.Facing2D;
            FixPoint angle = owner_position_cmp.FacingAngle;
            Vector3FP facing = new Vector3FP(xz_facing.x, FixPoint.Zero, xz_facing.z);
            Vector2FP side = xz_facing.Perpendicular();
            Vector2FP xz_offset = xz_facing * m_offset.z + side * m_offset.x;

            ObjectTypeData type_data = config.GetObjectTypeData(m_object_type_id);
            if (type_data == null)
                return;
            BirthPositionInfo birth_info = new BirthPositionInfo(source_pos.x + xz_offset.x, source_pos.y + m_offset.y, source_pos.z + xz_offset.z, angle, owner_position_cmp.GetCurrentSceneSpace());
            ObjectCreationContext object_context = new ObjectCreationContext();
            object_context.m_object_proxy_id = owner_player.ProxyID;
            object_context.m_object_type_id = m_object_type_id;
            object_context.m_object_proto_id = m_object_proto_id;
            object_context.m_birth_info = birth_info;
            object_context.m_type_data = type_data;
            object_context.m_proto_data = config.GetObjectProtoData(m_object_proto_id);
            object_context.m_logic_world = logic_world;
            object_context.m_owner_id = owner_player.ID;
            object_context.m_is_ai = true;
            object_context.m_is_local = owner_player.IsLocal;
            Entity obj = entity_manager.CreateObject(object_context);
            if (m_revert_when_unapply)
            {
                if (m_objects_id == null)
                    m_objects_id = new List<int>();
                m_objects_id.Add(obj.ID);
            }

            DeathComponent death_component = obj.GetComponent(DeathComponent.ID) as DeathComponent;
            if (death_component != null && m_object_life_time > FixPoint.Zero)
                death_component.SetLifeTime(m_object_life_time);

            SummonedEntityComponent summoned_component = obj.GetComponent(SummonedEntityComponent.ID) as SummonedEntityComponent;
            if (summoned_component != null)
                summoned_component.SetMaster(owner_entity);
        }

        public override void Unapply()
        {
            if (!m_revert_when_unapply || m_objects_id == null)
                return;
            for (int i = 0; i < m_objects_id.Count; ++i )
                GetLogicWorld().GetEntityManager().DestroyObject(m_objects_id[i]);
        }

        public void OnTaskService(FixPoint delta_time)
        {
            CreateOneObject();
            if (m_remain_count <= 0)
            {
                if (m_task != null)
                    m_task.Cancel();
            }
        }
    }
}