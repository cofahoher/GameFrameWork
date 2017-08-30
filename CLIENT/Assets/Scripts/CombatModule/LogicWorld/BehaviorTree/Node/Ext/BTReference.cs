using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BTReference : BTNode
    {
        //配置数据
        protected int m_reference_tree_id = -1;

        //运行数据
        private BeahviorTree m_reference_tree = null;

        public BTReference(int reference_tree_id)
        {
            m_reference_tree_id = reference_tree_id;
            CreateReferenceTree();
        }

        public BTReference(BTReference prototype)
            : base(prototype)
        {
            m_reference_tree_id = prototype.m_reference_tree_id;
            CreateReferenceTree();
        }

        protected void CreateReferenceTree()
        {
            if (m_reference_tree_id < 0)
                return;
            m_reference_tree = BehaviorTreeFactory.Instance.CreateBehaviorTree(m_reference_tree_id);
            if (m_reference_tree == null)
                return;
            AddChild(m_reference_tree);
        }

        public override BTNodeStatus OnUpdate()
        {
            if (m_reference_tree == null)
                m_status = BTNodeStatus.False;
            else
                m_status = m_reference_tree.OnUpdate();
            return m_status;
        }
    }
}