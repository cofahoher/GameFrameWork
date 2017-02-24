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

        public virtual void Destruct()
        {
            m_parent_object = null;
        }

        public virtual void InitializeProperty(ComponentProperty property)
        {
        }

        public virtual void PostInitializeComponent()
        {
        }

        public virtual void AfterObjectCreated()
        {
            if (m_disable_count == 0)
                OnEnable();
        }

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
    }
}