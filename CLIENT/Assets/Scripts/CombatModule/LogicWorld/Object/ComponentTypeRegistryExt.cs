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
            Register<PlayerGameplaySpecialComponent>(false);
            Register<PlayerTargetingComponent>(false);
            Register<AIComponent>(false);
            Register<AttributeManagerComponent>(false);
            Register<DamagableComponent>(false);
            Register<DamageModificationComponent>(false);
            Register<DeathComponent>(false);
            Register<EffectManagerComponent>(false);
            Register<EffectRegionComponent>(false);
            Register<EntityDefinitionComponent>(false);
            Register<EntityGameplaySpecilaComponent>(false);
            Register<LocomotorComponent>(false);
            Register<ManaComponent>(false);
            Register<ObstacleComponent>(false);
            Register<PathFindingComponent>(false);
            Register<PositionComponent>(false);
            Register<ProjectileComponent>(false);
            Register<SimpleAIComponent>(false);
            Register<SkillManagerComponent>(false);
            Register<SpawnObjectComponent>(false);
            Register<StateComponent>(false);
            Register<SummonedEntityComponent>(false);
            Register<TargetingComponent>(false);
            Register<BehaviorTreeSkillComponent>(false);
            Register<CreateObjectSkillComponent>(false);
            Register<DirectDamageSkillComponent>(false);
            Register<EffectGeneratorSkillComponent>(false);
            Register<KillTargetSkillComponent>(false);
            Register<SkillDefinitionComponent>(false);
            Register<SpurtSkillComponent>(false);
            Register<AddStateEffectComponent>(false);
            Register<ApplyGeneratorEffectComponent>(false);
            Register<ChangePlayerFactionEffectComponent>(false);
            Register<CreateObjectEffectComponent>(false);
            Register<DamageEffectComponent>(false);
            Register<DamageOverTimeEffectComponent>(false);
            Register<EffectDefinitionComponent>(false);
            Register<HealEffectComponent>(false);
            Register<KillOwnerEffectComponent>(false);
            Register<KnockbackEffectComponent>(false);
            Register<ModifyAttributeEffectComponent>(false);

#if COMBAT_CLIENT
            Register<AnimationComponent>(true);
            Register<AnimatorComponent>(true);
            Register<AimingComponent>(true);
            Register<ModelComponent>(true);
            Register<PredictLogicComponent>(true);
            Register<RenderEffectManagerComponent>(true);
#endif
        }
    }

    public partial class LevelComponent
    {
        public const int ID = 1460936472;
        public const int VID_CurrentLevel = -1466831087;

        static LevelComponent()
        {
            ComponentTypeRegistry.RegisterVariable(VID_CurrentLevel, ID);
        }

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("be_killed_experience", out value))
                m_be_killed_experience = int.Parse(value);
            if (variables.TryGetValue("max_level", out value))
                m_max_level = int.Parse(value);
            if (variables.TryGetValue("level_table", out value))
                m_experience_level_table = value;
            if (variables.TryGetValue("level", out value))
                CurrentLevel = int.Parse(value);
        }

        public override bool GetVariable(int id, out FixPoint value)
        {
            switch (id)
            {
            case VID_CurrentLevel:
                value = (FixPoint)(CurrentLevel);
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
            case VID_CurrentLevel:
                CurrentLevel = (int)value;
                return true;
            default:
                return false;
            }
        }

#region GETTER/SETTER
        public int BeKilledExperience
        {
            get { return m_be_killed_experience; }
        }

        public int MaxLevel
        {
            get { return m_max_level; }
        }
