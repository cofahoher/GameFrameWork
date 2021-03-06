﻿using System.Collections;
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

        public override void OnResurrect()
        {
            m_current_health = m_current_max_health;
        }
        #endregion

        public FixPoint MaxHealth
        {
            get { return m_current_max_health; }
            set
            {
                if (value > m_current_max_health && m_current_max_health > FixPoint.Zero)
                    CurrentHealth = CurrentHealth * value / m_current_max_health;
                m_current_max_health = value;
            }
        }

        public FixPoint CurrentHealth
        {
            get { return m_current_health; }
            set
            {
                FixPoint delta_health = value - m_current_health;
                if (ObjectUtil.IsDead(ParentObject))
                    return;
                ChangeHealth(delta_health, ParentObject.ID);
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
                ChangeHealth(-damage.m_damage_amount, damage.m_attacker_id);
                RecyclableObject.Recycle(damage);
                return;
            }
            if (!IsEnable())
            {
                RecyclableObject.Recycle(damage);
                return;
            }
            Entity attacker = GetLogicWorld().GetEntityManager().GetObject(damage.m_attacker_id);
            LastDamage = damage;
            FixPoint original_damage_amount = damage.m_damage_amount;
            FixPoint final_damage_amount = CalculateFinalDamageAmount(damage, attacker);
            ChangeHealth(-final_damage_amount, damage.m_attacker_id);
            ParentObject.SendSignal(SignalType.TakeDamage, damage);
#if COMBAT_CLIENT
            TakeDamageRenderMessage msg = RenderMessage.Create<TakeDamageRenderMessage>();
            msg.Construct(GetOwnerEntityID(), original_damage_amount, final_damage_amount, damage.m_render_effect_cfgid, damage.m_sound_cfgid);
            GetLogicWorld().AddRenderMessage(msg);
#endif
            if (m_current_health <= 0)
                ApplyExperience(attacker);
            GetLogicWorld().OnCauseDamage(attacker, (Entity)ParentObject, damage);
        }

        FixPoint CalculateFinalDamageAmount(Damage damage, Entity attacker)
        {
            FixPoint damage_amount = damage.m_damage_amount;
            if (attacker != null)
            {
                DamageModificationComponent attacker_cmp = attacker.GetComponent(DamageModificationComponent.ID) as DamageModificationComponent;
                if(attacker_cmp != null)
                    damage_amount = attacker_cmp.ApplyModifiersToDamage(damage, damage_amount, ParentObject, true);
            }
            DamageModificationComponent self_cmp = ParentObject.GetComponent(DamageModificationComponent.ID) as DamageModificationComponent;
            if (self_cmp != null)
                damage_amount = self_cmp.ApplyModifiersToDamage(damage, damage_amount, attacker, false);
            return damage_amount;
        }

        void ChangeHealth(FixPoint delta_health, int source_id)
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
                EntityUtil.KillEntity(ParentObject as Entity, source_id);
        }

        void ApplyExperience(Entity attacker)
        {
            if (attacker == null || attacker == ParentObject)
                return;
            LevelComponent level_cmp = ParentObject.GetComponent(LevelComponent.ID) as LevelComponent;
            if (level_cmp == null)
                return;
            int xp = level_cmp.BeKilledExperience;
            if (xp == 0)
                return;
            level_cmp = attacker.GetComponent(LevelComponent.ID) as LevelComponent;
            if (level_cmp == null)
                return;
            level_cmp.AddExperience(xp);
        }
    }
}