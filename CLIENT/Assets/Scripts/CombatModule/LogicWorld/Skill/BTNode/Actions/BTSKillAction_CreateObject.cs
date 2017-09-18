using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTSKillAction_CreateObject : BTAction
    {
        //配置数据
        int m_object_type_id = 0;
        int m_object_proto_id = 0;
        FixPoint m_object_life_time = FixPoint.Zero;
        int m_generator_cfgid = 0;
        Vector3FP m_offset;
        FixPoint m_angle_offset = FixPoint.Zero;

        //运行数据
        EffectGenerator m_generator;

        public BTSKillAction_CreateObject()
        {
        }

        public BTSKillAction_CreateObject(BTSKillAction_CreateObject prototype)
            : base(prototype)
        {
            m_object_type_id = prototype.m_object_type_id;
            m_object_proto_id = prototype.m_object_proto_id;
            m_object_life_time = prototype.m_object_life_time;
            m_generator_cfgid = prototype.m_generator_cfgid;
            m_offset = prototype.m_offset;
            m_angle_offset = prototype.m_angle_offset;
        }

        protected override void ResetRuntimeData()
        {
            if (m_generator != null)
            {
                SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
                GetLogicWorld().GetEffectManager().DestroyGenerator(m_generator.ID, skill_component.GetOwnerEntityID());
                m_generator = null;
            }
        }

        public override void ClearRunningTrace()
        {
            if (m_generator != null)
                m_generator.Deactivate();
        }

        protected override void OnActionEnter()
        {
            if (m_generator_cfgid > 0 && m_generator == null)
            {
                SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
                m_generator = GetLogicWorld().GetEffectManager().CreateGenerator(m_generator_cfgid, skill_component.GetOwnerEntity());
            }
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
            SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
            Skill owner_skill = skill_component.GetOwnerSkill();

            Target target = owner_skill.GetMajorTarget();
            LogicWorld logic_world = GetLogicWorld();
            EntityManager entity_manager = logic_world.GetEntityManager();
            IConfigProvider config = logic_world.GetConfigProvider();
            Player owner_player = owner_skill.GetOwnerPlayer();
            Entity owner_entity = owner_skill.GetOwnerEntity();
            PositionComponent owner_position_cmp = owner_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            Vector3FP source_pos = owner_position_cmp.CurrentPosition;

            Vector2FP xz_facing;
            FixPoint angle;
            Vector3FP facing;
            if (target == null)
            {
                xz_facing = owner_position_cmp.Facing2D;
                angle = owner_position_cmp.FacingAngle;
                facing.x = xz_facing.x;
                facing.y = FixPoint.Zero;
                facing.z = xz_facing.z;
            }
            else
            {
                Vector3FP target_pos = target.GetPosition(logic_world);
                xz_facing.x = target_pos.x - source_pos.x;
                xz_facing.z = target_pos.z - source_pos.z;
                xz_facing.Normalize();
                angle = xz_facing.ToDegree();
                facing = target_pos - source_pos;
                facing.Normalize();
            }
            Vector2FP side = xz_facing.Perpendicular();
            Vector2FP xz_offset = xz_facing * m_offset.z + side * m_offset.x;

            ObjectTypeData type_data = config.GetObjectTypeData(m_object_type_id);
            if (type_data == null)
                return;
            Vector3FP birth_position = new Vector3FP(source_pos.x + xz_offset.x, source_pos.y + m_offset.y, source_pos.z + xz_offset.z);
            BirthPositionInfo birth_info = new BirthPositionInfo(birth_position.x, birth_position.y, birth_position.z, angle, owner_position_cmp.GetCurrentSceneSpace());
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

            Entity created_object = entity_manager.CreateObject(object_context);
            skill_component.CurrentTarget = created_object;

            DeathComponent death_component = created_object.GetComponent(DeathComponent.ID) as DeathComponent;
            if (death_component != null && m_object_life_time > FixPoint.Zero)
                death_component.SetLifeTime(m_object_life_time);

            SummonedEntityComponent summoned_component = created_object.GetComponent(SummonedEntityComponent.ID) as SummonedEntityComponent;
            if (summoned_component != null)
                summoned_component.SetMaster(owner_entity);

            ProjectileComponent projectile_component = created_object.GetComponent(ProjectileComponent.ID) as ProjectileComponent;
            if (projectile_component != null)
            {
                ProjectileParameters param = RecyclableObject.Create<ProjectileParameters>();
                param.m_start_time = logic_world.GetCurrentTime();
                param.m_life_time = m_object_life_time;
                param.m_source_entity_id = owner_entity.ID;
                param.m_start_position = birth_position;
                param.m_fixed_facing = facing;
                if (target != null)
                {
                    param.m_target_entity_id = target.GetEntityID();
                    param.m_target_position = target.GetPosition(logic_world);
                }
                else
                {
                    if (owner_skill.GetDefinitionComponent().ExternalDataType == SkillDefinitionComponent.NeedExternalTarget)
                    {
                        param.m_target_entity_id = 0;
                        FixPoint range = owner_skill.GetDefinitionComponent().MaxRange;
                        if (range <= 0)
                            range = FixPoint.Ten;  //ZZWTODO
                        if (projectile_component.Speed > FixPoint.Zero)
                            param.m_life_time = range / projectile_component.Speed;
                        param.m_target_position = param.m_start_position + param.m_fixed_facing * range;
                    }
                }
                param.m_generator_id = m_generator == null ? 0 : m_generator.ID;
                projectile_component.InitParam(param);
            }
            else if (m_generator != null)
            {
                EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
                app_data.m_original_entity_id = owner_entity.ID;
                app_data.m_source_entity_id = owner_entity.ID;
                m_generator.Activate(app_data, created_object);
                RecyclableObject.Recycle(app_data);
            }
            skill_component.CurrentTarget = null;
        }

        protected override void OnActionExit()
        {
        }
    }
}