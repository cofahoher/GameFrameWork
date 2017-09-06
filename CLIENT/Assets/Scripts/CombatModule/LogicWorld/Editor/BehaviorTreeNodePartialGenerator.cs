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
            REGISTER_BTNODE<BTAction_Test>()
                .REGISTER_VARIABLE<int>("config_int", "m_int")
                .REGISTER_VARIABLE<FixPoint>("config_fp", "m_fp")
                .REGISTER_VARIABLE<string>("config_string", "m_string")
                .REGISTER_VARIABLE<bool>("config_bool", "m_bool")
                .REGISTER_VARIABLE_CRC<int>("config_crc", "m_crcint")
                .REGISTER_VARIABLE<Formula>("config_formula", "m_formula")
                ;
            REGISTER_BTNODE<BTAction_Test2>();
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

        [MenuItem("FrameWork/Generate Combat BehaviorTree Code", false, 102)]
        public static void GenerateAll()
        {
            StreamWriter writer = new StreamWriter("Assets/Scripts/CoreGame/LogicWorld/BehaviorTree/BehaviorTreeNodeTypeRegistryExt.cs");
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