using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BTActionBuffer
    {
        List<BTAction> m_previous_actions = new List<BTAction>();
        List<BTAction> m_current_actions = new List<BTAction>();

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

        public void ExitAllAction()
        {
            var enumerator = m_previous_actions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.ExitAction();
            }
            m_previous_actions.Clear();
        }
    }
}

