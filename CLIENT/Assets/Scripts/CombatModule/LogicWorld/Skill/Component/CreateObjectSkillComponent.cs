using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class CreateObjectSkillComponent : SkillComponent, INeedTaskService
    {
        //配置数据
        int m_object_type_id = 0;
        int m_object_proto_id = 0;
        FixPoint m_object_life_time = FixPoint.Zero;
        int m_generator_cfgid = 0;
        Vector3FP m_offset;
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
            Impact();
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

        public override void Deactivate()
        {
            if (m_generator != null)
                m_generator.Deactivate();
            if (m_task != null)
                m_task.Cancel();
        }

        public void OnTaskService(FixPoint delta_time)
        {
            Impact();
        }

        void Impact()
        {
            --m_remain_attack_cnt;
            if (m_remain_attack_cnt <= 0)
            {
                if (m_task != null)
                    m_task.Cancel();
            }

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

            DeathComponent death_component = obj.GetComponent(DeathComponent.ID) as DeathComponent;
            if (death_component != null && m_object_life_time > FixPoint.Zero)
            {
                death_component.SetLifeTime(m_object_life_time);
            }

            ProjectileComponent projectile_component = obj.GetComponent(ProjectileComponent.ID) as ProjectileComponent;
            if (projectile_component != null)
            {
                ProjectileParameters param = RecyclableObject.Create<ProjectileParameters>();
                param.m_life_time = m_object_life_time;
                param.m_source_entity_id = owner_entity.ID;
                if (target == null)
                    param.m_target_entity_id = 0;
                else
                    param.m_target_entity_id = target.GetEntityID();
                param.m_facing = facing;
                param.m_generator_id = m_generator == null ? 0 : m_generator.ID;
                projectile_component.InitParam(param);
            }
        }
    }
}