#endregion
    }

    public partial class TurnManagerComponent
    {
        public const int ID = 522304681;
    }

    public partial class FactionComponent
    {
        public const int ID = -2067245938;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("faction", out value))
                m_faction = (int)CRC.Calculate(value);
        }
    }

    public partial class PlayerAIComponent
    {
        public const int ID = -1685636568;
    }

    public partial class PlayerGameplaySpecialComponent
    {
        public const int ID = -1911957170;
    }

    public partial class PlayerTargetingComponent
    {
        public const int ID = -1756719123;
    }

    public partial class AIComponent
    {
        public const int ID = 1842924899;
    }

    public partial class AttributeManagerComponent
    {
        public const int ID = -1022416408;
    }

    public partial class DamagableComponent
    {
        public const int ID = -178669635;
        public const int VID_MaxHealth = 1886202899;
        public const int VID_CurrentHealth = -39273671;

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
        public const int ID = 1600517526;
    }

    public partial class DeathComponent
    {
        public const int ID = -453346718;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("born_generator_id", out value))
                m_born_generator_cfgid = int.Parse(value);
            if (variables.TryGetValue("die_generator_id", out value))
                m_die_generator_cfgid = int.Parse(value);
            if (variables.TryGetValue("killer_generator_id", out value))
                m_killer_generator_cfgid = int.Parse(value);
            if (variables.TryGetValue("life_time", out value))
                m_life_time = FixPoint.Parse(value);
            if (variables.TryGetValue("hide_delay", out value))
                m_hide_delay = FixPoint.Parse(value);
            if (variables.TryGetValue("delete_delay", out value))
                m_delete_delay = FixPoint.Parse(value);
            if (variables.TryGetValue("can_resurrect", out value))
                m_can_resurrect = bool.Parse(value);
        }
    }

    public partial class EffectManagerComponent
    {
        public const int ID = 1995518324;
    }

    public partial class EffectRegionComponent
    {
        public const int ID = 1326997351;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("gathering_type", out value))
                m_target_gathering_param.m_type = (int)CRC.Calculate(value);
            if (variables.TryGetValue("gathering_param1", out value))
                m_target_gathering_param.m_param1 = FixPoint.Parse(value);
            if (variables.TryGetValue("gathering_param2", out value))
                m_target_gathering_param.m_param2 = FixPoint.Parse(value);
            if (variables.TryGetValue("gathering_faction", out value))
                m_target_gathering_param.m_faction = (int)CRC.Calculate(value);
            if (variables.TryGetValue("gathering_category", out value))
                m_target_gathering_param.m_category = (int)CRC.Calculate(value);
            if (variables.TryGetValue("enter_generator", out value))
                m_enter_generator_cfgid = int.Parse(value);
            if (variables.TryGetValue("period_generator", out value))
                m_period_generator_cfgid = int.Parse(value);
            if (variables.TryGetValue("period", out value))
                m_period = FixPoint.Parse(value);
        }
    }

    public partial class EntityDefinitionComponent
    {
        public const int ID = -1587766303;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("category1", out value))
                m_category_1 = (int)CRC.Calculate(value);
            if (variables.TryGetValue("category2", out value))
                m_category_2 = (int)CRC.Calculate(value);
            if (variables.TryGetValue("category3", out value))
                m_category_3 = (int)CRC.Calculate(value);
        }
    }

    public partial class EntityGameplaySpecilaComponent
    {
        public const int ID = -1899933679;
    }

    public partial class LocomotorComponent
    {
        public const int ID = 694646728;
        public const int VID_MaxSpeed = -1745541986;

        static LocomotorComponent()
        {
            ComponentTypeRegistry.RegisterVariable(VID_MaxSpeed, ID);
        }

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("max_speed", out value))
                MaxSpeed = FixPoint.Parse(value);
            if (variables.TryGetValue("avoid_obstacle", out value))
                m_avoid_obstacle = bool.Parse(value);
        }

        public override bool GetVariable(int id, out FixPoint value)
        {
            switch (id)
            {
            case VID_MaxSpeed:
                value = MaxSpeed;
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
            case VID_MaxSpeed:
                MaxSpeed = value;
                return true;
            default:
                return false;
            }
        }
    }

    public partial class ManaComponent
    {
        public const int ID = -1133849163;
    }

    public partial class ObstacleComponent
    {
        public const int ID = 1898152231;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("ext_x", out value))
                m_extents.x = FixPoint.Parse(value);
            if (variables.TryGetValue("ext_y", out value))
                m_extents.y = FixPoint.Parse(value);
            if (variables.TryGetValue("ext_z", out value))
                m_extents.z = FixPoint.Parse(value);
        }
    }

    public partial class PathFindingComponent
    {
        public const int ID = -975410129;
    }

    public partial class PositionComponent
    {
        public const int ID = 1095243466;
        public const int VID_X = -1505763071;
        public const int VID_Y = -1088106432;
        public const int VID_Z = -1811315837;
        public const int VID_BaseAngle = 1682267402;
        public const int VID_Radius = -1373094910;
        public const int VID_Height = 231863489;

        static PositionComponent()
        {
            ComponentTypeRegistry.RegisterVariable(VID_X, ID);
            ComponentTypeRegistry.RegisterVariable(VID_Y, ID);
            ComponentTypeRegistry.RegisterVariable(VID_Z, ID);
            ComponentTypeRegistry.RegisterVariable(VID_BaseAngle, ID);
            ComponentTypeRegistry.RegisterVariable(VID_Radius, ID);
            ComponentTypeRegistry.RegisterVariable(VID_Height, ID);
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
                m_base_angle = FixPoint.Parse(value);
            if (variables.TryGetValue("radius", out value))
                m_radius = FixPoint.Parse(value);
            if (variables.TryGetValue("height", out value))
                m_height = FixPoint.Parse(value);
            if (variables.TryGetValue("base_rotatable", out value))
                m_base_rotatable = bool.Parse(value);
            if (variables.TryGetValue("collision_sender", out value))
                m_collision_sender = bool.Parse(value);
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
            case VID_BaseAngle:
                value = m_base_angle;
                return true;
            case VID_Radius:
                value = m_radius;
                return true;
            case VID_Height:
                value = m_height;
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
            case VID_BaseAngle:
                m_base_angle = value;
                return true;
            case VID_Radius:
                m_radius = value;
                return true;
            case VID_Height:
                m_height = value;
                return true;
            default:
                return false;
            }
        }

