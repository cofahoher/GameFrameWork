using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public abstract class BTComposite : BTNode
    {
        public BTComposite()
        {
        }

        public BTComposite(BTComposite prototype)
            : base(prototype)
        {
        }

        public override void ResetNode()
        {
            base.ResetNode();
            ResetRuntimeData();
        }

        protected abstract void ResetRuntimeData();
    }
}