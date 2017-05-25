using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Effect : Object
    {
        EffectDefinitionComponent m_definition_component;
        bool m_is_applied = false;

        #region GETTER
        public EffectDefinitionComponent GetDefinitionComponent()
        {
            return m_definition_component;
        }

        public bool Applied
        {
            get { return m_is_applied; }
        }
        #endregion

        #region 初始化/销毁
        protected override void PostInitializeObject(ObjectCreationContext context)
        {
            m_definition_component = GetComponent(EffectDefinitionComponent.ID) as EffectDefinitionComponent;
        }

        protected override void OnDestruct()
        {
            m_definition_component = null;
        }
        #endregion

        #region ILogicOwnerInfo
        public override Object GetOwnerObject()
        {
            return GetOwnerEntity();
        }
        public override int GetOwnerPlayerID()
        {
            return GetOwnerEntity().GetOwnerPlayerID();
        }
        public override Player GetOwnerPlayer()
        {
            return GetOwnerEntity().GetOwnerPlayer();
        }
        public override int GetOwnerEntityID()
        {
            return m_context.m_owner_id;
        }
        public override Entity GetOwnerEntity()
        {
            return m_context.m_logic_world.GetEntityManager().GetObject(m_context.m_owner_id);
        }
        #endregion

        #region Effect流程
        public void Apply()
        {
            if (m_is_applied)
                return;
            m_is_applied = true;
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                EffectComponent cmp = enumerator.Current.Value as EffectComponent;
                if (cmp != null)
                    cmp.Apply();
            }
        }

        public void Unapply()
        {
            if (!m_is_applied)
                return;
            m_is_applied = false;
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                EffectComponent cmp = enumerator.Current.Value as EffectComponent;
                if (cmp != null)
                    cmp.Unapply();
            }
        }
        #endregion

        #region Variable
        //ZZW no need
        #endregion
    }

    public class EffectExpireTask : Task<LogicWorld>
    {
        int m_entity_id = 0;
        int m_effect_id = 0;

        public void Construct(int entity_id, int effect_id)
        {
            m_entity_id = entity_id;
            m_effect_id = effect_id;
        }

        public override void OnReset()
        {
            m_entity_id = 0;
            m_effect_id = 0;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            Entity entity = logic_world.GetEntityManager().GetObject(m_entity_id);
            if (entity == null)
                return;
            EffectRegistry registry = EntityUtil.GetEffectRegistry(entity);
            if (registry == null)
                return;
            Effect effect = registry.GetEffect(m_effect_id);
            if (effect == null)
                return;
            EffectDefinitionComponent definition_cmp = effect.GetDefinitionComponent();
            if (definition_cmp.ExpirationTime > logic_world.GetCurrentTime())
                return;
            registry.RemoveEffect(m_effect_id);
        }
    }
}