#region GETTER/SETTER
        public FixPoint X
        {
            get { return m_current_position.x; }
        }

        public FixPoint Y
        {
            get { return m_current_position.y; }
        }

        public FixPoint Z
        {
            get { return m_current_position.z; }
        }

        public FixPoint Radius
        {
            get { return m_radius; }
        }

        public FixPoint Height
        {
            get { return m_height; }
        }

        public bool BaseRotatable
        {
            get { return m_base_rotatable; }
        }

        public bool Visible
        {
            get { return m_visible; }
        }
#endregion
    }

    public partial class ProjectileComponent
    {
        public const int ID = -1092026181;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("speed", out value))
                m_speed = FixPoint.Parse(value);
            if (variables.TryGetValue("lifetime", out value))
                m_lifetime = FixPoint.Parse(value);
            if (variables.TryGetValue("collision_sound", out value))
                m_collision_sound_cfgid = int.Parse(value);
            if (variables.TryGetValue("collision_faction", out value))
                m_collision_faction = (int)CRC.Calculate(value);
            if (variables.TryGetValue("can_cross_obstacle", out value))
                m_can_cross_obstacle = bool.Parse(value);
        }
    }

    public partial class SimpleAIComponent
    {
        public const int ID = 1341755975;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("guard_range", out value))
                m_guard_range = FixPoint.Parse(value);
        }
    }

    public partial class SkillManagerComponent
    {
        public const int ID = 2066148607;
    }

    public partial class SpawnObjectComponent
    {
        public const int ID = 1753005899;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("object_type_id", out value))
                m_object_type_id = int.Parse(value);
            if (variables.TryGetValue("object_proto_id", out value))
                m_object_proto_id = int.Parse(value);
            if (variables.TryGetValue("object_distance", out value))
                m_object_distance = FixPoint.Parse(value);
            if (variables.TryGetValue("init_count", out value))
                m_init_count = int.Parse(value);
            if (variables.TryGetValue("max_count", out value))
                m_max_count = int.Parse(value);
            if (variables.TryGetValue("update_interval", out value))
                m_update_interval = FixPoint.Parse(value);
        }
    }

    public partial class StateComponent
    {
        public const int ID = 11707299;
    }

    public partial class SummonedEntityComponent
    {
        public const int ID = 1051926082;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("die_with_master", out value))
                m_die_with_master = bool.Parse(value);
        }
    }

    public partial class TargetingComponent
    {
        public const int ID = -775984024;
    }

    public partial class BehaviorTreeSkillComponent
    {
        public const int ID = 777250943;
    }

    public partial class CreateObjectSkillComponent
    {
        public const int ID = 332789708;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("object_type_id", out value))
                m_object_type_id = int.Parse(value);
            if (variables.TryGetValue("object_proto_id", out value))
                m_object_proto_id = int.Parse(value);
            if (variables.TryGetValue("object_life_time", out value))
                m_object_life_time = FixPoint.Parse(value);
            if (variables.TryGetValue("generator_id", out value))
                m_generator_cfgid = int.Parse(value);
            if (variables.TryGetValue("offset_x", out value))
                m_offset.x = FixPoint.Parse(value);
            if (variables.TryGetValue("offset_y", out value))
                m_offset.y = FixPoint.Parse(value);
            if (variables.TryGetValue("offset_z", out value))
                m_offset.z = FixPoint.Parse(value);
            if (variables.TryGetValue("combo_type", out value))
                m_combo_type_crc = (int)CRC.Calculate(value);
            if (variables.TryGetValue("combo_attack_cnt", out value))
                m_combo_attack_cnt = int.Parse(value);
            if (variables.TryGetValue("combo_interval", out value))
                m_combo_interval = FixPoint.Parse(value);
        }
    }

    public partial class DirectDamageSkillComponent
    {
        public const int ID = -360499912;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("delay_time", out value))
                m_delay_time = FixPoint.Parse(value);
            if (variables.TryGetValue("damage_type", out value))
                m_damage_type_id = (int)CRC.Calculate(value);
            if (variables.TryGetValue("damage_amount", out value))
                m_damage_amount.Compile(value);
            if (variables.TryGetValue("damage_render_effect", out value))
                m_damage_render_effect_cfgid = int.Parse(value);
            if (variables.TryGetValue("damage_sound", out value))
                m_damage_sound_cfgid = int.Parse(value);
            if (variables.TryGetValue("combo_attack_cnt", out value))
                m_combo_attack_cnt = int.Parse(value);
            if (variables.TryGetValue("combo_interval", out value))
                m_combo_interval = FixPoint.Parse(value);
            if (variables.TryGetValue("render_effect", out value))
                m_render_effect_cfgid = int.Parse(value);
        }
    }

    public partial class EffectGeneratorSkillComponent
    {
        public const int ID = 1037752092;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("delay_time", out value))
                m_delay_time = FixPoint.Parse(value);
            if (variables.TryGetValue("generator_id", out value))
                m_generator_cfgid = int.Parse(value);
            if (variables.TryGetValue("render_effect", out value))
                m_render_effect_cfgid = int.Parse(value);
            if (variables.TryGetValue("render_delay_time", out value))
                m_render_delay_time = FixPoint.Parse(value);
        }
    }

    public partial class KillTargetSkillComponent
    {
        public const int ID = -1694896567;
    }

    public partial class SkillDefinitionComponent
    {
        public const int ID = -1434735094;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("mana_type", out value))
                m_mana_type = (int)CRC.Calculate(value);
            if (variables.TryGetValue("mana_cost", out value))
                m_mana_cost.Compile(value);
            if (variables.TryGetValue("min_range", out value))
                m_min_range.Compile(value);
            if (variables.TryGetValue("max_range", out value))
                m_max_range.Compile(value);
            if (variables.TryGetValue("cooldown_time", out value))
                m_cooldown_time.Compile(value);
            if (variables.TryGetValue("casting_time", out value))
                m_casting_time.Compile(value);
            if (variables.TryGetValue("inflict_time", out value))
                m_inflict_time.Compile(value);
            if (variables.TryGetValue("expiration_time", out value))
                m_expiration_time.Compile(value);
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
            if (variables.TryGetValue("gathering_type", out value))
                m_target_gathering_param.m_type = (int)CRC.Calculate(value);
            if (variables.TryGetValue("gathering_param1", out value))
                m_target_gathering_param.m_param1 = FixPoint.Parse(value);
            if (variables.TryGetValue("gathering_param2", out value))
                m_target_gathering_param.m_param2 = FixPoint.Parse(value);
            if (variables.TryGetValue("gathering_faction", out value))
                m_target_gathering_param.m_faction = (int)CRC.Calculate(value);
            if (variables.TryGetValue("gathering_category", out value))
                m_target_gathering_param.m_category = (int)CRC.Calculate(value);
            if (variables.TryGetValue("need_gather_targets", out value))
                m_need_gather_targets = bool.Parse(value);
            if (variables.TryGetValue("targets_min_count_for_activate", out value))
                m_targets_min_count_for_activate = int.Parse(value);
            if (variables.TryGetValue("external_data_type", out value))
                m_external_data_type = (int)CRC.Calculate(value);
            if (variables.TryGetValue("auto_face", out value))
                m_auto_face_type = (int)CRC.Calculate(value);
            if (variables.TryGetValue("inflict_type", out value))
                m_inflict_type = int.Parse(value);
            if (variables.TryGetValue("inflict_missile", out value))
                m_inflict_missile = value;
            if (variables.TryGetValue("inflict_missile_speed", out value))
                m_inflict_missile_speed = FixPoint.Parse(value);
            if (variables.TryGetValue("impact_delay", out value))
                m_impact_delay = FixPoint.Parse(value);
            if (variables.TryGetValue("icon", out value))
                m_icon = value;
            if (variables.TryGetValue("auto_aim", out value))
                m_auto_aim_type = (int)CRC.Calculate(value);
            if (variables.TryGetValue("casting_animation", out value))
                m_casting_animation = value;
            if (variables.TryGetValue("main_animation", out value))
                m_main_animation = value;
            if (variables.TryGetValue("expiration_animation", out value))
                m_expiration_animation = value;
            if (variables.TryGetValue("main_render_effect", out value))
                m_main_render_effect_cfgid = int.Parse(value);
            if (variables.TryGetValue("main_sound", out value))
                m_main_sound = int.Parse(value);
        }

