using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    class BehaviorTreeCache
    {
        public BeahviorTree m_proto = null;
        public List<BeahviorTree> m_cache = new List<BeahviorTree>();
    }

    public class BehaviorTreeFactory : Singleton<BehaviorTreeFactory>
    {
        const int MAX_CACHE_CNT = 30;
        private Dictionary<int, BehaviorTreeCache> m_pools = new Dictionary<int, BehaviorTreeCache>();

        private BehaviorTreeFactory()
        {
        }

        public override void Destruct()
        {
        }

        public BeahviorTree CreateBehaviorTree(int bt_config_id)
        {
            bool is_new = false;
            BehaviorTreeCache pool = GetPool(bt_config_id, out is_new);
            if (pool == null || pool.m_proto == null)
                return null;
            BeahviorTree instance = null;
            int cache_count = pool.m_cache.Count;
            if (cache_count > 0)
            {
                instance = pool.m_cache[cache_count - 1];
                pool.m_cache.RemoveAt(cache_count - 1);
            }
            else
            {
                instance = pool.m_proto.CloneBehaviorTree();
            }
            return instance;
        }

        public void RecycleBehaviorTree(BeahviorTree instance)
        {
            if (instance == null)
                return;
            int bt_config_id = instance.ConfigID;
            bool is_new = false;
            BehaviorTreeCache pool = GetPool(bt_config_id, out is_new);
            if (pool == null || pool.m_proto == null)
                return;
            instance.Reset();
            pool.m_cache.Add(instance);
        }

        public void CacheBehaviorTree(int bt_config_id, int cache_cnt)
        {
            bool is_new = false;
            BehaviorTreeCache pool = GetPool(bt_config_id, out is_new);
            if (pool == null || pool.m_proto == null)
                return;
            if (is_new)
            {
                --cache_cnt;
            }
            else
            {
                if (cache_cnt + pool.m_cache.Count > MAX_CACHE_CNT)
                    cache_cnt = MAX_CACHE_CNT - pool.m_cache.Count;
            }
            for (int i = 0; i < cache_cnt; ++i)
            {
                BeahviorTree instance = pool.m_proto.CloneBehaviorTree();
                pool.m_cache.Add(instance);
            }
        }

        BehaviorTreeCache GetPool(int bt_config_id, out bool is_new)
        {
            is_new = false;
            BehaviorTreeCache pool = null;
            if (!m_pools.TryGetValue(bt_config_id, out pool))
            {
                is_new = true;
                pool = new BehaviorTreeCache();
                BeahviorTree instance = CreateBeahviorTreeFromConfig(bt_config_id);
                if (instance == null)
                {
                    LogWrapper.LogError("BehaviorTreeFactory, INVALID ID, ", bt_config_id);
                }
                else
                {
                    pool.m_proto = instance;
                    pool.m_cache.Add(instance);
                }
                m_pools[bt_config_id] = pool;
            }
            return pool;
        }

        BeahviorTree CreateBeahviorTreeFromConfig(int bt_config_id)
        {
            // ZZWTODO 获取配置解析
            //BeahviorTree tree = new BeahviorTree(bt_config_id);
            //return tree;
            return null;
        }
    }
}