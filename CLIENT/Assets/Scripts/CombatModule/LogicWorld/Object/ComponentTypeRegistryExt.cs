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
        public const int VID_CurrentLevel = -1695888365;

        static LevelComponent()
        {
            ComponentTypeRegistry.RegisterVariable(VID_CurrentLevel, ID);
        }

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("level", out value))
                m_current_level = int.Parse(value);
        }

        public override bool GetVariable(int id, out FixPoint value)
        {
            switch (id)
            {
            case VID_CurrentLevel:
                value = (FixPoint)(m_current_level);
                return true;
            default:
                value = FixPoint.Zero;
                return false;
            }
        }

#region GETTER/SETTER
        public int CurrentLevel
        {
            get { return m_current_level; }
        }
#endregion
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
        public const int VID_MaxHealth = 1505485722;
        public const int VID_CurrentHealth = -921437827;

        static DamagableComponent()
        {
            ComponentTypeRegistry.RegisterVariable(VID_MaxHealth, ID);
            ComponentTypeRegistry.RegisterVariable(VID_CurrentHealth, ID);
        }

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("max_health", out value))
                m_current_max_health = FixPoint.Parse(value);
            if (variables.TryGetValue("current_health", out value))
                CurrentHealth = FixPoint.Parse(value);
        }

        public override bool GetVariable(int id, out FixPoint value)
        {
            switch (id)
            {
            case VID_MaxHealth:
                value = m_current_max_health;
                return true;
            case VID_CurrentHealth:
                value = CurrentHealth;
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
            case VID_MaxHealth:
                m_current_max_health = value;
                return true;
            case VID_CurrentHealth:
                CurrentHealth = value;
                return true;
            default:
                return false;
            }
        }

#region GETTER/SETTER
        public FixPoint MaxHealth
        {
            get { return m_current_max_health; }
        }
#endregion
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

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("max_speed", out value))
                m_current_max_speed = FixPoint.Parse(value);
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

#region GETTER/SETTER
        public FixPoint MaxSpeed
        {
            get { return m_current_max_speed; }
        }
#endregion
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
        public const int VID_CurrentAngle = 8471817;
        public const int VID_ExtX = 159317834;
        public const int VID_ExtY = 2121912284;
        public const int VID_ExtZ = -412049818;
        public const int VID_Visible = 2058414169;

        static PositionComponent()
        {
            ComponentTypeRegistry.RegisterVariable(VID_X, ID);
            ComponentTypeRegistry.RegisterVariable(VID_Y, ID);
            ComponentTypeRegistry.RegisterVariable(VID_Z, ID);
            ComponentTypeRegistry.RegisterVariable(VID_CurrentAngle, ID);
            ComponentTypeRegistry.RegisterVariable(VID_ExtX, ID);
            ComponentTypeRegistry.RegisterVariable(VID_ExtY, ID);
            ComponentTypeRegistry.RegisterVariable(VID_ExtZ, ID);
            ComponentTypeRegistry.RegisterVariable(VID_Visible, ID);
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
            case VID_CurrentAngle:
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
            case VID_CurrentAngle:
                m_current_angle = value;
                return true;
            case VID_Visible:
                m_visible = (bool)value;
                return true;
            default:
                return false;
            }
        }

#region GETTER/SETTER
        public FixPoint X
        {
            get { return m_current_position.x; }
            set { m_current_position.x = value; }
        }

        public FixPoint Y
        {
            get { return m_current_position.y; }
            set { m_current_position.y = value; }
        }

        public FixPoint Z
        {
            get { return m_current_position.z; }
            set { m_current_position.z = value; }
        }

        public FixPoint CurrentAngle
        {
            get { return m_current_angle; }
            set { m_current_angle = value; }
        }

        public FixPoint ExtX
        {
            get { return m_extents.x; }
        }

        public FixPoint ExtY
        {
            get { return m_extents.y; }
        }

        public FixPoint ExtZ
        {
            get { return m_extents.z; }
        }

        public bool Visible
        {
            get { return m_visible; }
            set { m_visible = value; }
        }
