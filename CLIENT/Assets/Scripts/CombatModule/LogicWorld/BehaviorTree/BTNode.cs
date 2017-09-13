using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum BTNodeStatus
    {
        Unreach,
        False,
        True,
        Running,
    }

    public abstract class BTNode
    {
        //配置数据
        protected List<BTNode> m_children = null;
        //运行数据
        protected BTNodeStatus m_status = BTNodeStatus.Unreach;
        //非节点数据
        protected BTContext m_context;

        #region 创建
        public BTNode()
        {
        }

        public BTNode(BTNode prototype)
        {
            if (prototype.m_children != null)
            {
                for (int i = 0; i < prototype.m_children.Count; ++i)
                {
                    BTNode clone = prototype.m_children[i].Clone();
                    AddChild(clone);
                }
            }
        }
        public virtual void InitializeVariable(Dictionary<string, string> variables)
        {
        }

        public BTNode Clone()
        {
            System.Type t = this.GetType();
            BTNode clone = System.Activator.CreateInstance(t, this) as BTNode;
            return clone;
        }

        public virtual void ResetNode()
        {
            m_status = BTNodeStatus.Unreach;
            if (m_children != null)
            {
                for (int i = 0; i < m_children.Count; ++i)
                    m_children[i].ResetNode();
            }
        }

        public virtual void ClearRunningTrace()
        {
        }

        public void SetContext(BTContext context)
        {
            m_context = context;
            if (m_children != null)
            {
                for (int i = 0; i < m_children.Count; ++i)
                    m_children[i].SetContext(context);
            }
        }
        #endregion

        #region Getter
        public BTNodeStatus CurrentStatus
        {
            get { return m_status; }
        }

        public BTContext Context
        {
            get { return m_context; }
        }

        public List<BTNode> GetChildren()
        {
            return m_children;
        }
        #endregion

        public void AddChild(BTNode node)
        {
            if (node == null)
                return;
            if (m_children == null)
                m_children = new List<BTNode>();
            m_children.Add(node);
        }

        public abstract BTNodeStatus OnUpdate();

        public virtual void ShutDown()
        {
            if (m_children != null)
            {
                for (int i = 0; i < m_children.Count; ++i)
                    m_children[i].ShutDown();
            }
        }
    }
}