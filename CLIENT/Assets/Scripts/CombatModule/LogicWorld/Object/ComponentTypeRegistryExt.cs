using System;
using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class ComponentTypeRegistry
    {
        static public void RegisterDefaultComponents()
        {
            if (ms_default_components_registered)
                return;
            ms_default_components_registered = true;

            Register<LevelComponent>(false);
            Register<TurnManagerComponent>(false);
            Register<FactionComponent>(false);
            Register<PlayerAIComponent>(false);
            Register<PlayerTargetingComponent>(false);
            Register<AIComponent>(false);
            Register<AttributeManagerComponent>(false);
            Register<DamagableComponent>(false);
            Register<DamageModificationComponent>(false);
            Register<DeathComponent>(false);
            Register<EffectManagerComponent>(false);
            Register<LocomotorComponent>(false);
            Register<ManaComponent>(false);
            Register<PositionComponent>(false);
            Register<SkillManagerComponent>(false);
            Register<StateComponent>(false);
            Register<CreateObjectSkillComponent>(false);
            Register<EffectGeneratorSkillComponent>(false);
            Register<SkillDefinitionComponent>(false);
            Register<WeaponSkillComponent>(false);
            Register<AddStateEffectComponent>(false);
            Register<ApplyGeneratorEffectComponent>(false);
            Register<DamageEffectComponent>(false);
            Register<HealEffectComponent>(false);

#if COMBAT_CLIENT
            Register<AnimationComponent>(true);
            Register<AnimatorComponent>(true);
            Register<ModelComponent>(true);
#endif

        }
    }

    public partial class LevelComponent
    {
        public const int ID = -1526547556;
    }

    public partial class TurnManagerComponent
    {
        public const int ID = -971101728;
    }

    public partial class FactionComponent
    {
        public const int ID = 1956626782;
    }

    public partial class PlayerAIComponent
    {
        public const int ID = 564472396;
    }

    public partial class PlayerTargetingComponent
    {
        public const int ID = 54498699;
    }

    public partial class AIComponent
    {
        public const int ID = 870870259;
    }

    public partial class AttributeManagerComponent
    {
        public const int ID = 794590895;
    }

    public partial class DamagableComponent
    {
        public const int ID = 1470729607;
    }

    public partial class DamageModificationComponent
    {
        public const int ID = 588960288;
    }

    public partial class DeathComponent
    {
        public const int ID = -2008540504;
    }

    public partial class EffectManagerComponent
    {
        public const int ID = -1307860976;
    }

    public partial class LocomotorComponent
    {
        public const int ID = 1531533156;
        public const int VID_MaxSpeed = -1136656387;

        static LocomotorComponent()
        {
            ComponentTypeRegistry.RegisterVariable(VID_MaxSpeed, ID);
        }

        public override bool GetVariable(int id, out FixPoint value)
        {
            switch (id)
            {
            case VID_MaxSpeed:
                value = m_current_max_speed;
                return true;
            default:
                value = FixPoint.Zero;
                return false;
            }
        }

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("max_speed", out value))
                m_current_max_speed = FixPoint.Parse(value);
        }
    }

    public partial class ManaComponent
    {
        public const int ID = 1470329370;
    }

    public partial class PositionComponent
    {
        public const int ID = -280295719;
        public const int VID_X = -1931733373;
        public const int VID_Y = -69523947;
        public const int VID_Z = 1657960367;
        public const int VID_Angle = 8471817;
        public const int VID_ExtX = 159317834;
        public const int VID_ExtY = 2121912284;
        public const int VID_ExtZ = -412049818;
        public const int VID_Visible = 2058414169;

        static PositionComponent()
        {
            ComponentTypeRegistry.RegisterVariable(VID_X, ID);
            ComponentTypeRegistry.RegisterVariable(VID_Y, ID);
            ComponentTypeRegistry.RegisterVariable(VID_Z, ID);
            ComponentTypeRegistry.RegisterVariable(VID_Angle, ID);
            ComponentTypeRegistry.RegisterVariable(VID_ExtX, ID);
            ComponentTypeRegistry.RegisterVariable(VID_ExtY, ID);
            ComponentTypeRegistry.RegisterVariable(VID_ExtZ, ID);
            ComponentTypeRegistry.RegisterVariable(VID_Visible, ID);
        }

        public override bool GetVariable(int id, out FixPoint value)
        {
            switch (id)
            {
            case VID_X:
                value = m_current_position.x;
                return true;
            case VID_Y:
                value = m_current_position.y;
                return true;
            case VID_Z:
                value = m_current_position.z;
                return true;
            case VID_Angle:
                value = m_current_angle;
                return true;
            case VID_ExtX:
                value = m_extents.x;
                return true;
            case VID_ExtY:
                value = m_extents.y;
                return true;
            case VID_ExtZ:
                value = m_extents.z;
                return true;
            case VID_Visible:
                value = (FixPoint)(m_visible);
                return true;
            default:
                value = FixPoint.Zero;
                return false;
            }
        }

        public override bool SetVariable(int id, FixPoint value)
        {
            switch (id)
            {
            case VID_X:
                m_current_position.x = value;
                return true;
            case VID_Y:
                m_current_position.y = value;
                return true;
            case VID_Z:
                m_current_position.z = value;
                return true;
            case VID_Angle:
                m_current_angle = value;
                return true;
            case VID_Visible:
                m_visible = (bool)value;
                return true;
            default:
                return false;
            }
        }

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("x", out value))
                m_current_position.x = FixPoint.Parse(value);
            if (variables.TryGetValue("y", out value))
                m_current_position.y = FixPoint.Parse(value);
            if (variables.TryGetValue("z", out value))
                m_current_position.z = FixPoint.Parse(value);
            if (variables.TryGetValue("angle", out value))
                m_current_angle = FixPoint.Parse(value);
            if (variables.TryGetValue("ext_x", out value))
                m_extents.x = FixPoint.Parse(value);
            if (variables.TryGetValue("ext_y", out value))
                m_extents.y = FixPoint.Parse(value);
            if (variables.TryGetValue("ext_z", out value))
                m_extents.z = FixPoint.Parse(value);
            if (variables.TryGetValue("visible", out value))
                m_visible = bool.Parse(value);
        }
    }

    public partial class SkillManagerComponent
    {
        public const int ID = -632761300;
    }

    public partial class StateComponent
    {
        public const int ID = 391674091;
    }

    public partial class CreateObjectSkillComponent
    {
        public const int ID = -1398796007;
    }

    public partial class EffectGeneratorSkillComponent
    {
        public const int ID = -1137093856;
    }

    public partial class SkillDefinitionComponent
    {
        public const int ID = 1669330388;
    }

    public partial class WeaponSkillComponent
    {
        public const int ID = -1579537349;
    }

    public partial class AddStateEffectComponent
    {
        public const int ID = -1130149388;
    }

    public partial class ApplyGeneratorEffectComponent
    {
        public const int ID = 48043748;
    }

    public partial class DamageEffectComponent
    {
        public const int ID = 557364623;
    }

    public partial class HealEffectComponent
    {
        public const int ID = 2141629956;
    }

#if COMBAT_CLIENT
    public partial class AnimationComponent
    {
        public const int ID = -1519428271;
    }

    public partial class AnimatorComponent
    {
        public const int ID = -317181815;
    }

    public partial class ModelComponent
    {
        public const int ID = -495890513;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("asset", out value))
                m_asset_name = value;
        }
    }
#endif
}