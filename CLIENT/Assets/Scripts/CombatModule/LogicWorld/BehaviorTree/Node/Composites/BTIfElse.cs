using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTIfElse : BTComposite
    {
        //配置数据

        //运行数据
        int m_running_index = 0;

        public BTIfElse()
        {
        }

        public BTIfElse(BTIfElse prototype)
            : base(prototype)
        {
        }

        protected override void ResetRuntimeData()
        {
            m_running_index = 0;
        }

        public override void ClearRunningTrace()
        {
            m_running_index = 0;
        }

        public override BTNodeStatus OnUpdate(FixPoint delta_time)
        {
            m_status = BTNodeStatus.False;
            if (m_children == null || m_children.Count != 3)
                return m_status;
            if (m_running_index == 0)
            {
                m_status = m_children[m_running_index].OnUpdate(delta_time);
                if (m_status == BTNodeStatus.True)
                    m_running_index = 1;
                else if (m_status == BTNodeStatus.False)
                    m_running_index = 2;
                else
                    return m_status;
            }
            m_status = m_children[m_running_index].OnUpdate(delta_time);
            if (m_status != BTNodeStatus.Running)
                m_running_index = 0;
            return m_status;
        }
    }
}