#endregion
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
        public const int VID_ManaCost = -177021205;
        public const int VID_CooldownTime = -766682502;
        public const int VID_CastingTime = -1011172798;
        public const int VID_ExpirationTime = -2084769206;
        public const int VID_StartsActive = -332320693;
        public const int VID_BlocksOtherSkillsWhenActive = -1537750290;
        public const int VID_BlocksMovementWhenActive = -27660750;
        public const int VID_DeactivateWhenMoving = 1315455694;
        public const int VID_CanActivateWhileMoving = 961060384;
        public const int VID_CanActivateWhenDisabled = 2062940555;
        public const int VID_ExpectedTargetCount = 626351239;
        public const int VID_AIExpectedTargetCount = -1116078248;
        public const int VID_IsSkill = 314640135;
        public const int VID_Priority = 1655102503;
        public const int VID_PsDelay = -1651357205;

        static SkillDefinitionComponent()
        {
            ComponentTypeRegistry.RegisterVariable(VID_ManaCost, ID);
            ComponentTypeRegistry.RegisterVariable(VID_CooldownTime, ID);
            ComponentTypeRegistry.RegisterVariable(VID_CastingTime, ID);
            ComponentTypeRegistry.RegisterVariable(VID_ExpirationTime, ID);
            ComponentTypeRegistry.RegisterVariable(VID_StartsActive, ID);
            ComponentTypeRegistry.RegisterVariable(VID_BlocksOtherSkillsWhenActive, ID);
            ComponentTypeRegistry.RegisterVariable(VID_BlocksMovementWhenActive, ID);
            ComponentTypeRegistry.RegisterVariable(VID_DeactivateWhenMoving, ID);
            ComponentTypeRegistry.RegisterVariable(VID_CanActivateWhileMoving, ID);
            ComponentTypeRegistry.RegisterVariable(VID_CanActivateWhenDisabled, ID);
            ComponentTypeRegistry.RegisterVariable(VID_ExpectedTargetCount, ID);
            ComponentTypeRegistry.RegisterVariable(VID_AIExpectedTargetCount, ID);
            ComponentTypeRegistry.RegisterVariable(VID_IsSkill, ID);
            ComponentTypeRegistry.RegisterVariable(VID_Priority, ID);
            ComponentTypeRegistry.RegisterVariable(VID_PsDelay, ID);
        }

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("mana_cost_formula", out value))
                m_mana_cost_formula.Compile(value);
            if (variables.TryGetValue("cooldown_time_formula", out value))
                m_cooldown_time_formula.Compile(value);
            if (variables.TryGetValue("casting_time_formula", out value))
                m_casting_time_formula.Compile(value);
            if (variables.TryGetValue("expiration_time_formula", out value))
                m_expiration_time_formula.Compile(value);
            if (variables.TryGetValue("starts_active", out value))
                m_starts_active = bool.Parse(value);
            if (variables.TryGetValue("blocks_other_skills_when_active", out value))
                m_blocks_other_skills_when_active = bool.Parse(value);
            if (variables.TryGetValue("blocks_movement_when_active", out value))
                m_blocks_movement_when_active = bool.Parse(value);
            if (variables.TryGetValue("deactivate_when_moving", out value))
                m_deactivate_when_moving = bool.Parse(value);
            if (variables.TryGetValue("can_activate_while_moving", out value))
                m_can_activate_while_moving = bool.Parse(value);
            if (variables.TryGetValue("can_activate_when_disabled", out value))
                m_can_activate_when_disabled = bool.Parse(value);
            if (variables.TryGetValue("expected_target_count", out value))
                m_expected_target_count = int.Parse(value);
            if (variables.TryGetValue("ai_expected_target_count", out value))
                m_ai_expected_target_count = int.Parse(value);
            if (variables.TryGetValue("is_skill", out value))
                m_is_skill = bool.Parse(value);
            if (variables.TryGetValue("priority", out value))
                m_priority = int.Parse(value);
            if (variables.TryGetValue("skill_desc", out value))
                m_skill_desc = value;
            if (variables.TryGetValue("animation_res", out value))
                m_animation_res = value;
            if (variables.TryGetValue("ps_res", out value))
                m_ps_res = value;
            if (variables.TryGetValue("ps_delay", out value))
                m_ps_delay = FixPoint.Parse(value);
        }

        public override bool GetVariable(int id, out FixPoint value)
        {
            switch (id)
            {
            case VID_ManaCost:
                value = m_mana_cost_formula.Evaluate(this);
                return true;
            case VID_CooldownTime:
                value = m_cooldown_time_formula.Evaluate(this);
                return true;
            case VID_CastingTime:
                value = m_casting_time_formula.Evaluate(this);
                return true;
            case VID_ExpirationTime:
                value = m_expiration_time_formula.Evaluate(this);
                return true;
            case VID_StartsActive:
                value = (FixPoint)(m_starts_active);
                return true;
            case VID_BlocksOtherSkillsWhenActive:
                value = (FixPoint)(m_blocks_other_skills_when_active);
                return true;
            case VID_BlocksMovementWhenActive:
                value = (FixPoint)(m_blocks_movement_when_active);
                return true;
            case VID_DeactivateWhenMoving:
                value = (FixPoint)(m_deactivate_when_moving);
                return true;
            case VID_CanActivateWhileMoving:
                value = (FixPoint)(m_can_activate_while_moving);
                return true;
            case VID_CanActivateWhenDisabled:
                value = (FixPoint)(m_can_activate_when_disabled);
                return true;
            case VID_ExpectedTargetCount:
                value = (FixPoint)(m_expected_target_count);
                return true;
            case VID_AIExpectedTargetCount:
                value = (FixPoint)(m_ai_expected_target_count);
                return true;
            case VID_IsSkill:
                value = (FixPoint)(m_is_skill);
                return true;
            case VID_Priority:
                value = (FixPoint)(m_priority);
                return true;
            case VID_PsDelay:
                value = m_ps_delay;
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
            case VID_StartsActive:
                m_starts_active = (bool)value;
                return true;
            case VID_BlocksOtherSkillsWhenActive:
                m_blocks_other_skills_when_active = (bool)value;
                return true;
            case VID_BlocksMovementWhenActive:
                m_blocks_movement_when_active = (bool)value;
                return true;
            case VID_DeactivateWhenMoving:
                m_deactivate_when_moving = (bool)value;
                return true;
            case VID_CanActivateWhileMoving:
                m_can_activate_while_moving = (bool)value;
                return true;
            case VID_CanActivateWhenDisabled:
                m_can_activate_when_disabled = (bool)value;
                return true;
            case VID_ExpectedTargetCount:
                m_expected_target_count = (int)value;
                return true;
            case VID_AIExpectedTargetCount:
                m_ai_expected_target_count = (int)value;
                return true;
            case VID_IsSkill:
                m_is_skill = (bool)value;
                return true;
            case VID_Priority:
                m_priority = (int)value;
                return true;
            case VID_PsDelay:
                m_ps_delay = value;
                return true;
            default:
                return false;
            }
        }

