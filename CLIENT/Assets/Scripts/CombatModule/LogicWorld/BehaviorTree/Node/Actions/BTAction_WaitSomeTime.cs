using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTAction_WaitSomeTime : BTAction
    {
        //配置数据
        FixPoint m_time = FixPoint.One;

        //运行数据
        FixPoint m_remain_time = FixPoint.Zero;

        public BTAction_WaitSomeTime()
        {
        }

        public BTAction_WaitSomeTime(BTAction_WaitSomeTime prototype)
            : base(prototype)
        {
            m_time = prototype.m_time;
        }

        protected override void ResetRuntimeData()
        {
            m_remain_time = FixPoint.Zero;
        }

        public override void ClearRunningTrace()
        {
            m_remain_time = FixPoint.Zero;
        }

        protected override void OnActionEnter()
        {
            m_status = BTNodeStatus.Running;
            m_remain_time = m_time;
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
            m_remain_time -= delta_time;
            if (m_remain_time <= FixPoint.Zero)
            {
                m_status = BTNodeStatus.True;
                m_remain_time = m_time;
            }
            else
            {
                m_status = BTNodeStatus.Running;
            }
        }

        protected override void OnActionExit()
        {
            m_remain_time = FixPoint.Zero;
        }
    }
}