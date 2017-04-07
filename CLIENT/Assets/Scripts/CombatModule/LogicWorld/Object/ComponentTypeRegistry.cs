using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class ComponentTypeRegistry
    {
        static bool ms_default_components_registered = false;
        static Dictionary<int, System.Type> m_components_id2type = new Dictionary<int, System.Type>();
        static Dictionary<System.Type, int> m_components_type2id = new Dictionary<System.Type, int>();
        static HashSet<int> m_render_components_id = new HashSet<int>();
        static Dictionary<int, int> m_variable2component = new Dictionary<int, int>();

        public static void Register<TComponent>(bool is_render_componet)
        {
            Register(typeof(TComponent), is_render_componet);
        }

        public static void Register(System.Type type, bool is_render_componet)
        {
            int component_type_id = (int)CRC.Calculate(type.Name);
#if UNITY_EDITOR
            System.Type existed_type;
            if (m_components_id2type.TryGetValue(component_type_id, out existed_type))
            {
                if (type.Name != existed_type.Name)
                    LogWrapper.LogError("ComponentTypeRegistry, component ", type.FullName, " has same crcid with existed component ", existed_type.FullName);
            }
#endif
            m_components_id2type[component_type_id] = type;
            m_components_type2id[type] = component_type_id;
            if (is_render_componet)
                m_render_components_id.Add(component_type_id);
        }

        public static bool IsLogicComponent(int component_type_id)
        {
            return !m_render_components_id.Contains(component_type_id);
        }

        public static bool IsRenderComponent(int component_type_id)
        {
            return m_render_components_id.Contains(component_type_id);
        }

        public static System.Type ComponentID2Type(int component_type_id)
        {
            System.Type type = null;
            m_components_id2type.TryGetValue(component_type_id, out type);
            return type;
        }

        public static int ComponentType2ID(System.Type type)
        {
            int component_type_id = -1; ;
            m_components_type2id.TryGetValue(type, out component_type_id);
            return component_type_id;
        }

        public static Component CreateComponent(int component_type_id)
        {
            System.Type type = null;
            if (!m_components_id2type.TryGetValue(component_type_id, out type))
                return null;
            return System.Activator.CreateInstance(type) as Component;
        }

        public static void RegisterVariable(int variable_id, int component_type_id)
        {
#if UNITY_EDITOR
            if (m_variable2component.ContainsKey(variable_id))
                LogWrapper.LogError("ComponentTypeRegistry, variable id(", (uint)variable_id, ") has already existed.");
#endif
            m_variable2component[variable_id] = component_type_id;
        }

        public static int GetVariableOwnerComponentID(int variable_id)
        {
            int component_type_id;
            if (!m_variable2component.TryGetValue(variable_id, out component_type_id))
                component_type_id = 0;
            return component_type_id;
        }
    }
}