#region GETTER/SETTER
        public int ManaType
        {
            get { return m_mana_type; }
        }

        public FixPoint ManaCost
        {
            get { return m_mana_cost.Evaluate(this); }
        }

        public FixPoint MinRange
        {
            get { return m_min_range.Evaluate(this); }
        }

        public FixPoint MaxRange
        {
            get { return m_max_range.Evaluate(this); }
        }

        public FixPoint CooldownTime
        {
            get { return m_cooldown_time.Evaluate(this); }
        }

        public FixPoint CastingTime
        {
            get { return m_casting_time.Evaluate(this); }
        }

        public FixPoint InflictTime
        {
            get { return m_inflict_time.Evaluate(this); }
        }

        public FixPoint ExpirationTime
        {
            get { return m_expiration_time.Evaluate(this); }
        }

        public bool StartsActive
        {
            get { return m_starts_active; }
        }

        public bool BlocksOtherSkillsWhenActive
        {
            get { return m_blocks_other_skills_when_active; }
        }

        public bool BlocksMovementWhenActive
        {
            get { return m_blocks_movement_when_active; }
        }

        public bool DeactivateWhenMoving
        {
            get { return m_deactivate_when_moving; }
        }

        public bool CanActivateWhileMoving
        {
            get { return m_can_activate_while_moving; }
        }

        public bool CanActivateWhenDisabled
        {
            get { return m_can_activate_when_disabled; }
        }

        public bool NeedGatherTargets
        {
            get { return m_need_gather_targets; }
        }

        public int TargetsMinCountForActivate
        {
            get { return m_targets_min_count_for_activate; }
        }

        public int ExternalDataType
        {
            get { return m_external_data_type; }
        }

        public int AutoFaceType
        {
            get { return m_auto_face_type; }
        }

        public int InflictType
        {
            get { return m_inflict_type; }
        }

        public FixPoint InflictMissileSpeed
        {
            get { return m_inflict_missile_speed; }
        }

        public FixPoint ImpactDelay
        {
            get { return m_impact_delay; }
        }
