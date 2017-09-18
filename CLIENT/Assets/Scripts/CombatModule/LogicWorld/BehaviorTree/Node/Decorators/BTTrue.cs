using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTTrue : BTDecorator
    {
        public BTTrue()
        {
        }

        public BTTrue(BTTrue prototype)
            : base(prototype)
        {
        }

        protected override void ResetRuntimeData()
        {
        }

        protected override BTNodeStatus Decorate(BTNodeStatus status)
        {
            return BTNodeStatus.True;
        }
    }
}