#region GETTER/SETTER
        public FixPoint ManaCost
        {
            get { return m_mana_cost_formula.Evaluate(this); }
        }

        public FixPoint CooldownTime
        {
            get { return m_cooldown_time_formula.Evaluate(this); }
        }

        public FixPoint CastingTime
        {
            get { return m_casting_time_formula.Evaluate(this); }
        }

        public FixPoint ExpirationTime
        {
            get { return m_expiration_time_formula.Evaluate(this); }
        }

        public bool StartsActive
        {
            get { return m_starts_active; }
            set { m_starts_active = value; }
        }

        public bool BlocksOtherSkillsWhenActive
        {
            get { return m_blocks_other_skills_when_active; }
            set { m_blocks_other_skills_when_active = value; }
        }

        public bool BlocksMovementWhenActive
        {
            get { return m_blocks_movement_when_active; }
            set { m_blocks_movement_when_active = value; }
        }

        public bool DeactivateWhenMoving
        {
            get { return m_deactivate_when_moving; }
            set { m_deactivate_when_moving = value; }
        }

        public bool CanActivateWhileMoving
        {
            get { return m_can_activate_while_moving; }
            set { m_can_activate_while_moving = value; }
        }

        public bool CanActivateWhenDisabled
        {
            get { return m_can_activate_when_disabled; }
            set { m_can_activate_when_disabled = value; }
        }

        public int ExpectedTargetCount
        {
            get { return m_expected_target_count; }
            set { m_expected_target_count = value; }
        }

        public int AIExpectedTargetCount
        {
            get { return m_ai_expected_target_count; }
            set { m_ai_expected_target_count = value; }
        }

        public bool IsSkill
        {
            get { return m_is_skill; }
            set { m_is_skill = value; }
        }

        public int Priority
        {
            get { return m_priority; }
            set { m_priority = value; }
        }

        public FixPoint PsDelay
        {
            get { return m_ps_delay; }
            set { m_ps_delay = value; }
        }
#endregion
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