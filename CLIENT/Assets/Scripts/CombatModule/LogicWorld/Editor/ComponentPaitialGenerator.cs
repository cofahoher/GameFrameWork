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
    public class ComponentPaitialGenerator
    {
        static void InitLogicComponents()
        {
            //按类别然后按字母顺序，方便检查
            m_logic = true;
            //Object
            REGISTER_COMPONENT<LevelComponent>();
            REGISTER_COMPONENT<TurnManagerComponent>();
            //Player
            REGISTER_COMPONENT<FactionComponent>();
            REGISTER_COMPONENT<PlayerAIComponent>();
            REGISTER_COMPONENT<PlayerTargetingComponent>();
            //Entity
            REGISTER_COMPONENT<AIComponent>();
            REGISTER_COMPONENT<AttributeManagerComponent>();
            REGISTER_COMPONENT<DamagableComponent>();
            REGISTER_COMPONENT<DamageModificationComponent>();
            REGISTER_COMPONENT<DeathComponent>();
            REGISTER_COMPONENT<EffectManagerComponent>();
            REGISTER_COMPONENT<LocomotorComponent>()
                .REGISTER_VARIABLE<FixPoint>("max_speed", "VID_MaxSpeed", "m_current_max_speed", false);
            REGISTER_COMPONENT<ManaComponent>();
            REGISTER_COMPONENT<PositionComponent>()
                .REGISTER_VARIABLE<FixPoint>("x", "VID_X", "m_current_position.x")
                .REGISTER_VARIABLE<FixPoint>("y", "VID_Y", "m_current_position.y")
                .REGISTER_VARIABLE<FixPoint>("z", "VID_Z", "m_current_position.z")
                .REGISTER_VARIABLE<FixPoint>("angle", "VID_Angle", "m_current_angle")
                .REGISTER_VARIABLE<FixPoint>("ext_x", "VID_ExtX", "m_extents.x", false)
                .REGISTER_VARIABLE<FixPoint>("ext_y", "VID_ExtY", "m_extents.y", false)
                .REGISTER_VARIABLE<FixPoint>("ext_z", "VID_ExtZ", "m_extents.z", false)
                .REGISTER_VARIABLE<bool>("visible", "VID_Visible", "m_visible");
            REGISTER_COMPONENT<SkillManagerComponent>();
            REGISTER_COMPONENT<StateComponent>();
            //Skill
            REGISTER_COMPONENT<CreateObjectSkillComponent>();
            REGISTER_COMPONENT<EffectGeneratorSkillComponent>();
            REGISTER_COMPONENT<SkillDefinitionComponent>();
            REGISTER_COMPONENT<WeaponSkillComponent>();
            //Effect
            REGISTER_COMPONENT<AddStateEffectComponent>();
            REGISTER_COMPONENT<ApplyGeneratorEffectComponent>();
            REGISTER_COMPONENT<DamageEffectComponent>();
            REGISTER_COMPONENT<HealEffectComponent>();
        }

        static void InitRenderComponents()
        {
            m_logic = false;
            REGISTER_COMPONENT<AnimationComponent>();
            REGISTER_COMPONENT<AnimatorComponent>();
            REGISTER_COMPONENT<ModelComponent>(false, false, true)
                .REGISTER_VARIABLE<string>("asset", null, "m_asset_name");
        }

        static ComponentPaitialGenerator()
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
            public string m_type_name;      //类型名
            public string m_config_name;    //配置文件中使用的名字，比如"ext_x"
            public string m_code_name;      //代码中的变量名，比如"VID_ExtX"
            public string m_code_fragment;  //代码中这个值的来源，比如"m_extents.x"，可以是null，表示，不能GetSet
            public bool m_can_set = true;   //m_code_fragment非null时，是否可以设置
            public bool NeedCast()
            {
                return m_type_name != "FixPoint";
            }
            public bool NeedParse()
            {
                return m_type_name != "string";
            }
        }
        class EditorComponent
        {
            public string m_name;
            public List<EditorVariable> m_variables = new List<EditorVariable>();
            public bool m_getter;
            public bool m_setter;
            public bool m_initer;
            public EditorComponent REGISTER_VARIABLE<T>(string config_name, string code_name, string code_fragment = null, bool can_set = true)
            {
                EditorVariable variable = new EditorVariable();
                System.Type type = typeof(T);
                if (!m_real_type.TryGetValue(type, out variable.m_type_name))
                    variable.m_type_name = type.Name;
                variable.m_config_name = config_name;
                variable.m_code_name = code_name;
                variable.m_code_fragment = code_fragment;
                variable.m_can_set = can_set;
                m_variables.Add(variable);
                return this;
            }
        }
        static bool m_logic = true;
        static List<EditorComponent> m_logic_componnets = new List<EditorComponent>();
        static List<EditorComponent> m_render_componnets = new List<EditorComponent>();

        static EditorComponent REGISTER_COMPONENT<T>(bool getter = true, bool setter = true, bool initer = true)
        {
            EditorComponent new_cmp = new EditorComponent();
            new_cmp.m_name = typeof(T).Name;
            new_cmp.m_getter = getter;
            new_cmp.m_setter = setter;
            new_cmp.m_initer = initer;
            if (m_logic)
                m_logic_componnets.Add(new_cmp);
            else
                m_render_componnets.Add(new_cmp);
            return new_cmp;
        }

        [MenuItem("H3D/Generate Combat Component Code", false, 999)]
        public static void GenerateAll()
        {
            StreamWriter writer = new StreamWriter("Assets/Scripts/CoreGame/LogicWorld/Object/ComponentTypeRegistryExt.cs");
            writer.Write(
@"using System;
using System.Collections;
using System.Collections.Generic;
namespace Combat
{");

            m_logic_componnets.Clear();
            m_render_componnets.Clear();
            InitLogicComponents();
            InitRenderComponents();

            GenerateComponentTypeRegistry(writer);

            m_logic = true;
            int count = m_logic_componnets.Count;
            for (int i = 0; i < count; ++i)
            {
                if (i > 0)
                    writer.Write("\r\n\r\n");
                else
                    writer.Write("\r\n");
                GenerateOneComponent(writer, m_logic_componnets[i]);
            }

            m_logic = false;
            writer.Write("\r\n\r\n#if COMBAT_CLIENT");
            count = m_render_componnets.Count;
            for (int i = 0; i < count; ++i)
            {
                if (i > 0)
                    writer.Write("\r\n\r\n");
                else
                    writer.Write("\r\n");
                GenerateOneComponent(writer, m_render_componnets[i]);
            }
            writer.Write("\r\n#endif\r\n");

            m_logic_componnets.Clear();
            m_render_componnets.Clear();
            writer.Write("}");
            writer.Flush();
            writer.Close();
            writer = null;
        }

        static void GenerateComponentTypeRegistry(StreamWriter writer)
        {
            writer.Write(
@"
    public partial class ComponentTypeRegistry
    {
        static public void RegisterDefaultComponents()
        {
            if (ms_default_components_registered)
                return;
            ms_default_components_registered = true;
");
            for (int i = 0; i < m_logic_componnets.Count; ++i)
            {
                writer.Write("\r\n            Register<");
                writer.Write(m_logic_componnets[i].m_name);
                writer.Write(">(false);");
            }
            writer.Write("\r\n\r\n#if COMBAT_CLIENT");
            for (int i = 0; i < m_render_componnets.Count; ++i)
            {
                writer.Write("\r\n            Register<");
                writer.Write(m_render_componnets[i].m_name);
                writer.Write(">(true);");
            }
            writer.Write("\r\n#endif\r\n");
            writer.Write("\r\n        }\r\n    }\r\n");
        }

        static void GenerateOneComponent(StreamWriter writer, EditorComponent component)
        {
            writer.Write("    public partial class ");
            writer.Write(component.m_name);
            writer.Write("\r\n    {\r\n        public const int ID = ");
            writer.Write(((int)CRC.Calculate(component.m_name)).ToString());
            writer.Write(";");
            if (component.m_variables.Count > 0)
                GenerateVariableRelatedCode(writer, component);
            writer.Write("\r\n    }");
        }

        static void GenerateVariableRelatedCode(StreamWriter writer, EditorComponent component)
        {
            int code_variable_count = 0;
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_name == null)
                    continue;
                ++code_variable_count;
                writer.Write("\r\n        public const int ");
                writer.Write(variable.m_code_name);
                writer.Write(" = ");
                writer.Write(((int)CRC.Calculate(variable.m_config_name)).ToString());
                writer.Write(";");
            }

            if (code_variable_count > 0)
                GenerateStaticConstructor(writer, component);
            if (component.m_getter && code_variable_count > 0)
                GenerateVariableGetter(writer, component);
            if (component.m_setter && code_variable_count > 0)
                GenerateVariableSetter(writer, component);
            if (component.m_initer)
                GenerateVariableIniter(writer, component);
        }

        static void GenerateStaticConstructor(StreamWriter writer, EditorComponent component)
        {
            writer.Write("\r\n\r\n        static ");
            writer.Write(component.m_name);
            writer.Write("()\r\n        {");
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_name == null)
                    continue;
                writer.Write("\r\n            ComponentTypeRegistry.RegisterVariable(");
                writer.Write(variable.m_code_name);
                writer.Write(", ID);");
            }
            writer.Write("\r\n        }");
        }

        static void GenerateVariableGetter(StreamWriter writer, EditorComponent component)
        {
            //        public override bool GetVariable(int id, out FixPoint value)
            //        {
            //            switch (id)
            //            {
            //            case VID_X:
            //                value = m_current_position.x;
            //                return true;
            //            default:
            //                value = FixPoint.Zero;
            //                return false;
            //            }
            //        }
            writer.Write(
@"

        public override bool GetVariable(int id, out FixPoint value)
        {
            switch (id)
            {");
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_fragment == null || variable.m_code_name == null)
                    continue;
                writer.Write("\r\n            case ");
                writer.Write(variable.m_code_name);
                writer.Write(":\r\n                value = ");
                if (variable.NeedCast())
                {
                    writer.Write("(FixPoint)(");
                    writer.Write(variable.m_code_fragment);
                    writer.Write(");");
                }
                else
                {
                    writer.Write(variable.m_code_fragment);
                    writer.Write(";");
                }
                writer.Write("\r\n                return true;");
            }
            writer.Write(
@"
            default:
                value = FixPoint.Zero;
                return false;
            }
        }");
        }

        static void GenerateVariableSetter(StreamWriter writer, EditorComponent component)
        {
            bool need = false;
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_fragment == null || variable.m_code_name == null || !variable.m_can_set)
                    continue;
                need = true;
                break;
            }
            if (!need)
                return;
            //        public override bool SetVariable(int id, FixPoint value)
            //        {
            //            switch (id)
            //            {
            //            case VID_X:
            //                m_current_position.x = value;
            //                return true;
            //            default:
            //                return false;
            //            }
            //        }
            writer.Write(
@"

        public override bool SetVariable(int id, FixPoint value)
        {
            switch (id)
            {");
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_fragment == null || variable.m_code_name == null || !variable.m_can_set)
                    continue;
                writer.Write("\r\n            case ");
                writer.Write(variable.m_code_name);
                writer.Write(":\r\n                ");
                writer.Write(variable.m_code_fragment);
                if (variable.NeedCast())
                {
                    writer.Write(" = (");
                    writer.Write(variable.m_type_name);
                    writer.Write(")value;");
                }
                else
                {
                    writer.Write(" = value;");
                }
                writer.Write("\r\n                return true;");
            }
            writer.Write(
@"
            default:
                return false;
            }
        }");
        }

        static void GenerateVariableIniter(StreamWriter writer, EditorComponent component)
        {
            //        public override void InitializeVariable(Dictionary<string, string> variables)
            //        {
            //            string value;
            //            if (variables.TryGetValue("x", out value))
            //                m_current_position.x = int.Parse(value);
            //        }
            writer.Write(
@"

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;");
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                writer.Write("\r\n            if (variables.TryGetValue(\"");
                writer.Write(variable.m_config_name);
                writer.Write("\", out value))\r\n                ");
                writer.Write(variable.m_code_fragment);
                if (variable.NeedParse())
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