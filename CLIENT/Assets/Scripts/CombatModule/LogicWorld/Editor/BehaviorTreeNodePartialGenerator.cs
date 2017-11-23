using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
namespace Combat
{
    public class BehaviorTreeNodePartialGenerator
    {
        //控制整个节点的标志位

        //控制单个变量的标志位
        const int TRANSFORM2CRCID = 1 << 15;

        static void InitAllBTNode()
        {
            #region 独立的
            //Actions
            REGISTER_BTNODE<BTAction_StopTreeUpdate>();
            REGISTER_BTNODE<BTAction_WaitSomeTime>()
                .REGISTER_VARIABLE<FixPoint>("time", "m_time");
            //Composites
            REGISTER_BTNODE<BTFor>()
                .REGISTER_VARIABLE<int>("n", "m_n")
                .REGISTER_VARIABLE_CRC<int>("variable_id", "m_variable_id");
            REGISTER_BTNODE<BTIfElse>();
            REGISTER_BTNODE<BTParallelSelector>();
            REGISTER_BTNODE<BTParallelSequence>();
            REGISTER_BTNODE<BTSelector>();
            REGISTER_BTNODE<BTSequence>();
            //Conditions
            REGISTER_BTNODE<BTConditionExpression>()
                .REGISTER_VARIABLE<string>("expression", "m_expression");
            REGISTER_BTNODE<BTConditionRandom>()
                .REGISTER_VARIABLE<FixPoint>("pass_rate", "m_pass_rate");
            //Decorators
            REGISTER_BTNODE<BTFalse>();
            REGISTER_BTNODE<BTNot>();
            REGISTER_BTNODE<BTPulse>()
                .REGISTER_VARIABLE<FixPoint>("interval", "m_interval");
            REGISTER_BTNODE<BTTrue>();
            //Ext
            REGISTER_BTNODE<BTReference>();
            //test
            REGISTER_BTNODE<BTAction_Test>()
                .REGISTER_VARIABLE<int>("config_int", "m_int")
                .REGISTER_VARIABLE<FixPoint>("config_fp", "m_fp")
                .REGISTER_VARIABLE<string>("config_string", "m_string")
                .REGISTER_VARIABLE<bool>("config_bool", "m_bool")
                .REGISTER_VARIABLE_CRC<int>("config_crc", "m_crcint")
                .REGISTER_VARIABLE<Formula>("config_formula", "m_formula")
                ;
            REGISTER_BTNODE<BTAction_Test2>();
            #endregion

            #region 偏游戏无关的
            //AI Conditions
            REGISTER_BTNODE<BTAICondition_HasTarget>();
            //AI Actions
            REGISTER_BTNODE<BTAIAction_GatherTarget>()
                .REGISTER_VARIABLE_CRC<int>("gathering_type", "m_target_gathering_param.m_type")
                .REGISTER_VARIABLE<FixPoint>("gathering_param1", "m_target_gathering_param.m_param1")
                .REGISTER_VARIABLE<FixPoint>("gathering_param2", "m_target_gathering_param.m_param2")
                .REGISTER_VARIABLE_CRC<int>("gathering_faction", "m_target_gathering_param.m_faction")
                .REGISTER_VARIABLE_CRC<int>("gathering_category", "m_target_gathering_param.m_category");
            REGISTER_BTNODE<BTAIAction_MoveToTarget>()
                .REGISTER_VARIABLE<FixPoint>("range", "m_range");
            //Skill Conditions
            //Skill Actions
            REGISTER_BTNODE<BTSKillAction_ApplyDamageToTargets>()
                .REGISTER_VARIABLE_CRC<int>("damage_type", "m_damage_type_id")
                .REGISTER_VARIABLE<Formula>("damage_amount", "m_damage_amount")
                .REGISTER_VARIABLE<int>("damage_render_effect", "m_damage_render_effect_cfgid")
                .REGISTER_VARIABLE<int>("damage_sound", "m_damage_sound_cfgid");
            REGISTER_BTNODE<BTSKillAction_ApplyEffectToTargets>()
                .REGISTER_VARIABLE<int>("generator_id", "m_generator_cfgid");
            REGISTER_BTNODE<BTSKillAction_CreateObject>()
                .REGISTER_VARIABLE<int>("object_type_id", "m_object_type_id")
                .REGISTER_VARIABLE<int>("object_proto_id", "m_object_proto_id")
                .REGISTER_VARIABLE<FixPoint>("object_life_time", "m_object_life_time")
                .REGISTER_VARIABLE<int>("generator_id", "m_generator_cfgid")
                .REGISTER_VARIABLE<FixPoint>("offset_x", "m_offset.x")
                .REGISTER_VARIABLE<FixPoint>("offset_y", "m_offset.y")
                .REGISTER_VARIABLE<FixPoint>("offset_z", "m_offset.z");
            REGISTER_BTNODE<BTSKillAction_GatherTargets>()
                .REGISTER_VARIABLE_CRC<int>("gathering_type", "m_target_gathering_param.m_type")
                .REGISTER_VARIABLE<FixPoint>("gathering_param1", "m_target_gathering_param.m_param1")
                .REGISTER_VARIABLE<FixPoint>("gathering_param2", "m_target_gathering_param.m_param2")
                .REGISTER_VARIABLE_CRC<int>("gathering_faction", "m_target_gathering_param.m_faction")
                .REGISTER_VARIABLE_CRC<int>("gathering_category", "m_target_gathering_param.m_category");
            REGISTER_BTNODE<BTSKillAction_KillTargets>();
            REGISTER_BTNODE<BTSKillAction_PlayAction>()
                .REGISTER_VARIABLE<string>("animation", "m_animation")
                .REGISTER_VARIABLE<string>("next_animation", "m_next_animation")
                .REGISTER_VARIABLE<bool>("loop", "m_loop");
            REGISTER_BTNODE<BTSKillAction_PlayRenderEffect>()
                .REGISTER_VARIABLE<int>("render_effect_cfgid", "m_render_effect_cfgid");
            REGISTER_BTNODE<BTSKillAction_PlaySound>()
                .REGISTER_VARIABLE<int>("sound", "m_sound");
            REGISTER_BTNODE<BTSKillAction_Spurt>()
                .REGISTER_VARIABLE<FixPoint>("distance", "m_distance")
                .REGISTER_VARIABLE<FixPoint>("time", "m_time")
                .REGISTER_VARIABLE<int>("collision_target_generator_id", "m_collision_target_generator_cfgid")
                .REGISTER_VARIABLE<bool>("backward", "m_backward");
            #endregion

            #region 具体游戏
            //Crawl AI Conditions
            //Crawl AI Actions
            //Crawl Skill Conditions
            //Crawl Skill Actions
            #endregion
        }

