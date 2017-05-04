using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class CreateObjectSkillComponent : SkillComponent
    {
        int m_object_type_id = -1;
        int m_object_proto_id = -1;
        Vector3FP m_offset;
        FixPoint m_speed;

        #region 初始化/销毁
        #endregion

        public override void Inflict(FixPoint start_time)
        {
            LogicWorld logic_world = GetLogicWorld();
            EntityManager entity_manager = logic_world.GetEntityManager();
            IConfigProvider config = logic_world.GetConfigProvider();
            Player owner_player = GetOwnerPlayer();
            PositionComponent owner_position_cmp = GetOwnerEntity().GetComponent(PositionComponent.ID) as PositionComponent;

            Vector2FP facing = owner_position_cmp.Facing;
            Vector2FP side = facing.Perpendicular();
            Vector2FP xz_offset = facing * m_offset.z + side * m_offset.x;
            Vector3FP original = owner_position_cmp.CurrentPosition;

            ObjectTypeData type_data = config.GetObjectTypeData(m_object_type_id);
            if (type_data == null)
                return;
            BirthPositionInfo birth_info = new BirthPositionInfo(original.x + xz_offset.x, original.y + m_offset.y, original.z + xz_offset.z, owner_position_cmp.CurrentAngle);
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

            ProjectileComponent projectile_component = obj.GetComponent(ProjectileComponent.ID) as ProjectileComponent;
            if (projectile_component != null)
            {
            }
        }

        public override void Deactivate()
        {
        }
    }
}