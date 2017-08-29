﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BTSequence : BTComposite
    {
        //运行数据
        int m_index = 0;

        public BTSequence()
        {
            ResetRuntimeData();
        }

        public BTSequence(BTSequence prototype)
            : base(prototype)
        {
            ResetRuntimeData();
        }

        protected override void ResetRuntimeData()
        {
            m_index = 0;
        }

        public override BTNodeStatus OnUpdate()
        {
            m_status = BTNodeStatus.True;
            if (m_children == null)
                return m_status;
            for (; m_index < m_children.Count; ++m_index)
            {
                m_status = m_children[m_index].OnUpdate();
                if (m_status == BTNodeStatus.Running)
                {
                    break;
                }
                else if (m_status == BTNodeStatus.False)
                {
                    m_index = 0;
                    break;
                }
            }
            if (m_status == BTNodeStatus.True)
            {
                m_index = 0;
            }
            return m_status;
        }
    }
}