using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class CreateObjectEffectComponent : EffectComponent
    {
        //配置数据
        int m_object_type_id = 0;
        int m_object_proto_id = 0;
        FixPoint m_object_life_time = FixPoint.Zero;
        Vector3FP m_offset;
        bool m_revert_when_unapply = true;
        //运行数据
        int m_object_id = 0;

        public override void Apply()
        {
            LogicWorld logic_world = GetLogicWorld();
            EntityManager entity_manager = logic_world.GetEntityManager();
            IConfigProvider config = logic_world.GetConfigProvider();
            Player owner_player = GetOwnerPlayer();
            Entity owner_entity = GetOwnerEntity();
            PositionComponent owner_position_cmp = owner_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            Vector3FP source_pos = owner_position_cmp.CurrentPosition;

            Vector2FP xz_facing = owner_position_cmp.Facing2D;
            FixPoint angle = owner_position_cmp.CurrentAngle;
            Vector3FP facing = new Vector3FP(xz_facing.x, FixPoint.Zero, xz_facing.z);
            Vector2FP side = xz_facing.Perpendicular();
            Vector2FP xz_offset = xz_facing * m_offset.z + side * m_offset.x;

            ObjectTypeData type_data = config.GetObjectTypeData(m_object_type_id);
            if (type_data == null)
                return;
            BirthPositionInfo birth_info = new BirthPositionInfo(source_pos.x + xz_offset.x, source_pos.y + m_offset.y, source_pos.z + xz_offset.z, angle);
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
            m_object_id = obj.ID;

            DeathComponent death_component = obj.GetComponent(DeathComponent.ID) as DeathComponent;
            if (death_component != null && m_object_life_time > FixPoint.Zero)
                death_component.SetLifeTime(m_object_life_time);

            SummonedEntityComponent summoned_component = obj.GetComponent(SummonedEntityComponent.ID) as SummonedEntityComponent;
            if (summoned_component != null)
                summoned_component.SetMaster(owner_entity);
        }

        public override void Unapply()
        {
            if (!m_revert_when_unapply || m_object_id == 0)
                return;
            GetLogicWorld().GetEntityManager().DestroyObject(m_object_id);
        }
    }
}