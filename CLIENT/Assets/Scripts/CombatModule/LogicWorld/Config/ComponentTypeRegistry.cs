using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class ComponentTypeRegistry
    {
        static bool ms_default_components_registered = false;
        static Dictionary<int, System.Type> m_component_id2type = new Dictionary<int, System.Type>();
        static Dictionary<System.Type, int> m_component_type2id = new Dictionary<System.Type, int>();
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
            System.Type type = null;
            m_component_id2type.TryGetValue(component_type_id, out type);
            return type;
        }

        public static int ComponentType2ID(System.Type type)
        {
            int component_type_id = -1; ;
            m_component_type2id.TryGetValue(type, out component_type_id);
            return component_type_id;
        }

        public static Component CreateComponent(int component_type_id)
        {
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

        static public void Register(int id, System.Type type)
        {
            m_component_id2type[id] = type;
            m_component_type2id[type] = id;
        }

        static public void RegisterDefaultComponents()
        {
            if (ms_default_components_registered)
                return;
            ms_default_components_registered = true;
            //【0001, 1000】Object Component
            Register(CT_TurnManagerComponent, typeof(TurnManagerComponent));

            //【1001, 2000】Player Component
            Register(CT_FactionComponent, typeof(FactionComponent));
            Register(CT_PlayerAIComponent, typeof(PlayerAIComponent));
            Register(CT_PlayerTargetingComponent, typeof(PlayerTargetingComponent));

            //【2001, 3000】Entity Component
            Register(CT_PositionComponent, typeof(PositionComponent));
            Register(CT_LocomotorComponent, typeof(LocomotorComponent));
            Register(CT_DamagableComponent, typeof(DamagableComponent));
            Register(CT_ManaComponent, typeof(ManaComponent));
            Register(CT_DeathComponent, typeof(DeathComponent));
            Register(CT_StateComponent, typeof(StateComponent));
            Register(CT_AttributeManagerComponent, typeof(AttributeManagerComponent));
            Register(CT_SkillManagerComponent, typeof(SkillManagerComponent));
            Register(CT_EffectManagerComponent, typeof(EffectManagerComponent));
            Register(CT_DamageModificationComponent, typeof(DamageModificationComponent));
            Register(CT_AIComponent, typeof(AIComponent));

            //【3001, 4000】Skill Component
            Register(CT_SkillDefinitionComponent, typeof(SkillDefinitionComponent));
            Register(CT_WeaponSkillComponent, typeof(WeaponSkillComponent));
            Register(CT_EffectGeneratorSkillComponent, typeof(EffectGeneratorSkillComponent));
            Register(CT_CreateObjectSkillComponent, typeof(CreateObjectSkillComponent));

            //【4001, 5000】Effect Component
            Register(CT_DamageEffectComponent, typeof(DamageEffectComponent));
            Register(CT_HealEffectComponent, typeof(HealEffectComponent));
            Register(CT_AddStateEffectComponent, typeof(AddStateEffectComponent));
            Register(CT_ApplyGeneratorEffectComponent, typeof(ApplyGeneratorEffectComponent));

            //【10001，】Render Component
//#if COMBAT_CLIENT
//            Register(CT_ModelComponent, typeof(ModelComponent));
//#endif
        }
    }
}