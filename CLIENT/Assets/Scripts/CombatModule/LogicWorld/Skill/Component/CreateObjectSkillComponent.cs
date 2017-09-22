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
            Entity owner_entity = GetOwnerEntity();
            Target projectile_target = GetOwnerSkill().GetMajorTarget();
            FixPoint angle_offset = FixPoint.Zero;
            if (m_combo_type_crc == ComboType_Angle)
            {
                if (m_combo_attack_cnt % 2 == 1)
                {
                    angle_offset = m_combo_interval * (FixPoint)((index + 1) / 2);
                    if (index % 2 != 0)
                        angle_offset = -angle_offset;
                }
                else
                {
                    angle_offset = m_combo_interval * ((FixPoint)(index / 2) + FixPoint.One / FixPoint.Two);
                    if (index % 2 != 0)
                        angle_offset = -angle_offset;
                }
            }
            EntityUtil.CreateEntityForSkillAndEffect(this, owner_entity, projectile_target, m_offset, angle_offset, m_object_type_id, m_object_proto_id, m_object_life_time, m_generator);
        }
    }
}