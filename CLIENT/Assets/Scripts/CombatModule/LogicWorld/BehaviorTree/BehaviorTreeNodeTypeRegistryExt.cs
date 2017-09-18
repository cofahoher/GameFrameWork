using System;
using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BehaviorTreeNodeTypeRegistry
    {
        static public void RegisterDefaultNodes()
        {
            if (ms_default_btnodes_registered)
                return;
            ms_default_btnodes_registered = true;

            Register<BTAction_StopTreeUpdate>();
            Register<BTAction_WaitSomeTime>();
            Register<BTFor>();
            Register<BTIfElse>();
            Register<BTParallelSelector>();
            Register<BTParallelSequence>();
            Register<BTSelector>();
            Register<BTSequence>();
            Register<BTFalse>();
            Register<BTNot>();
            Register<BTPulse>();
            Register<BTTrue>();
            Register<BTReference>();
            Register<BTAction_Test>();
            Register<BTAction_Test2>();
            Register<BTSKillAction_ApplyDamageToTargets>();
            Register<BTSKillAction_ApplyEffectToTargets>();
            Register<BTSKillAction_CreateObject>();
            Register<BTSKillAction_GatherTargets>();
            Register<BTSKillAction_KillTargets>();
            Register<BTSKillAction_PlayAction>();
            Register<BTSKillAction_PlayRenderEffect>();
            Register<BTSKillAction_PlaySound>();
            Register<BTSKillAction_Spurt>();
        }
    }

    public partial class BTAction_StopTreeUpdate
    {
        public const int ID = 350980521;
    }

    public partial class BTAction_WaitSomeTime
    {
        public const int ID = 1190266554;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("time", out value))
                m_time = FixPoint.Parse(value);
        }
    }

    public partial class BTFor
    {
        public const int ID = 266622855;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("n", out value))
                m_n = int.Parse(value);
        }
    }

    public partial class BTIfElse
    {
        public const int ID = 1975170596;
    }

    public partial class BTParallelSelector
    {
        public const int ID = -1284902641;
    }

    public partial class BTParallelSequence
    {
        public const int ID = 755470241;
    }

    public partial class BTSelector
    {
        public const int ID = 747098536;
    }

    public partial class BTSequence
    {
        public const int ID = -1293308154;
    }

    public partial class BTFalse
    {
        public const int ID = -540892261;
    }

    public partial class BTNot
    {
        public const int ID = -1242729876;
    }

    public partial class BTPulse
    {
        public const int ID = 226717157;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("interval", out value))
                m_interval = FixPoint.Parse(value);
        }
    }

    public partial class BTTrue
    {
        public const int ID = -996190328;
    }

    public partial class BTReference
    {
        public const int ID = -1464327809;
    }

    public partial class BTAction_Test
    {
        public const int ID = -582665673;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("config_int", out value))
                m_int = int.Parse(value);
            if (variables.TryGetValue("config_fp", out value))
                m_fp = FixPoint.Parse(value);
            if (variables.TryGetValue("config_string", out value))
                m_string = value;
            if (variables.TryGetValue("config_bool", out value))
                m_bool = bool.Parse(value);
            if (variables.TryGetValue("config_crc", out value))
                m_crcint = (int)CRC.Calculate(value);
            if (variables.TryGetValue("config_formula", out value))
                m_formula.Compile(value);
        }
    }

    public partial class BTAction_Test2
    {
        public const int ID = -89988531;
    }

    public partial class BTSKillAction_ApplyDamageToTargets
    {
        public const int ID = -84623167;

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

    public partial class BTSKillAction_ApplyEffectToTargets
    {
        public const int ID = -706376222;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("generator_id", out value))
                m_generator_cfgid = int.Parse(value);
        }
    }

    public partial class BTSKillAction_CreateObject
    {
        public const int ID = 836422180;

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
        }
    }

    public partial class BTSKillAction_GatherTargets
    {
        public const int ID = -1607557959;

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
        }
    }

    public partial class BTSKillAction_KillTargets
    {
        public const int ID = -124361925;
    }

    public partial class BTSKillAction_PlayAction
    {
        public const int ID = 313169777;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("animation", out value))
                m_animation = value;
            if (variables.TryGetValue("next_animation", out value))
                m_next_animation = value;
            if (variables.TryGetValue("loop", out value))
                m_loop = bool.Parse(value);
        }
    }

    public partial class BTSKillAction_PlayRenderEffect
    {
        public const int ID = -115988519;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("render_effect_cfgid", out value))
                m_render_effect_cfgid = int.Parse(value);
        }
    }

    public partial class BTSKillAction_PlaySound
    {
        public const int ID = 1834559836;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("sound", out value))
                m_sound = int.Parse(value);
        }
    }

    public partial class BTSKillAction_Spurt
    {
        public const int ID = -1023934299;

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
}