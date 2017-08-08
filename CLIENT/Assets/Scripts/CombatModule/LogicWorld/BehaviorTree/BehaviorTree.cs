using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BeahviorTree : BTNode, IRecyclable
    {
        //配置数据
        int m_config_id = 0;
        FixPoint m_update_interval = FixPoint.One;
        //运行数据
        bool m_active = false;

        #region 创建
        public BeahviorTree(int config_id)
        {
            m_config_id = config_id;
        }

        public BeahviorTree(BeahviorTree prototype)
            : base(prototype)
        {
            m_config_id = prototype.m_config_id;
            m_update_interval = prototype.m_update_interval;
        }

        public override void ResetNode()
        {
            base.ResetNode();
            m_active = false;
        }

        public void Reset()
        {
            ResetNode();
        }

        public BeahviorTree CloneBehaviorTree()
        {
            BeahviorTree clone = Clone() as BeahviorTree;
            return clone;
        }
        #endregion

        #region GETTER
        public int ConfigID
        {
            get { return m_config_id; }
        }
        #endregion

        public override BTNodeStatus OnUpdate()
        {
            if (m_children == null)
                return BTNodeStatus.False;
            return m_children[0].OnUpdate();
        }
    }
}