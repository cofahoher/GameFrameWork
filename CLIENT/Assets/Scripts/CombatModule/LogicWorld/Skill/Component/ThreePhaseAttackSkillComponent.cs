using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    /*
     * 这是专门写的近战三段攻击，如果还要更复杂的，等待行为树技能
     */
    public partial class ThreePhaseAttackSkillComponent : SkillComponent
    {
        public const int THREE = 3;
        //配置数据
        int m_target_gathering_type = 0;
        FixPoint m_target_gathering_param1;
        FixPoint m_target_gathering_param2;
        int m_damage_type_id = 0;
        FixPoint[] m_inflict_time = new FixPoint[THREE];
        Formula[] m_damage_amount = new Formula[THREE];
        int[] m_generator_cfg_id = new int[THREE];

        //运行数据
        EffectGenerator[] m_generator = new EffectGenerator[THREE];
        ThreePhaseAttackInflictTask m_task;
        int m_next_impact = 0;

        public ThreePhaseAttackSkillComponent()
        {
            for (int i = 0; i < THREE; ++i)
            {
                m_inflict_time[i] = FixPoint.MinusOne;
                m_damage_amount[i] = RecyclableObject.Create<Formula>();
                m_generator_cfg_id[i] = 0;
                m_generator[i] = null;
            }
        }

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            EffectManager effect_manager = GetLogicWorld().GetEffectManager();
            Entity owner_entity = GetOwnerEntity();
            for (int i = 0; i < THREE; ++i)
            {
                m_generator[i] = effect_manager.CreateGenerator(m_generator_cfg_id[i], owner_entity);
            }
        }

        protected override void OnDestruct()
        {
            EffectManager effect_manager = GetLogicWorld().GetEffectManager();
            int owner_entity_id = GetOwnerEntityID();
            for (int i = 0; i < THREE; ++i)
            {
                RecyclableObject.Recycle(m_damage_amount[i]);
                m_damage_amount[i] = null;
                if (m_generator[i] != null)
                {
                    effect_manager.DestroyGenerator(m_generator[i].ID, owner_entity_id);
                    m_generator[i] = null;
                }
            }

            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
        }
        #endregion

        void BuildSkillTargets()
        {
            if (m_target_gathering_type != 0)
                ((Skill)ParentObject).BuildSkillTargets(m_target_gathering_type, m_target_gathering_param1, m_target_gathering_param2);
        }

        public override void Inflict(FixPoint start_time)
        {
            ScheduleNextImpact();
        }

        void ScheduleNextImpact()
        {
            FixPoint delay = m_inflict_time[m_next_impact];
            if (delay < FixPoint.Zero)
                return;
            if (m_task == null)
            {
                m_task = LogicTask.Create<ThreePhaseAttackInflictTask>();
                m_task.Construct(this);
            }
            var schedeler = GetLogicWorld().GetTaskScheduler();
            if (m_next_impact > 0)
                delay -= m_inflict_time[m_next_impact - 1];
            schedeler.Schedule(m_task, GetCurrentTime(), delay);
        }

        public void Impact()
        {
            BuildSkillTargets();
            Entity attacker = GetOwnerEntity();
            List<Target> targets = GetOwnerSkill().GetTargets();
            EffectGenerator generator = m_generator[m_next_impact];
            EffectApplicationData app_data = null;
            if (generator != null)
            {
                RecyclableObject.Create<EffectApplicationData>();
                app_data.m_original_entity_id = attacker.ID;
                app_data.m_source_entity_id = attacker.ID;
            }
            for (int i = 0; i < targets.Count; ++i)
            {
                m_current_target = targets[i].GetEntity();
                if (m_current_target == null)
                    continue;
                DamagableComponent damageable_component = m_current_target.GetComponent<DamagableComponent>();
                if (damageable_component == null)
                    continue;
                Damage damage = RecyclableObject.Create<Damage>();
                damage.m_attacker_id = attacker.ID;
                damage.m_defender_id = m_current_target.ID;
                damage.m_damage_type = m_damage_type_id;
                damage.m_damage_amount = m_damage_amount[m_next_impact].Evaluate(this);
                damage.m_damage_amount = DamageSystem.Instance.CalculateDamageAmount(m_damage_type_id, damage.m_damage_amount, attacker, m_current_target);
                damageable_component.TakeDamage(damage);
                if (generator != null)
                    generator.Activate(app_data, m_current_target);
            }
            m_current_target = null;
            RecyclableObject.Recycle(app_data);
            if (++m_next_impact < THREE)
                ScheduleNextImpact();
        }

        public override void Deactivate()
        {
            for (int i = 0; i < m_next_impact; ++i)
                m_generator[i].Deactivate();
            m_next_impact = 0;
        }
    }

    class ThreePhaseAttackInflictTask : Task<LogicWorld>
    {
        ThreePhaseAttackSkillComponent m_component;
        int m_index = 0;

        public void Construct(ThreePhaseAttackSkillComponent component)
        {
            m_component = component;
        }

        public override void OnReset()
        {
            m_component = null;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            m_component.Impact();
        }
    }
}
