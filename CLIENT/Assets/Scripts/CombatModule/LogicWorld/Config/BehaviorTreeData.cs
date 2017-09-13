using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BehaviorTreeData
    {
        public int m_id = 0;
        public string m_description;
        public List<BehaviorTreeEntryNodeData> m_entry_nodes = new List<BehaviorTreeEntryNodeData>();
    }

    public class BehaviorTreeNodeData
    {
        public int m_node_type = 0;
        public Dictionary<string, string> m_node_variables;
        public List<BehaviorTreeNodeData> m_sub_nodes;
    }

    public struct BehaviorTreeEntryNodeExtraData
    {
        public int m_entry_name_id;
        public FixPoint m_update_interval;
    }

    public class BehaviorTreeEntryNodeData : BehaviorTreeNodeData
    {
        public BehaviorTreeEntryNodeExtraData m_extra_data = new BehaviorTreeEntryNodeExtraData();
        public string m_description;
    }
}