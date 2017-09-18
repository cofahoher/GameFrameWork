using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTPulse : BTDecorator
    {
        //配置数据
        FixPoint m_interval = FixPoint.One;
        //运行数据
        FixPoint m_next_execute_time = FixPoint.Zero;

        public BTPulse()
        {
        }

        public BTPulse(BTPulse prototype)
            : base(prototype)
        {
            m_interval = prototype.m_interval;
        }

        protected override void ResetRuntimeData()
        {
            m_next_execute_time = FixPoint.Zero;
        }

        protected override bool CanExecute()
        {
            if (m_next_execute_time == FixPoint.Zero)
                m_next_execute_time = m_context.GetLogicWorld().GetCurrentTime() + m_interval;
            return m_context.GetLogicWorld().GetCurrentTime() >= m_next_execute_time;
        }

        protected override void PrepareForNextExecute()
        {
            m_next_execute_time += m_interval;
        }
    }
}