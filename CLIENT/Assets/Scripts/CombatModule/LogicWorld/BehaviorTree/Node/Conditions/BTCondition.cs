using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public abstract class BTCondition : BTNode
    {
        public BTCondition()
        {
        }

        public BTCondition(BTCondition prototype)
            : base(prototype)
        {
        }

        public override void ResetNode()
        {
            base.ResetNode();
            ResetRuntimeData();
        }

        protected abstract void ResetRuntimeData();

        public override BTNodeStatus OnUpdate()
        {
            if (IsSatisfy())
                m_status = BTNodeStatus.True;
            else
                m_status = BTNodeStatus.False;
            return m_status;
        }

        protected virtual bool IsSatisfy()
        {
            return true;
        }
    }
}