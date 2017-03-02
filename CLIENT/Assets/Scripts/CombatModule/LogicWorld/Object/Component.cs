using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public abstract class Component : ILogicOwnerInfo, IDestruct
    {
        protected Object m_parent_object;
        protected int m_component_type_id = -1;
        protected int m_disable_count = 0;

        public Component()
        {
        }

        #region GETTER
        public Object ParentObject
        {
            get { return m_parent_object; }
            set { m_parent_object = value; }
        }
        public int ComponentTypeID
        {
            get { return m_component_type_id; }
            set { m_component_type_id = value; }
        }
        #endregion

        #region ILogicOwnerInfo
        public LogicWorld GetLogicWorld()
        {
            return m_parent_object.GetLogicWorld();
        }
        public int GetCurrentTime()
        {
            return m_parent_object.GetCurrentTime();
        }
        public int GetOwnerObjectID()
        {
            return m_parent_object.ID;
        }
        public Object GetOwnerObject()
        {
            return m_parent_object;
        }
        public abstract int GetOwnerPlayerID();
        public abstract Player GetOwnerPlayer();
        public abstract int GetOwnerEntityID();
        public abstract Entity GetOwnerEntity();
        #endregion

        #region Destruct
        public void Destruct()
        {
            OnDestruct();
            m_parent_object = null;
        }

        public virtual void OnDestruct()
        {
        }
        #endregion

        #region Construct
        public virtual void InitializeVariable(ComponentVariable property)
        {
        }

        public virtual void InitializeComponent()
        {
        }

        public void OnObjectCreated()
        {
            PostInitializeComponent();
            if (m_disable_count == 0)
                OnEnable();
            else if (m_disable_count > 0)
                OnDisable();
        }

        protected virtual void PostInitializeComponent()
        {
        }
        #endregion

        #region Enable&Disable
        public void Enable()
        {
            if (m_disable_count > 0)
            {
                --m_disable_count;
                if (m_disable_count == 0)
                    OnEnable();
            }
        }

        public void Disable()
        {
            ++m_disable_count;
            if (m_disable_count == 1)
                OnDisable();
        }

        public bool IsEnable()
        {
            return m_disable_count == 0;
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }
        #endregion
    }
}