using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public interface IRecyclable
    {
        void Reset();  //Reset后，如同刚被构造
    }

    public class ResuableObjectPool<TBaseClassOrInterface> : Singleton<ResuableObjectPool<TBaseClassOrInterface>> where TBaseClassOrInterface : IRecyclable
    {
        Dictionary<System.Type, List<TBaseClassOrInterface>> m_pools = new Dictionary<System.Type, List<TBaseClassOrInterface>>();

        private ResuableObjectPool()
        {
        }

        public override void Destruct()
        {
            Clear();
        }

        public void Clear()
        {
            m_pools.Clear();
        }

        public TClass Create<TClass>() where TClass : class, TBaseClassOrInterface, new()
        {
            System.Type type = typeof(TClass);
            TClass instance = null;
            List<TBaseClassOrInterface> pool = null;
            if (!m_pools.TryGetValue(type, out pool))
            {
                instance = new TClass();
            }
            else
            {
                int cache_count = pool.Count;
                if (cache_count > 0)
                {
                    instance = (TClass)pool[cache_count - 1];
                    pool.RemoveAt(cache_count - 1);
                }
                else
                {
                    instance = new TClass();
                }
            }
            return instance;
        }

        public void Recycle(TBaseClassOrInterface instance)
        {
            instance.Reset();
            System.Type type = instance.GetType();
            List<TBaseClassOrInterface> pool = null;
            if (!m_pools.TryGetValue(type, out pool))
            {
                pool = new List<TBaseClassOrInterface>();
                m_pools[type] = pool;
            }
            pool.Add(instance);
        }

        public System.Object Create(System.Type type)
        {
            List<TBaseClassOrInterface> pool = null;
            if (!m_pools.TryGetValue(type, out pool))
            {
                System.Object instance = System.Activator.CreateInstance(type);
                return instance;
            }
            else
            {
                int cache_count = pool.Count;
                if (cache_count > 0)
                {
                    System.Object instance = pool[cache_count - 1];
                    pool.RemoveAt(cache_count - 1);
                    return instance;
                }
                else
                {
                    System.Object instance = System.Activator.CreateInstance(type);
                    return instance;
                }
            }
        }
    }

    public class ResuableObjectFactory<TBaseClass> where TBaseClass : class, IRecyclable
    {
        static bool ms_registered = false;
        static Dictionary<int, System.Type> m_id2type = new Dictionary<int, System.Type>();

        public static bool Registered
        {
            get { return ms_registered; }
            set { ms_registered = value; }
        }

        public static void Register(int id, System.Type type)
        {
            m_id2type[id] = type;
        }

        public static TBaseClass Create(int id)
        {
            System.Type type = null;
            if (!m_id2type.TryGetValue(id, out type))
                return null;
            TBaseClass base_obj = ResuableObjectPool<TBaseClass>.Instance.Create(type) as TBaseClass;
            return base_obj;
        }

        public static void Recycle(TBaseClass obj)
        {
            ResuableObjectPool<TBaseClass>.Instance.Recycle(obj);
        }

        public static TClass Create<TClass>() where TClass : class, TBaseClass, new()
        {
            return ResuableObjectPool<TBaseClass>.Instance.Create<TClass>();
        }
    }
    
#if UNITY_EDITOR
    public class TestRecyclable : IRecyclable
    {
        /*
         * 使用方式：
         * 
         * TestRecyclable temp = TestRecyclable.Create();
         * // 使用temp
         * TestRecyclable.Recycle(temp);
         * 
         */
        public static TestRecyclable Create()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<TestRecyclable>();
        }

        public static void Recycle(TestRecyclable instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }

        public TestRecyclable()
        {
        }

        public void Reset()
        {
        }
    }
#endif
}