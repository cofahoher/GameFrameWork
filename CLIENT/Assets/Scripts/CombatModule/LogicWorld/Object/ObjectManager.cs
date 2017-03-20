using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public abstract class ObjectManager<TObject> : IDestruct where TObject : Object
    {
        protected LogicWorld m_logic_world;
        protected IDGenerator m_id_generator;
        protected SortedDictionary<int, TObject> m_objects = new SortedDictionary<int, TObject>();
        protected SortedDictionary<string, TObject> m_named_objects = new SortedDictionary<string, TObject>();
        protected bool m_is_dirty = false;

        public ObjectManager(LogicWorld logic_world, int first_id)
        {
            m_logic_world = logic_world;
            m_id_generator = new IDGenerator(first_id);
        }

        public virtual void Destruct()
        {
            List<int> ids = new List<int>();
            var keys = m_objects.Keys;
            var enumerator = keys.GetEnumerator();
            while (enumerator.MoveNext())
            {
                int id = enumerator.Current;
                ids.Add(id);
            }
            for (int i = 0; i < ids.Count; ++i)
            {
                TObject obj;
                if (!m_objects.TryGetValue(ids[i], out obj))
                    continue;
                PreDestroyObject(obj);
                obj.Destruct();
            }
            m_objects.Clear();
            m_named_objects.Clear();

            m_logic_world = null;
        }

        public bool Dirty
        {
            get { return m_is_dirty; }
            set { m_is_dirty = value; }
        }

        public TObject GetObject(int object_id)
        {
            TObject obj;
            m_objects.TryGetValue(object_id, out obj);
            return obj;
        }

        public TObject GetObject(string name)
        {
            TObject obj;
            m_named_objects.TryGetValue(name, out obj);
            return obj;
        }

        public TObject CreateObject(ObjectCreationContext context)
        {
            if (context.m_object_id < 0)
            {
                int id = m_id_generator.GenID();
                context.m_object_id = id;
            }
            TObject obj = CreateObjectInstance();
            m_objects[context.m_object_id] = obj;
            if (context.m_name != null && context.m_name.Length > 0)
                m_named_objects[context.m_name] = obj;
            obj.InitializeObject(context);
            AfterObjectCreated(obj);
            m_is_dirty = true;
            return obj;
        }

        protected abstract TObject CreateObjectInstance();

        protected virtual void AfterObjectCreated(TObject obj)
        {
        }

        public void DestroyObject(int object_id)
        {
            TObject obj;
            if (!m_objects.TryGetValue(object_id, out obj))
                return;
            string name = obj.Name;
            if (name != null && name.Length > 0)
                m_named_objects.Remove(name);
            PreDestroyObject(obj);
            obj.Destruct();
            m_objects.Remove(object_id);
            m_is_dirty = true;
        }

        protected virtual void PreDestroyObject(TObject obj)
        {
        }

        public SortedDictionary<int, TObject> GetAllObjects()
        {
            return m_objects;
        }
    }
}