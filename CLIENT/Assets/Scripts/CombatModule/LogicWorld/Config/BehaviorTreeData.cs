using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BehaviorTreeData
    {
        public string m_name;
        public FixPoint m_update_interval = FixPoint.One;
        public BehaviorTreeNodeData m_root_node = new BehaviorTreeNodeData();
    }

    public class BehaviorTreeNodeData
    {
        public int m_node_type = 0;
        public Dictionary<string, string> m_node_variables;
        public List<BehaviorTreeNodeData> m_sub_nodes;
    }
}