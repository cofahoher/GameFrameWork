using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class CreateObjectSkillComponent : SkillComponent
    {
        //配置数据
        int m_object_type_id = 0;
        int m_object_proto_id = 0;
        int m_generator_cfgid = 0;
        Vector3FP m_offset;

        //运行数据
        int m_generator_id = 0;

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            EffectGenerator generator = GetLogicWorld().GetEffectManager().CreateGenerator(m_generator_cfgid, GetOwnerEntity());
            if (generator != null)
                m_generator_id = generator.ID;
        }
        #endregion

        public override void Inflict(FixPoint start_time)
        {
            Target target = GetOwnerSkill().GetMajorTarget();

            LogicWorld logic_world = GetLogicWorld();
            EntityManager entity_manager = logic_world.GetEntityManager();
            IConfigProvider config = logic_world.GetConfigProvider();
            Player owner_player = GetOwnerPlayer();
            Entity owner_entity = GetOwnerEntity();
            PositionComponent owner_position_cmp = owner_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            Vector3FP source_pos = owner_position_cmp.CurrentPosition;

            Vector2FP xz_facing;
            FixPoint angle;
            Vector3FP facing;
            if (target == null)
            {
                xz_facing = owner_position_cmp.Facing;
                angle = owner_position_cmp.CurrentAngle;
                facing.x = xz_facing.x;
                facing.y = FixPoint.Zero;
                facing.z = xz_facing.z;
            }
            else
            {
                Vector3FP target_pos = target.GetPosition();
                xz_facing.x = target_pos.x - source_pos.x;
                xz_facing.z = target_pos.z - source_pos.z;
                xz_facing.Normalize();
                angle = FixPoint.Radian2Degree(FixPoint.Atan2(-xz_facing.z, xz_facing.x));
                facing = target_pos - source_pos;
                facing.Normalize();
            }
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

            ProjectileComponent projectile_component = obj.GetComponent(ProjectileComponent.ID) as ProjectileComponent;
            if (projectile_component != null)
            {
                ProjectileParameters param = RecyclableObject.Create<ProjectileParameters>();
                param.m_source_entity_id = owner_entity.ID;
                if (target == null)
                    param.m_target_entity_id = 0;
                else
                    param.m_target_entity_id = target.GetEntityID();
                param.m_facing = facing;
                param.m_generator_id = m_generator_id;
                projectile_component.InitParam(param);
            }
        }

        public override void Deactivate()
        {
        }
    }
}