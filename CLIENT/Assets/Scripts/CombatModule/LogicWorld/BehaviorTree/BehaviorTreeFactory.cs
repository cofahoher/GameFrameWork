using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    class BehaviorTreeCache
    {
        public BehaviorTree m_proto = null;
        public List<BehaviorTree> m_cache = new List<BehaviorTree>();
    }

    public class BehaviorTreeFactory : Singleton<BehaviorTreeFactory>
    {
        const int MAX_CACHE_CNT = 30;
        IConfigProvider m_config_provider = null;
        private Dictionary<int, BehaviorTreeCache> m_pools = new Dictionary<int, BehaviorTreeCache>();

        private BehaviorTreeFactory()
        {
        }

        public override void Destruct()
        {
        }

        public void SetConfigProvider(IConfigProvider config_provider)
        {
            m_config_provider = config_provider;
        }

        public BehaviorTree CreateBehaviorTree(int bt_config_id)
        {
            bool is_new = false;
            BehaviorTreeCache pool = GetPool(bt_config_id, out is_new);
            if (pool == null || pool.m_proto == null)
                return null;
            BehaviorTree instance = null;
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

        public void RecycleBehaviorTree(BehaviorTree instance)
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
                BehaviorTree instance = pool.m_proto.CloneBehaviorTree();
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
                BehaviorTree instance = CreateBeahviorTreeFromConfig(bt_config_id);
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

        BehaviorTree CreateBeahviorTreeFromConfig(int bt_config_id)
        {
            BehaviorTreeData data = m_config_provider.GetBehaviorTreeData(bt_config_id);
            if (data == null)
                return null;
            BehaviorTree tree = new BehaviorTree(bt_config_id);
            for (int i = 0; i < data.m_entry_nodes.Count; ++i)
            {
                BTNode entry_node = CreateBTNode(data.m_entry_nodes[i]);
                if (entry_node != null)
                    tree.AddEntry(entry_node, data.m_entry_nodes[i].m_extra_data);
            }
            tree.SetSignalData(data.m_signal_datas);
            tree.SetEventData(data.m_event_datas);
            return tree;
        }

        BTNode CreateBTNode(BehaviorTreeNodeData node_data)
        {
            BTNode btnode = BehaviorTreeNodeTypeRegistry.CreateBTNode(node_data.m_node_type);
            if (btnode == null)
                return null;
            if (node_data.m_node_variables != null)
                btnode.InitializeVariable(node_data.m_node_variables);
            if (node_data.m_sub_nodes != null)
            {
                for (int i = 0; i < node_data.m_sub_nodes.Count; ++i)
                {
                    BTNode sub_btnode = CreateBTNode(node_data.m_sub_nodes[i]);
                    if (sub_btnode != null)
                        btnode.AddChild(sub_btnode);
                }
            }
            return btnode;
        }
    }
}