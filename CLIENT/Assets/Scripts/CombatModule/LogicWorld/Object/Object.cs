using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    enum ObjectClassType
    {
        General = 0,
        Player,
        Entity,
        Ability,
        Effect,
        RenderEntity,
    }

    public abstract class Object : SignalGenerator, ILogicOwnerInfo, IDestruct
    {
        protected ObjectCreationContext m_context;
        protected SortedDictionary<int, Component> m_components = new SortedDictionary<int, Component>();
        protected bool m_is_delete_pending = false;

        public Object()
        {
        }

        public void Destruct()
        {
            OnDestruct();
            NotifyGeneratorDestroyAndRemoveAllListeners();
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Component component = enumerator.Current.Value;
                component.Destruct();
            }
            m_components.Clear();
        }

        protected virtual void OnDestruct()
        {
        }

        #region GETTER
        public ObjectCreationContext GetCreationContext()
        {
            return m_context;
        }
        public int ID
        {
            get { return m_context.m_object_id; }
        }
        public string Name
        {
            get { return m_context.m_name; }
        }
        public bool DeletePending
        {
            get { return m_is_delete_pending; }
        }
        #endregion

        #region SignalGenerator
        protected override LogicWorld GetLogicWorldForSignal()
        {
            return m_context.m_logic_world;
        }
        #endregion

        #region ILogicOwnerInfo
        public LogicWorld GetLogicWorld()
        {
            return m_context.m_logic_world;
        }
        public FixPoint GetCurrentTime()
        {
            return m_context.m_logic_world.CurrentTime;
        }
        public int GetOwnerObjectID()
        {
            return m_context.m_owner_id;
        }
        public virtual Object GetOwnerObject()
        {
            return null;
        }
        public virtual int GetOwnerPlayerID()
        {
            return 0;
        }
        public virtual Player GetOwnerPlayer()
        {
            return null;
        }
        public virtual int GetOwnerEntityID()
        {
            return 0;
        }
        public virtual Entity GetOwnerEntity()
        {
            return null;
        }
        #endregion

        #region 初始化
        public void InitializeObject(ObjectCreationContext context)
        {
            m_context = context;
            PreInitializeObject(context);
            InitializeComponents(context);
            PostInitializeObject(context);
        }

        protected virtual void PreInitializeObject(ObjectCreationContext context)
        {
        }

        void InitializeComponents(ObjectCreationContext context)
        {
            List<ComponentData> components_data = context.m_type_data.m_components_data;
            for (int i = 0; i < components_data.Count; ++i)
                AddComponent(components_data[i].m_component_type_id);
            InitializeComponentVariables(context.m_type_data);

            //ZZWTODO context.m_proto_data attribute skill

            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Component component = enumerator.Current.Value;
                component.InitializeComponent();
            }

            enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Component component = enumerator.Current.Value;
                component.OnObjectCreated();
            }
        }

        void InitializeComponentVariables(ObjectTypeData type_data)
        {
            if (type_data == null)
                return;
            List<ComponentData> components_data = type_data.m_components_data;
            for (int i = 0; i < components_data.Count; ++i)
            {
                if (!IsSuitableComponent(components_data[i].m_component_type_id))
                    continue;
                Dictionary<string, string> variables = components_data[i].m_component_variables;
                if (variables == null || variables.Count == 0)
                    continue;
                Component component = GetComponent(components_data[i].m_component_type_id);
                if (component == null)
                    continue;
                component.InitializeVariable(variables);
            }
        }

        protected virtual void PostInitializeObject(ObjectCreationContext context)
        {
        }

        Component AddComponent(int component_type_id)
        {
            if (!IsSuitableComponent(component_type_id))
                return null;
            Component component = ComponentTypeRegistry.CreateComponent(component_type_id);
            if (component == null)
                return null;
            component.ParentObject = this;
            component.ComponentTypeID = component_type_id;
            m_components[component_type_id] = component;
            return component;
        }

        protected virtual bool IsSuitableComponent(int component_type_id)
        {
            return ComponentTypeRegistry.IsLogicComponent(component_type_id);
        }
        #endregion

        #region Components
        public T GetComponent<T>() where T : Component
        {
            Component component;
            int component_type_id = ComponentTypeRegistry.ComponentType2ID(typeof(T));
            if (!m_components.TryGetValue(component_type_id, out component))
                return null;
            return component as T;
        }

        public Component GetComponent(System.Type type)
        {
            Component component;
            int component_type_id = ComponentTypeRegistry.ComponentType2ID(type);
            m_components.TryGetValue(component_type_id, out component);
            return component;
        }

        public Component GetComponent(int component_type_id)
        {
            Component component;
            m_components.TryGetValue(component_type_id, out component);
            return component;
        }
        #endregion

        public bool IsDead()
        {
            if (m_is_delete_pending)
                return true;
            //ZZWTODO Death State
            return false;
        }
    }
}