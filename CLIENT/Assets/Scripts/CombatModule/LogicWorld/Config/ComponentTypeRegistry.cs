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

        public static System.Type ComponentID2Type(int component_type_id)
        {
            if (m_component_id2type == null)
                InitializeRegistry();
            System.Type type = null;
            m_component_id2type.TryGetValue(component_type_id, out type);
            return type;
        }

        public static int ComponentType2ID(System.Type type)
        {
            if (m_component_type2id == null)
                InitializeRegistry();
            int component_type_id = -1; ;
            m_component_type2id.TryGetValue(type, out component_type_id);
            return component_type_id;
        }

        public static Component CreateComponent(int component_type_id)
        {
            if (m_component_id2type == null)
                InitializeRegistry();
            System.Type type = null;
            if (!m_component_id2type.TryGetValue(component_type_id, out type))
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
        public const int CT_ModelComponent = 10001;
        #endregion


        static Dictionary<int, System.Type> m_component_id2type = null;
        static Dictionary<System.Type, int> m_component_type2id = null;
        static void InitializeRegistry()
        {
            m_component_id2type = new Dictionary<int, System.Type>();
            m_component_type2id = new Dictionary<System.Type, int>();

            //m_component_registry[] = typeof();
            //【0001, 1000】Object Component
            AddRegistry(CT_TurnManagerComponent, typeof(TurnManagerComponent));

            //【1001, 2000】Player Component
            AddRegistry(CT_FactionComponent, typeof(FactionComponent));
            AddRegistry(CT_PlayerAIComponent, typeof(PlayerAIComponent));
            AddRegistry(CT_PlayerTargetingComponent, typeof(PlayerTargetingComponent));

            //【2001, 3000】Entity Component
            AddRegistry(CT_PositionComponent, typeof(PositionComponent));
            AddRegistry(CT_LocomotorComponent, typeof(LocomotorComponent));
            AddRegistry(CT_DamagableComponent, typeof(DamagableComponent));
            AddRegistry(CT_ManaComponent, typeof(ManaComponent));
            AddRegistry(CT_DeathComponent, typeof(DeathComponent));
            AddRegistry(CT_StateComponent, typeof(StateComponent));
            AddRegistry(CT_AttributeManagerComponent, typeof(AttributeManagerComponent));
            AddRegistry(CT_SkillManagerComponent, typeof(SkillManagerComponent));
            AddRegistry(CT_EffectManagerComponent, typeof(EffectManagerComponent));
            AddRegistry(CT_DamageModificationComponent, typeof(DamageModificationComponent));
            AddRegistry(CT_AIComponent, typeof(AIComponent));

            //【3001, 4000】Skill Component
            AddRegistry(CT_SkillDefinitionComponent, typeof(SkillDefinitionComponent));
            AddRegistry(CT_WeaponSkillComponent, typeof(WeaponSkillComponent));
            AddRegistry(CT_EffectGeneratorSkillComponent, typeof(EffectGeneratorSkillComponent));
            AddRegistry(CT_CreateObjectSkillComponent, typeof(CreateObjectSkillComponent));

            //【4001, 5000】Effect Component
            AddRegistry(CT_DamageEffectComponent, typeof(DamageEffectComponent));
            AddRegistry(CT_HealEffectComponent, typeof(HealEffectComponent));
            AddRegistry(CT_AddStateEffectComponent, typeof(AddStateEffectComponent));
            AddRegistry(CT_ApplyGeneratorEffectComponent, typeof(ApplyGeneratorEffectComponent));

            //【10001，】Render Component
#if COMBAT_CLIENT
            AddRegistry(CT_ModelComponent, typeof(ModelComponent));
#endif
        }

        static void AddRegistry(int id, System.Type type)
        {
            m_component_id2type[id] = type;
            m_component_type2id[type] = id;
        }
    }
}