#endregion
    }

    public partial class SpurtSkillComponent
    {
        public const int ID = -781874088;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("distance", out value))
                m_distance = FixPoint.Parse(value);
            if (variables.TryGetValue("time", out value))
                m_time = FixPoint.Parse(value);
            if (variables.TryGetValue("collision_target_generator_id", out value))
                m_collision_target_generator_cfgid = int.Parse(value);
            if (variables.TryGetValue("backward", out value))
                m_backward = bool.Parse(value);
        }
    }

    public partial class AddStateEffectComponent
    {
        public const int ID = 347312498;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("state", out value))
                m_state = (int)CRC.Calculate(value);
        }
    }

    public partial class ApplyGeneratorEffectComponent
    {
        public const int ID = -943248477;
    }

    public partial class ChangePlayerFactionEffectComponent
    {
        public const int ID = -2128128422;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("faction", out value))
                m_faction = (int)CRC.Calculate(value);
            if (variables.TryGetValue("revert_when_unapply", out value))
                m_revert_when_unapply = bool.Parse(value);
        }
    }

    public partial class CreateObjectEffectComponent
    {
        public const int ID = 709457371;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("object_type_id", out value))
                m_object_type_id = int.Parse(value);
            if (variables.TryGetValue("object_proto_id", out value))
                m_object_proto_id = int.Parse(value);
            if (variables.TryGetValue("object_life_time", out value))
                m_object_life_time = FixPoint.Parse(value);
            if (variables.TryGetValue("offset_x", out value))
                m_offset.x = FixPoint.Parse(value);
            if (variables.TryGetValue("offset_y", out value))
                m_offset.y = FixPoint.Parse(value);
            if (variables.TryGetValue("offset_z", out value))
                m_offset.z = FixPoint.Parse(value);
            if (variables.TryGetValue("object_count", out value))
                m_object_count = int.Parse(value);
            if (variables.TryGetValue("interval", out value))
                m_interval = FixPoint.Parse(value);
            if (variables.TryGetValue("revert_when_unapply", out value))
                m_revert_when_unapply = bool.Parse(value);
        }
    }

    public partial class DamageEffectComponent
    {
        public const int ID = 1635290451;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("damage_type", out value))
                m_damage_type_id = (int)CRC.Calculate(value);
            if (variables.TryGetValue("damage_amount", out value))
                m_damage_amount.Compile(value);
            if (variables.TryGetValue("damage_render_effect", out value))
                m_damage_render_effect_cfgid = int.Parse(value);
            if (variables.TryGetValue("damage_sound", out value))
                m_damage_sound_cfgid = int.Parse(value);
        }
    }

    public partial class DamageOverTimeEffectComponent
    {
        public const int ID = 1300961956;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("damage_type", out value))
                m_damage_type_id = (int)CRC.Calculate(value);
            if (variables.TryGetValue("damage_amount", out value))
                m_damage_amount.Compile(value);
            if (variables.TryGetValue("damage_render_effect", out value))
                m_damage_render_effect_cfgid = int.Parse(value);
            if (variables.TryGetValue("damage_sound", out value))
                m_damage_sound_cfgid = int.Parse(value);
            if (variables.TryGetValue("period", out value))
                m_period = FixPoint.Parse(value);
        }
    }

    public partial class EffectDefinitionComponent
    {
        public const int ID = 473097098;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("category", out value))
                m_category = (int)CRC.Calculate(value);
            if (variables.TryGetValue("conflict_id", out value))
                m_conflict_id = (int)CRC.Calculate(value);
            if (variables.TryGetValue("duration", out value))
                m_duration.Compile(value);
        }

