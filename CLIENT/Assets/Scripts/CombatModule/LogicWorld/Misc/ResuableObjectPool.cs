using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public interface IRecyclable
    {
        void Reset();  //Reset后，如同刚被构造
    }

    public class ResuableObjectPool<TT> : Singleton<ResuableObjectPool<TT>> where TT : IRecyclable
    {
        Dictionary<System.Type, List<IRecyclable>> m_pools = new Dictionary<System.Type, List<IRecyclable>>();

        private ResuableObjectPool()
        {
        }

        public override void Destruct()
        {
            Clear();
        }

        public T Create<T>() where T : IRecyclable, new()
        {
            System.Type type = typeof(T);
            T instance = default(T);
            List<IRecyclable> pool = null;
            if (!m_pools.TryGetValue(type, out pool))
            {
                instance = new T();
            }
            else
            {
                int cache_count = pool.Count;
                if (cache_count > 0)
                {
                    instance = (T)pool[cache_count - 1];
                    pool.RemoveAt(cache_count - 1);
                }
                else
                {
                    instance = new T();
                }
            }
            return instance;
        }

        public void Recycle(TT instance)
        {
            instance.Reset();
            System.Type type = instance.GetType();
            List<IRecyclable> pool = null;
            if (!m_pools.TryGetValue(type, out pool))
            {
                pool = new List<IRecyclable>();
                m_pools[type] = pool;
            }
            pool.Add(instance);
        }

        public void Clear()
        {
            m_pools.Clear();
        }
    }

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
}