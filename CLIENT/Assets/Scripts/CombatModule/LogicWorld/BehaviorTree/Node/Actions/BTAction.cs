using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public abstract class BTAction : BTNode
    {
        //运行数据
        private BTNodeStatus m_status_backup = BTNodeStatus.True;
        bool m_is_running = false;

        public BTAction()
        {
            m_status_backup = BTNodeStatus.True;
            m_is_running = false;
        }

        public BTAction(BTAction prototype)
            : base(prototype)
        {
            m_status_backup = BTNodeStatus.True;
            m_is_running = false;
        }

        public override void ResetNode()
        {
            base.ResetNode();
            m_status_backup = BTNodeStatus.True;
            m_is_running = false;
            ResetRuntimeData();
        }

        protected abstract void ResetRuntimeData();

        public override BTNodeStatus OnUpdate(FixPoint delta_time)
        {
            m_status = m_status_backup;
            BTActionBuffer action_buffer = m_context.GetActionBuffer();
            action_buffer.AddCurrentActions(this);
            if (m_is_running)
                UpdateAction(delta_time);
            else
                EnterAction();
            m_status_backup = m_status;
            return m_status;
        }

        void EnterAction()
        {
            m_status = BTNodeStatus.True;
            m_is_running = true;
            //如果要实现为状态机，那就override OnActionEnter修改m_status为BTNodeStatus.Running
            OnActionEnter();
            OnActionUpdate(FixPoint.Zero);
        }

        void UpdateAction(FixPoint delta_time)
        {
            OnActionUpdate(delta_time);
        }

        public void ExitAction()
        {
            m_status = BTNodeStatus.True;
            m_is_running = false;
            OnActionExit();
        }

        protected abstract void OnActionEnter();
        protected abstract void OnActionUpdate(FixPoint delta_time);
        protected abstract void OnActionExit();
    }
}