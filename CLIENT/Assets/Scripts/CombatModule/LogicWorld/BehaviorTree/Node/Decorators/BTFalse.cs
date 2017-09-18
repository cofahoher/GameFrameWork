using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTFalse : BTDecorator
    {
        public BTFalse()
        {
        }

        public BTFalse(BTFalse prototype)
            : base(prototype)
        {
        }

        protected override void ResetRuntimeData()
        {
        }

        protected override BTNodeStatus Decorate(BTNodeStatus status)
        {
            return BTNodeStatus.False;
        }
    }
}