        static BehaviorTreeNodePartialGenerator()
        {
            m_real_type[typeof(char)] = "char";
            m_real_type[typeof(short)] = "short";
            m_real_type[typeof(int)] = "int";
            m_real_type[typeof(long)] = "long";
            m_real_type[typeof(bool)] = "bool";
            m_real_type[typeof(float)] = "float";
            m_real_type[typeof(double)] = "double";
            m_real_type[typeof(string)] = "string";
        }
        static Dictionary<System.Type, string> m_real_type = new Dictionary<Type, string>();

        class EditorVariable
        {
            public string m_type_name;              //类型名，比如"int"、"FixPoint"
            public string m_config_name;            //配置文件中使用的名字，比如"max_speed"
            public string m_code_fragment;          //代码中这个值的代码片段，比如"m_max_speed"、"MaxSpeed"
            public int m_flag = 0;

            public bool IsFormula()
            {
                return m_type_name == "Formula";
            }
            public bool Transform2Crc()
            {
                return (m_flag & TRANSFORM2CRCID) != 0;
            }
            public bool NeedParse()
            {
                return m_type_name != "string";
            }
        }

        class EditorBTNode
        {
            public string m_name;
            public List<EditorVariable> m_variables = new List<EditorVariable>();
            public EditorBTNode REGISTER_VARIABLE<T>(string config_name, string code_fragment, int flag = 0)
            {
                RegisterVariableInternal<T>(config_name, code_fragment, flag);
                return this;
            }
            public EditorBTNode REGISTER_VARIABLE_CRC<T>(string config_name, string code_fragment, int flag = 0)
            {
                EditorVariable variable = RegisterVariableInternal<T>(config_name, code_fragment, flag);
                variable.m_flag |= TRANSFORM2CRCID;
                variable.m_type_name = "int";
                return this;
            }
            EditorVariable RegisterVariableInternal<T>(string config_name, string code_fragment, int flag)
            {
                EditorVariable variable = new EditorVariable();
                System.Type type = typeof(T);
                if (!m_real_type.TryGetValue(type, out variable.m_type_name))
                    variable.m_type_name = type.Name;
                variable.m_config_name = config_name;
                variable.m_code_fragment = code_fragment;
                variable.m_flag = flag;
                m_variables.Add(variable);
                return variable;
            }
        }

