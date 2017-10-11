using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTAICondition_HasTarget : BTCondition
    {
        public BTAICondition_HasTarget()
        {
        }

        public BTAICondition_HasTarget(BTAICondition_HasTarget prototype)
            : base(prototype)
        {
        }

        protected override void ResetRuntimeData()
        {
        }

        protected override bool IsSatisfy()
        {
            int current_target_id = (int)(m_context.GetData(BTContextKey.CurrentTargetID));
            if (current_target_id > 0)
                return true;
            else
                return false;
        }
    }
}