#region GETTER/SETTER
        public int Category
        {
            get { return m_category; }
        }

        public int ConflictID
        {
            get { return m_conflict_id; }
        }

        public FixPoint Duration
        {
            get { return m_duration.Evaluate(this); }
        }
#endregion
    }

    public partial class HealEffectComponent
    {
        public const int ID = -679969353;
    }

    public partial class KillOwnerEffectComponent
    {
        public const int ID = 1345354818;
    }

    public partial class KnockbackEffectComponent
    {
        public const int ID = 732875937;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("distance", out value))
                m_distance = FixPoint.Parse(value);
            if (variables.TryGetValue("time", out value))
                m_time = FixPoint.Parse(value);
        }
    }

    public partial class ModifyAttributeEffectComponent
    {
        public const int ID = -107304768;
    }

    public partial class AnimationComponent
    {
        public const int ID = 60050710;

#if COMBAT_CLIENT

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("animation_path", out value))
                m_animation_path = value;
            if (variables.TryGetValue("locomotor_animation_name", out value))
                m_locomotor_animation_name = value;
        }
#endif
    }

    public partial class AnimatorComponent
    {
        public const int ID = -1773924714;

#if COMBAT_CLIENT

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("animator_path", out value))
                m_animator_path = value;
            if (variables.TryGetValue("locomotor_animation_name", out value))
                m_locomotor_animation_name = value;
        }
#endif
    }

    public partial class AimingComponent
    {
        public const int ID = -1191010116;

#if COMBAT_CLIENT

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("root_path", out value))
                m_root_path = value;
            if (variables.TryGetValue("aiming_line_parent_path", out value))
                m_aiming_line_parent_path = value;
            if (variables.TryGetValue("aiming_line_asset", out value))
                m_aiming_line_asset = value;
            if (variables.TryGetValue("rotate_turret_asset", out value))
                m_rotate_turret_asset = value;
        }
#endif
    }

    public partial class ModelComponent
    {
        public const int ID = -1332594716;

#if COMBAT_CLIENT

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("asset", out value))
                m_asset_name = value;
            if (variables.TryGetValue("bodyctrl_path", out value))
                m_bodyctrl_path = value;
            if (variables.TryGetValue("headctrl_path", out value))
                m_headctrl_path = value;
        }
#endif
    }

    public partial class PredictLogicComponent
    {
        public const int ID = 1521612208;
    }

    public partial class RenderEffectManagerComponent
    {
        public const int ID = -1117048785;
    }
}