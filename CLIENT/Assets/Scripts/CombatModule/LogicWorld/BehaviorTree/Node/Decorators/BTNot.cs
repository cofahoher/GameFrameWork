using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTNot : BTDecorator
    {
        public BTNot()
        {
        }

        public BTNot(BTNot prototype)
            : base(prototype)
        {
        }

        protected override void ResetRuntimeData()
        {
        }

        protected override BTNodeStatus Decorate(BTNodeStatus status)
        {
            if (status == BTNodeStatus.False)
                return BTNodeStatus.True;
            else if (status == BTNodeStatus.True)
                return BTNodeStatus.False;
            else
                return status;
        }
    }
}