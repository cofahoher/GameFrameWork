using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ComponentTypeRegistry
    {
        const int RENDER_COMPONENT_FIRST_ID = 10000;
        public static bool IsLogicComponent(int component_type_id)
        {
            return component_type_id < RENDER_COMPONENT_FIRST_ID;
        }
        public static bool IsRenderComponent(int component_type_id)
        {
            return component_type_id > RENDER_COMPONENT_FIRST_ID;
        }
        public static System.Type GetComponentClassType(int component_type_id)
        {
            if (m_component_registry == null)
                InitializeRegistry();
            System.Type type = null;
            m_component_registry.TryGetValue(component_type_id, out type);
            return type;
        }
        public static Component CreateComponent(int component_type_id)
        {
            if (m_component_registry == null)
                InitializeRegistry();
            System.Type type = null;
            if (!m_component_registry.TryGetValue(component_type_id, out type))
                return null;
            return System.Activator.CreateInstance(type) as Component;
        }

        #region 组件ID
        //【0001, 1000】Object Component
        public const int CT_TurnManagerComponent = 1;

        //【1001, 2000】Player Component
        public const int CT_FactionComponent = 1001;
        public const int CT_PlayerAIComponent = 1002;
        public const int CT_PlayerTargetingComponent = 1003;

        //【2001, 3000】Entity Component
        public const int CT_PositionComponent = 2001;
        public const int CT_LocomotorComponent = 2002;
        public const int CT_DamagableComponent = 2003;
        public const int CT_ManaComponent = 2004;
        public const int CT_DeathComponent = 2005;
        public const int CT_StateComponent = 2006;
        public const int CT_AttributeManagerComponent = 2007;
        public const int CT_SkillManagerComponent = 2008;
        public const int CT_EffectManagerComponent = 2009;
        public const int CT_DamageModificationComponent = 2010;
        public const int CT_AIComponent = 2011;

        //【3001, 4000】Skill Component
        public const int CT_SkillDefinitionComponent = 3001;
        public const int CT_WeaponSkillComponent = 3002;
        public const int CT_EffectGeneratorSkillComponent = 3003;
        public const int CT_CreateObjectSkillComponent = 3004;

        //【4001, 5000】Effect Component
        public const int CT_DamageEffectComponent = 4001;
        public const int CT_HealEffectComponent = 4002;
        public const int CT_AddStateEffectComponent = 4003;
        public const int CT_ApplyGeneratorEffectComponent = 4004;

        //【10001，】Render Component
        #endregion


        static Dictionary<int, System.Type> m_component_registry = null;
        static void InitializeRegistry()
        {
            m_component_registry = new Dictionary<int, System.Type>();

            //m_component_registry[] = typeof();
            //【0001, 1000】Object Component
            m_component_registry[CT_TurnManagerComponent] = typeof(TurnManagerComponent);

            //【1001, 2000】Player Component
            m_component_registry[CT_FactionComponent] = typeof(FactionComponent);
            m_component_registry[CT_PlayerAIComponent] = typeof(PlayerAIComponent);
            m_component_registry[CT_PlayerTargetingComponent] = typeof(PlayerTargetingComponent);

            //【2001, 3000】Entity Component
            m_component_registry[CT_PositionComponent] = typeof(PositionComponent);
            m_component_registry[CT_LocomotorComponent] = typeof(LocomotorComponent);
            m_component_registry[CT_DamagableComponent] = typeof(DamagableComponent);
            m_component_registry[CT_ManaComponent] = typeof(ManaComponent);
            m_component_registry[CT_DeathComponent] = typeof(DeathComponent);
            m_component_registry[CT_StateComponent] = typeof(StateComponent);
            m_component_registry[CT_AttributeManagerComponent] = typeof(AttributeManagerComponent);
            m_component_registry[CT_SkillManagerComponent] = typeof(SkillManagerComponent);
            m_component_registry[CT_EffectManagerComponent] = typeof(EffectManagerComponent);
            m_component_registry[CT_DamageModificationComponent] = typeof(DamageModificationComponent);
            m_component_registry[CT_AIComponent] = typeof(AIComponent);

            //【3001, 4000】Skill Component
            m_component_registry[CT_SkillDefinitionComponent] = typeof(SkillDefinitionComponent);
            m_component_registry[CT_WeaponSkillComponent] = typeof(WeaponSkillComponent);
            m_component_registry[CT_EffectGeneratorSkillComponent] = typeof(EffectGeneratorSkillComponent);
            m_component_registry[CT_CreateObjectSkillComponent] = typeof(CreateObjectSkillComponent);

            //【4001, 5000】Effect Component
            m_component_registry[CT_DamageEffectComponent] = typeof(DamageEffectComponent);
            m_component_registry[CT_HealEffectComponent] = typeof(HealEffectComponent);
            m_component_registry[CT_AddStateEffectComponent] = typeof(AddStateEffectComponent);
            m_component_registry[CT_ApplyGeneratorEffectComponent] = typeof(ApplyGeneratorEffectComponent);
        }
    }
}