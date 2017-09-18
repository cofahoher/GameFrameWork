using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTAction_StopTreeUpdate : BTAction
    {
        //配置数据

        //运行数据

        public BTAction_StopTreeUpdate()
        {
        }

        public BTAction_StopTreeUpdate(BTAction_StopTreeUpdate prototype)
            : base(prototype)
        {
        }

        protected override void ResetRuntimeData()
        {
        }

        public override void ClearRunningTrace()
        {
        }

        protected override void OnActionEnter()
        {
            m_context.GetBeahviorTree().StopUpdate();
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
        }

        protected override void OnActionExit()
        {
        }
    }
}