        static List<EditorBTNode> m_btnodes = new List<EditorBTNode>();
        static EditorBTNode REGISTER_BTNODE<T>()
        {
            EditorBTNode new_cmp = new EditorBTNode();
            new_cmp.m_name = typeof(T).Name;
            m_btnodes.Add(new_cmp);
            return new_cmp;
        }

        [MenuItem("FrameWork/Generate Combat BehaviorTree Code", false, 1002)]
        public static void GenerateAll()
        {
            StreamWriter writer = new StreamWriter("Assets/Scripts/CombatModule/LogicWorld/BehaviorTree/BehaviorTreeNodeTypeRegistryExt.cs");
            writer.Write(
@"using System;
using System.Collections;
using System.Collections.Generic;
namespace Combat
{");

            m_btnodes.Clear();
            InitAllBTNode();

            Generate_BehaviorTreeNodeTypeRegistry_RelatedCode(writer);

            int count = m_btnodes.Count;
            for (int i = 0; i < count; ++i)
            {
                if (i > 0)
                    writer.Write("\r\n\r\n");
                else
                    writer.Write("\r\n");
                GenerateOneBTNode(writer, m_btnodes[i]);
            }

            m_btnodes.Clear();
            writer.Write("\r\n}");
            writer.Flush();
            writer.Close();
            writer = null;
        }

        static void Generate_BehaviorTreeNodeTypeRegistry_RelatedCode(StreamWriter writer)
        {
            writer.Write("\r\n    public partial class BehaviorTreeNodeTypeRegistry\r\n    {");
            Generate_RegisterDefaultBTNodes(writer);
            writer.Write("\r\n    }\r\n");
        }

        static void Generate_RegisterDefaultBTNodes(StreamWriter writer)
        {
            writer.Write(
@"
        static public void RegisterDefaultNodes()
        {
            if (ms_default_btnodes_registered)
                return;
            ms_default_btnodes_registered = true;
");
            for (int i = 0; i < m_btnodes.Count; ++i)
            {
                writer.Write("\r\n            Register<");
                writer.Write(m_btnodes[i].m_name);
                writer.Write(">();");
            }
            writer.Write("\r\n        }");
        }

        static void GenerateOneBTNode(StreamWriter writer, EditorBTNode btnode)
        {
            writer.Write("    public partial class ");
            writer.Write(btnode.m_name);
            writer.Write("\r\n    {\r\n        public const int ID = ");
            writer.Write(((int)CRC.Calculate(btnode.m_name)).ToString());
            writer.Write(";");
            if (btnode.m_variables.Count > 0)
                Generate_InitializeVariable(writer, btnode);
            writer.Write("\r\n    }");
        }

        static void Generate_InitializeVariable(StreamWriter writer, EditorBTNode btnode)
        {
            writer.Write(
@"

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;");
            for (int i = 0; i < btnode.m_variables.Count; ++i)
            {
                EditorVariable variable = btnode.m_variables[i];
                writer.Write("\r\n            if (variables.TryGetValue(\"");
                writer.Write(variable.m_config_name);
                writer.Write("\", out value))\r\n                ");
                writer.Write(variable.m_code_fragment);
                if (variable.IsFormula())
                {
                    writer.Write(".Compile(value);");
                }
                else if (variable.Transform2Crc())
                {
                    writer.Write(" = (int)CRC.Calculate(value);");
                }
                else if (variable.NeedParse())
                {
                    writer.Write(" = ");
                    writer.Write(variable.m_type_name);
                    writer.Write(".Parse(value);");
                }
                else
                {
                    writer.Write(" = value;");
                }
            }
            writer.Write("\r\n        }");
        }
    }
}