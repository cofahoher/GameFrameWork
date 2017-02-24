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

    public class Object : SignalGenerator, ILogicOwnerInfo, IDestruct
    {
        protected ObjectCreationContext m_context;
        protected SortedDictionary<System.Type, Component> m_components = new SortedDictionary<System.Type, Component>();
        protected bool m_is_delete_pending = false;

        public Object()
        {
        }

        #region GETTER
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
        public int GetCurrentTime()
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

        #region Destruct
        public void Destruct()
        {
            OnDestruct();
            NotifyGeneratorDestroy();
            RemoveAllListeners();
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
        #endregion

        #region Construct
        public void InitializeObject(ObjectCreationContext context)
        {
            PreInitializeObject(context);
            InitializeComponents(context);
            PostInitializeObject(context);
        }

        protected virtual void PreInitializeObject(ObjectCreationContext context)
        {
        }

        void InitializeComponents(ObjectCreationContext context)
        {
            m_context = context;
            List<ComponentData> components_data = context.m_type_data.m_components_data;
            for (int i = 0; i < components_data.Count; ++i)
                AddComponent(components_data[i].m_component_type_id);
            InitializeComponentProperties(context.m_type_data);
            InitializeComponentProperties(context.m_custom_data);

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

        void InitializeComponentProperties(ObjectTypeData type_data)
        {
            if (type_data == null)
                return;
            List<ComponentData> components_data = type_data.m_components_data;
            for (int i = 0; i < components_data.Count; ++i)
            {
                List<ComponentProperty> properties = components_data[i].m_component_properties;
                if (properties == null || properties.Count == 0)
                    continue;
                Component component = GetComponent(components_data[i].m_component_type_id);
                if (component == null)
                    continue;
                for (int j = 0; j < properties.Count; ++j)
                    component.InitializeProperty(properties[j]);
            }
        }

        protected virtual void PostInitializeObject(ObjectCreationContext context)
        {
        }

        Component AddComponent(int component_type_id)
        {
            if (!CanAddComponent(component_type_id))
                return null;
            System.Type type = ComponentTypeRegistry.GetComponentClassType(component_type_id);
            if (type == null)
                return null;
            Component component = System.Activator.CreateInstance(type) as Component;
            if (component == null)
                return null;
            component.ParentObject = this;
            component.ComponentTypeID = component_type_id;
            m_components[type] = component;
            return component;
        }

        protected virtual bool CanAddComponent(int component_type_id)
        {
            return ComponentTypeRegistry.IsLogicComponent(component_type_id);
        }
        #endregion

        #region Components
        public T GetComponent<T>() where T : Component
        {
            Component component;
            if (!m_components.TryGetValue(typeof(T), out component))
                return null;
            return component as T;
        }

        public Component GetComponent(System.Type type)
        {
            Component component;
            m_components.TryGetValue(type, out component);
            return component;
        }

        public Component GetComponent(int component_type_id)
        {
            System.Type type = ComponentTypeRegistry.GetComponentClassType(component_type_id);
            if (type == null)
                return null;
            return GetComponent(type);
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