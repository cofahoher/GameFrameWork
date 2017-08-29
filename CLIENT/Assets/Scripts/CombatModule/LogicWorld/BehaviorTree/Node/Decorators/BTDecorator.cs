using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public abstract class BTDecorator : BTNode
    {
        public BTDecorator()
        {
        }

        public BTDecorator(BTDecorator prototype)
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
            BTNodeStatus status = BTNodeStatus.False;
            if (CanExecute())
            {
                if (m_children != null)
                    status = m_children[0].OnUpdate();
                status = this.Decorate(status);
                PrepareForNextExecute();
            }
            m_status = status;
            return m_status;
        }

        protected virtual bool CanExecute()
        {
            return true;
        }

        protected virtual void PrepareForNextExecute()
        {
        }

        protected virtual BTNodeStatus Decorate(BTNodeStatus status)
        {
            return status;
        }
    }
}