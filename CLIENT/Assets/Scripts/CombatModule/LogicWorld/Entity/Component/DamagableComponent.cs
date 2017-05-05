using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class DamagableComponent : EntityComponent
    {
        //运行数据
        FixPoint m_current_max_health = FixPoint.Zero;
        FixPoint m_current_health = FixPoint.MinusOne;
        Damage m_last_damage;

        #region 初始化/销毁
        protected override void OnDestruct()
        {
            RecyclableObject.Recycle(m_last_damage);
            m_last_damage = null;
        }

        public override void InitializeComponent()
        {
            if (m_current_health < 0)
                m_current_health = m_current_max_health;
        }
        #endregion

        public FixPoint CurrentHealth
        {
            get { return m_current_health; }
            set
            {
                FixPoint delta_health = value - m_current_health;
                ChangeHealth(delta_health);
            }
        }

        public Damage LastDamage
        {
            get { return m_last_damage; }
            set
            {
                if (m_last_damage != null)
                    RecyclableObject.Recycle(m_last_damage);
                m_last_damage = value;
            }
        }

        public void TakeDamage(Damage damage)
        {
            if (ObjectUtil.IsDead(ParentObject))
            {
                RecyclableObject.Recycle(damage);
                return;
            }
            if (damage.m_damage_amount < 0)
            {
                ChangeHealth(-damage.m_damage_amount);
                RecyclableObject.Recycle(damage);
                return;
            }
            if (!IsEnable())
            {
                RecyclableObject.Recycle(damage);
                return;
            }
            LastDamage = damage;
            FixPoint final_damage_amount = CalculateFinalDamageAmount(damage);
            ChangeHealth(-final_damage_amount);
            ParentObject.SendSignal(SignalType.TakeDamage, damage);
        }

        FixPoint CalculateFinalDamageAmount(Damage damage)
        {
            FixPoint damage_amount = damage.m_damage_amount;
            Entity attacker = GetLogicWorld().GetEntityManager().GetObject(damage.m_attacker_id);
            if (attacker != null)
            {
                DamageModificationComponent attacker_cmp = attacker.GetComponent<DamageModificationComponent>(DamageModificationComponent.ID);
                if(attacker_cmp != null)
                    damage_amount = attacker_cmp.ApplyModifiersToDamage(damage, damage_amount, ParentObject, true);
            }
            DamageModificationComponent self_cmp = ParentObject.GetComponent<DamageModificationComponent>(DamageModificationComponent.ID);
            if (self_cmp != null)
                damage_amount = self_cmp.ApplyModifiersToDamage(damage, damage_amount, attacker, false);
            return damage_amount;
        }

        void ChangeHealth(FixPoint delta_health)
        {
            if (delta_health > 0)
            {
                if (m_current_health + delta_health > m_current_max_health)
                    delta_health = m_current_max_health - m_current_health;
            }
            else
            {
                if (-delta_health > m_current_health)
                    delta_health = -m_current_health;
            }
            if (delta_health == 0)
                return;

            m_current_health += delta_health;

#if COMBAT_CLIENT
            ChangeHealthRenderMessage msg = RenderMessage.Create<ChangeHealthRenderMessage>();
            msg.Construct(ParentObject.ID, delta_health, m_current_health);
            GetLogicWorld().AddRenderMessage(msg);
#endif

            if (m_current_health <= 0)
            {
                DeathComponent death_component = ParentObject.GetComponent<DeathComponent>(DeathComponent.ID);
                if (death_component != null)
                    death_component.KillOwner();
                else
                    EntityUtil.KillEntity(ParentObject as Entity);
                ParentObject.SendSignal(SignalType.Die);
            }
        }
    }
}