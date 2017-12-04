using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTAction_RemoveContextValue : BTAction
    {
        //配置数据
        int m_context_key = -1;

        public BTAction_RemoveContextValue()
        {
        }

        public BTAction_RemoveContextValue(BTAction_RemoveContextValue prototype)
            : base(prototype)
        {
            m_context_key = prototype.m_context_key;
        }

        protected override void ResetRuntimeData()
        {
        }

        public override void ClearRunningTrace()
        {
        }

        protected override void OnActionEnter()
        {
            m_context.RemoveData(m_context_key);
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
        }

        protected override void OnActionExit()
        {
        }
    }
}