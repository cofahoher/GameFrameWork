using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class DamageOverTimeEffectComponent : EffectComponent, INeedTaskService
    {
        //配置数据
        int m_damage_type_id = 0;
        Formula m_damage_amount = RecyclableObject.Create<Formula>();
        int m_damage_render_effect_cfgid = 0;
        int m_damage_sound_cfgid = 0;
        FixPoint m_period = FixPoint.One;

        //运行数据
        ComponentCommonTask m_period_task;

        protected override void OnDestruct()
        {
            RecyclableObject.Recycle(m_damage_amount);
            m_damage_amount = null;

            if (m_period_task != null)
            {
                m_period_task.Cancel();
                LogicTask.Recycle(m_period_task);
                m_period_task = null;
            }
        }

        public override void Apply()
        {
            ApplyDamage();
            if (m_period > FixPoint.Zero)
            {
                if (m_period_task == null)
                {
                    m_period_task = LogicTask.Create<ComponentCommonTask>();
                    m_period_task.Construct(this);
                }
                var schedeler = GetLogicWorld().GetTaskScheduler();
                schedeler.Schedule(m_period_task, GetCurrentTime(), m_period, m_period);
            }
        }


        void ApplyDamage()
        {
            EffectDefinitionComponent definition_component = ((Effect)ParentObject).GetDefinitionComponent();
            EntityManager entity_manager = GetLogicWorld().GetEntityManager();
            Entity attacker = entity_manager.GetObject(definition_component.OriginalEntityID);
            Entity target = entity_manager.GetObject(definition_component.TargetEntityID);

            DamagableComponent damageable_component = target.GetComponent(DamagableComponent.ID) as DamagableComponent;
            if (damageable_component == null)
                return;
            Damage damage = RecyclableObject.Create<Damage>();
            damage.m_attacker_id = definition_component.OriginalEntityID;
            damage.m_defender_id = definition_component.TargetEntityID;
            damage.m_damage_type = m_damage_type_id;
            damage.m_damage_amount = m_damage_amount.Evaluate(this);
            damage.m_damage_amount = DamageSystem.Instance.CalculateDamageAmount(m_damage_type_id, damage.m_damage_amount, attacker, target);
            damage.m_render_effect_cfgid = m_damage_render_effect_cfgid;
            damage.m_sound_cfgid = m_damage_sound_cfgid;
            damageable_component.TakeDamage(damage);
        }

        public override void Unapply()
        {
            if (m_period_task != null)
                m_period_task.Cancel();
        }

        public void OnTaskService(FixPoint delta_time)
        {
            ApplyDamage();
        }
    }
}