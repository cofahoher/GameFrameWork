using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTParallelSelector : BTComposite
    {
        //运行数据
        int m_index = 0;

        public BTParallelSelector()
        {
            ResetRuntimeData();
        }

        public BTParallelSelector(BTParallelSelector prototype)
            : base(prototype)
        {
            ResetRuntimeData();
        }

        protected override void ResetRuntimeData()
        {
            m_index = 0;
        }

        public override void ClearRunningTrace()
        {
            m_index = 0;
            base.ClearRunningTrace();
        }

        public override BTNodeStatus OnUpdate(FixPoint delta_time)
        {
            m_status = BTNodeStatus.False;
            if (m_children == null)
                return m_status;
            for (; m_index < m_children.Count; ++m_index)
            {
                BTNodeStatus status = m_children[m_index].OnUpdate(delta_time);
                if (status == BTNodeStatus.Running)
                    return status;
                else if (status == BTNodeStatus.True)
                    m_status = status;
            }
            if (m_index == m_children.Count)
                m_index = 0;
            return m_status;
        }
    }
}