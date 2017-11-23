using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTFor : BTComposite
    {
        //配置数据
        int m_n = 1;
        int m_variable_id = -1;

        //运行数据
        int m_i = 0;

        public BTFor()
        {
        }

        public BTFor(BTFor prototype)
            : base(prototype)
        {
            m_n = prototype.m_n;
            m_variable_id = prototype.m_variable_id;
        }

        protected override void ResetRuntimeData()
        {
            m_i = 0;
        }

        public override void ClearRunningTrace()
        {
            m_i = 0;
        }

        public override BTNodeStatus OnUpdate(FixPoint delta_time)
        {
            if (m_children == null)
            {
                m_status = BTNodeStatus.False;
                return m_status;
            }
            m_status = BTNodeStatus.Running;
            if (m_variable_id != -1)
                m_context.SetData(m_variable_id, (FixPoint)m_i);
            BTNodeStatus child_status = m_children[0].OnUpdate(delta_time);
            if (child_status != BTNodeStatus.Running)
            {
                ++m_i;
                if (m_i >= m_n)
                {
                    m_status = BTNodeStatus.True;
                    m_i = 0;
                }
            }
            return m_status;
        }
    }
}