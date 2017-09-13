using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BTActionBuffer
    {
        List<BTAction> m_previous_actions = new List<BTAction>();
        List<BTAction> m_current_actions = new List<BTAction>();
        List<BTAction> m_backup_previous_action = new List<BTAction>();

        public void AddCurrentActions(BTAction action)
        {
            m_current_actions.Add(action);
        }

        public void SwapActions()
        {
            var enumerator = m_previous_actions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                BTAction action = enumerator.Current;
                if (!m_current_actions.Contains(action))
                    action.ExitAction();
            }
            m_previous_actions.Clear();
            List<BTAction> temp = m_previous_actions;
            m_previous_actions = m_current_actions;
            m_current_actions = temp;
        }

        public void Backup()
        {
            if (m_backup_previous_action.Count > 0)
                LogWrapper.LogError("BTActionBuffer::Backup(), m_backup_previous_action isn't empty");
            if (m_current_actions.Count > 0)
                LogWrapper.LogError("BTActionBuffer::Backup(), m_current_actions isn't empty");
            List<BTAction> temp = m_previous_actions;
            m_previous_actions = m_backup_previous_action;
            m_backup_previous_action = temp;
        }

        public void Restore()
        {
            if (m_previous_actions.Count > 0)
                LogWrapper.LogError("BTActionBuffer::Restore(), m_previous_actions isn't empty");
            if (m_current_actions.Count > 0)
                LogWrapper.LogError("BTActionBuffer::Restore(), m_current_actions isn't empty");
            List<BTAction> temp = m_backup_previous_action;
            m_backup_previous_action = m_previous_actions;
            m_previous_actions = temp;
        }

        public void ExitAllAction()
        {
            var enumerator = m_previous_actions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.ExitAction();
            }
            m_previous_actions.Clear();
            if (m_current_actions.Count > 0)
                LogWrapper.LogError("BTActionBuffer::ExitAllAction(), m_current_actions isn't empty");
            m_current_actions.Clear();
        }

        public void Clear()
        {
            m_previous_actions.Clear();
            m_current_actions.Clear();
            m_backup_previous_action.Clear();
        }
    }
}

