using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public interface IRecyclable
    {
        void Reset();  //Reset后，如同刚被构造
    }

    public class ResuableObjectPool<TT> where TT : IRecyclable
    {
        Dictionary<System.Type, List<IRecyclable>> m_pools = new Dictionary<System.Type, List<IRecyclable>>();

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
    }
}