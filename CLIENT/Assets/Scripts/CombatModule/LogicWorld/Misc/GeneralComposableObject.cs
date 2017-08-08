using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public interface IGeneralComponent<TOwner, TTime> : IDestruct
    {
        void Construct(TOwner owner);
        void Update(TTime delta_time, TTime total_time);
    }

    public class GeneralComponent<TOwner, TTime> : IGeneralComponent<TOwner, TTime>
    {
        TOwner m_owner;
        public virtual void Construct(TOwner owner)
        {
            m_owner = owner;
        }
        public virtual void Destruct()
        {
        }
        public virtual void Update(TTime delta_time, TTime total_time)
        {
        }
        public TOwner GetOwner()
        {
            return m_owner;
        }
        public TOwner Owner
        {
            get { return m_owner; }
            set { m_owner = value; }
        }
    }

    public abstract class GeneralComposableObject<TOwner, TTime>
    {
        protected Dictionary<System.Type, IGeneralComponent<TOwner, TTime>> m_components = null;
        protected List<IGeneralComponent<TOwner, TTime>> m_updateable_component = null;
        int m_updateable_cnt = 0;

        public TComponent AddComponent<TComponent>(bool need_update = false) where TComponent : class, IGeneralComponent<TOwner, TTime>, new()
        {
            if (m_components == null)
                m_components = new Dictionary<System.Type, IGeneralComponent<TOwner, TTime>>();
            TComponent component = new TComponent();
            component.Construct(GetSelf());
            m_components[typeof(TComponent)] = component;
            if (need_update)
            {
                if (m_updateable_component == null)
                    m_updateable_component = new List<IGeneralComponent<TOwner, TTime>>();
                m_updateable_component.Add(component);
                ++m_updateable_cnt;
            }
            return component;
        }

        public TComponent GetComponent<TComponent>() where TComponent : class, IGeneralComponent<TOwner, TTime>
        {
            if (m_components == null)
                return null;
            IGeneralComponent<TOwner, TTime> component;
            if (!m_components.TryGetValue(typeof(TComponent), out component))
                return null;
            return component as TComponent;
        }

        protected void UpdateGeneralComponent(TTime delta_time, TTime total_time)
        {
            if (m_updateable_cnt > 0)
            {
                for (int i = 0; i < m_updateable_component.Count; ++i)
                    m_updateable_component[i].Update(delta_time, total_time);
            }
        }

        protected void DestroyAllGeneralComponent()
        {
            if (m_components == null)
                return;
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
                enumerator.Current.Value.Destruct();
            m_components.Clear();
            m_components = null;
            if (m_updateable_component != null)
            {
                m_updateable_component.Clear();
                m_updateable_component = null;
            }
        }

        protected abstract TOwner GetSelf();
    }

#if UNITY_EDITOR
    public class SomeLogicComponent : GeneralComponent<LogicWorld, FixPoint>
    {
        public override void Update(FixPoint delta_time, FixPoint total_time)
        {
        }
    }

    public class SomeRenderComponent : GeneralComponent<RenderWorld, int>
    {
        public override void Update(int delta_time, int total_time)
        {
        }
    }
#endif
}
