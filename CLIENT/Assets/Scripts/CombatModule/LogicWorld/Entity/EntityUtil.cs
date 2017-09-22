using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EntityUtil
    {
        static FixPoint ms_death_delay = FixPoint.One / FixPoint.Ten;

        public static void KillEntity(Entity entity, int killer_id)
        {
            DeathComponent death_component = entity.GetComponent(DeathComponent.ID) as DeathComponent;
            if (death_component != null)
            {
                death_component.KillOwner(killer_id);
            }
            else
            {
                //Player player = entity.GetOwnerPlayer();
                //Entity killer = entity.GetLogicWorld().GetEntityManager().GetObject(killer_id);
                //player.OnEntityBeKilled(killer, entity);
                //if (killer != null)
                //{
                //    Player killer_player = killer.GetOwnerPlayer();
                //    killer_player.OnKillEntity(killer, entity);
                //}

                //没有死亡组件的就从简了
                entity.SendSignal(SignalType.Die);
                entity.GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.Die, entity.ID);
                entity.DeletePending = true;
                var schedeler = entity.GetLogicWorld().GetTaskScheduler();
                DeleteEntityTask delete_task = LogicTask.Create<DeleteEntityTask>();
                delete_task.Construct(entity.ID);
                schedeler.Schedule(delete_task, entity.GetCurrentTime(), ms_death_delay);
            }
        }

        public static EffectGeneratorRegistry GetEffectGeneratorRegistry(Entity entity)
        {
            if (entity == null)
                return null;
            EffectManagerComponent cmp = entity.GetComponent(EffectManagerComponent.ID) as EffectManagerComponent;
            if (cmp == null)
                return null;
            return cmp.GetEffectGeneratorRegistry();
        }

        public static EffectRegistry GetEffectRegistry(Entity entity)
        {
            if (entity == null)
                return null;
            EffectManagerComponent cmp = entity.GetComponent(EffectManagerComponent.ID) as EffectManagerComponent;
            if (cmp == null)
                return null;
            return cmp.GetEffectRegistry();
        }

        public static bool IsCategory(Entity entity, int category)
        {
            EntityDefinitionComponent component = entity.GetComponent(EntityDefinitionComponent.ID) as EntityDefinitionComponent;
            if (component == null)
                return false;
            return component.IsCategory(category);
        }

        public static Attribute GetAttribute(Entity entity, string attribute_name)
        {
            int attribute_id = (int)CRC.Calculate(attribute_name);
            return GetAttribute(entity, attribute_id);
        }

        public static Attribute GetAttribute(Entity entity, int attribute_id)
        {
            if (entity == null)
                return null;
            AttributeManagerComponent attribute_manager_component = entity.GetComponent(AttributeManagerComponent.ID) as AttributeManagerComponent;
            if (attribute_manager_component == null)
                return null;
            Attribute attribute = attribute_manager_component.GetAttributeByID(attribute_id);
            return attribute;
        }

        public static FixPoint GetAttributeValue(Entity entity, string attribute_name)
        {
            int attribute_id = (int)CRC.Calculate(attribute_name);
            return GetAttributeValue(entity, attribute_id);
        }

        public static FixPoint GetAttributeValue(Entity entity, int attribute_id)
        {
            if (entity == null)
                return FixPoint.Zero;
            AttributeManagerComponent attribute_manager_component = entity.GetComponent(AttributeManagerComponent.ID) as AttributeManagerComponent;
            if (attribute_manager_component == null)
                return FixPoint.Zero;
            Attribute attribute = attribute_manager_component.GetAttributeByID(attribute_id);
            if (attribute == null)
                return FixPoint.Zero;
            return attribute.Value;
        }

        public static Entity CreateEntityForSkillAndEffect(Component caller_component, Entity owner_entity, Target projectile_target, Vector3FP position_offset, FixPoint angle_offset, int object_type_id, int object_proto_id, FixPoint object_life_time, EffectGenerator attached_generator)
        {
            LogicWorld logic_world = owner_entity.GetLogicWorld();
            IConfigProvider config = logic_world.GetConfigProvider();
            ObjectTypeData type_data = config.GetObjectTypeData(object_type_id);
            if (type_data == null)
                return null;

            PositionComponent owner_position_cmp = owner_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            Vector3FP source_pos = owner_position_cmp.CurrentPosition;

            Vector2FP xz_facing;
            FixPoint angle;
            Vector3FP facing;
            if (projectile_target == null)
            {
                xz_facing = owner_position_cmp.Facing2D;
                angle = owner_position_cmp.FacingAngle;
                facing.x = xz_facing.x;
                facing.y = FixPoint.Zero;
                facing.z = xz_facing.z;
            }
            else
            {
                Vector3FP target_pos = projectile_target.GetPosition(logic_world);
                xz_facing.x = target_pos.x - source_pos.x;
                xz_facing.z = target_pos.z - source_pos.z;
                xz_facing.Normalize();
                angle = xz_facing.ToDegree();
                facing = target_pos - source_pos;
                facing.Normalize();
            }
            Vector2FP side = xz_facing.Perpendicular();
            Vector2FP xz_offset = xz_facing * position_offset.z + side * position_offset.x;

            if (angle_offset != FixPoint.Zero)
            {
                angle += angle_offset;
                FixPoint radian = FixPoint.Degree2Radian(-angle);
                facing.x = FixPoint.Cos(radian);
                facing.z = FixPoint.Sin(radian);
            }

            Vector3FP birth_position = new Vector3FP(source_pos.x + xz_offset.x, source_pos.y + position_offset.y, source_pos.z + xz_offset.z);
            BirthPositionInfo birth_info = new BirthPositionInfo(birth_position.x, birth_position.y, birth_position.z, angle, owner_position_cmp.GetCurrentSceneSpace());

            Player owner_player = owner_entity.GetOwnerPlayer();
            ObjectCreationContext object_context = new ObjectCreationContext();
            object_context.m_object_proxy_id = owner_player.ProxyID;
            object_context.m_object_type_id = object_type_id;
            object_context.m_object_proto_id = object_proto_id;
            object_context.m_birth_info = birth_info;
            object_context.m_type_data = type_data;
            object_context.m_proto_data = config.GetObjectProtoData(object_proto_id);
            object_context.m_logic_world = logic_world;
            object_context.m_owner_id = owner_player.ID;
            object_context.m_is_ai = true;
            object_context.m_is_local = owner_player.IsLocal;

            Entity created_entity = logic_world.GetEntityManager().CreateObject(object_context);

            DeathComponent death_component = created_entity.GetComponent(DeathComponent.ID) as DeathComponent;
            if (death_component != null && object_life_time > FixPoint.Zero)
                death_component.SetLifeTime(object_life_time);

            SummonedEntityComponent summoned_component = created_entity.GetComponent(SummonedEntityComponent.ID) as SummonedEntityComponent;
            if (summoned_component != null)
                summoned_component.SetMaster(owner_entity);

            ProjectileComponent projectile_component = created_entity.GetComponent(ProjectileComponent.ID) as ProjectileComponent;
            if (projectile_component != null)
            {
                ProjectileParameters param = RecyclableObject.Create<ProjectileParameters>();
                param.m_start_time = logic_world.GetCurrentTime();
                param.m_life_time = object_life_time;
                param.m_source_entity_id = owner_entity.ID;
                param.m_start_position = birth_position;
                param.m_fixed_facing = facing;
                if (projectile_target != null)
                {
                    param.m_target_entity_id = projectile_target.GetEntityID();
                    param.m_target_position = projectile_target.GetPosition(logic_world);
                }
                else
                {
                    Skill owner_skill = null;
                    SkillComponent skill_componnet = caller_component as SkillComponent;
                    if (skill_componnet != null)
                        owner_skill = skill_componnet.GetOwnerSkill();
                    if (owner_skill != null && owner_skill.GetDefinitionComponent().ExternalDataType == SkillDefinitionComponent.NeedExternalTarget)
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
                param.m_generator_id = attached_generator == null ? 0 : attached_generator.ID;
                projectile_component.InitParam(param);
            }
            else if (attached_generator != null)
            {
                EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
                app_data.m_original_entity_id = owner_entity.ID;
                app_data.m_source_entity_id = owner_entity.ID;
                attached_generator.Activate(app_data, created_entity);
                RecyclableObject.Recycle(app_data);
            }
            return created_entity;
        }
    }
}