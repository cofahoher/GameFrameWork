using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class CreateObjectSkillComponent : SkillComponent, INeedTaskService
    {
        public static readonly int ComboType_Time = (int)CRC.Calculate("Time");
        public static readonly int ComboType_Angle = (int)CRC.Calculate("Angle");

        //配置数据
        int m_object_type_id = 0;
        int m_object_proto_id = 0;
        FixPoint m_object_life_time = FixPoint.Zero;
        int m_generator_cfgid = 0;
        Vector3FP m_offset;
        int m_combo_type_crc = 0;
        int m_combo_attack_cnt = 1;
        FixPoint m_combo_interval = FixPoint.Zero;

        //运行数据
        EffectGenerator m_generator;
        int m_remain_attack_cnt = 0;
        ComponentCommonTask m_task;

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            if (m_generator_cfgid != 0)
                m_generator = GetLogicWorld().GetEffectManager().CreateGenerator(m_generator_cfgid, GetOwnerEntity());
            if (m_combo_type_crc == 0)
                m_combo_type_crc = ComboType_Time;
        }

        protected override void OnDestruct()
        {
            if (m_generator != null)
            {
                GetLogicWorld().GetEffectManager().DestroyGenerator(m_generator.ID, GetOwnerEntityID());
                m_generator = null;
            }

            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
        }
        #endregion

        public override void Inflict(FixPoint start_time)
        {
            m_remain_attack_cnt = m_combo_attack_cnt;
            if (m_combo_type_crc == ComboType_Time)
            {
                CreateOneObject(0);
                if (m_combo_attack_cnt > 1)
                {
                    if (m_task == null)
                    {
                        m_task = LogicTask.Create<ComponentCommonTask>();
                        m_task.Construct(this);
                    }
                    var schedeler = GetLogicWorld().GetTaskScheduler();
                    schedeler.Schedule(m_task, GetCurrentTime(), m_combo_interval, m_combo_interval);
                }
            }
            else if (m_combo_type_crc == ComboType_Angle)
            {
                for (int i = 0; i < m_combo_attack_cnt; ++i)
                    CreateOneObject(i);
            }
        }

        public override void Deactivate(bool force)
        {
            if (m_generator != null)
                m_generator.Deactivate();
            if (m_task != null)
                m_task.Cancel();
        }

        public void OnTaskService(FixPoint delta_time)
        {
            CreateOneObject(m_combo_attack_cnt - m_remain_attack_cnt);
            if (m_remain_attack_cnt <= 0)
            {
                if (m_task != null)
                    m_task.Cancel();
            }
        }

        void CreateOneObject(int index)
        {
            --m_remain_attack_cnt;
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
            if (m_combo_type_crc == ComboType_Angle)
            {
                if (m_combo_attack_cnt % 2 == 1)
                {
                    FixPoint angle_offset = m_combo_interval * (FixPoint)((index + 1)/2);
                    if (index % 2 == 0)
                        angle += angle_offset;
                    else
                        angle -= angle_offset;
                }
                else
                {
                    FixPoint angle_offset = m_combo_interval * ((FixPoint)(index / 2) + FixPoint.One / FixPoint.Two);
                    if (index % 2 == 0)
                        angle += angle_offset;
                    else
                        angle -= angle_offset;
                }
                FixPoint radian = FixPoint.Degree2Radian(-angle);
                facing.x = FixPoint.Cos(radian);
                facing.z = FixPoint.Sin(radian);
            }

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

            m_current_target = entity_manager.CreateObject(object_context);

            DeathComponent death_component = m_current_target.GetComponent(DeathComponent.ID) as DeathComponent;
            if (death_component != null && m_object_life_time > FixPoint.Zero)
                death_component.SetLifeTime(m_object_life_time);

            SummonedEntityComponent summoned_component = m_current_target.GetComponent(SummonedEntityComponent.ID) as SummonedEntityComponent;
            if (summoned_component != null)
                summoned_component.SetMaster(owner_entity);

            ProjectileComponent projectile_component = m_current_target.GetComponent(ProjectileComponent.ID) as ProjectileComponent;
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
                    Skill owner_skill = GetOwnerSkill();
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
                m_generator.Activate(app_data, m_current_target);
                RecyclableObject.Recycle(app_data);
            }
            m_current_target = null;
        }
    }
}