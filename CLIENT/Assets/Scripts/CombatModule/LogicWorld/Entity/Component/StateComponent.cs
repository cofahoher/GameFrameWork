using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class StateComponent : EntityComponent
    {
        SortedDictionary<int, List<int>> m_states = new SortedDictionary<int, List<int>>();

        public bool AddState(int state, int effect_id)
        {
            StateData data = StateSystem.GetState(state);
            if (data == null)
                return false;
            if (!CanAddState(data))
                return false;
            List<int> list;
            if (!m_states.TryGetValue(state, out list))
            {
                list = new List<int>();
                m_states[state] = list;
            }
            list.Add(effect_id);
            if (list.Count == 1)
                ActivateState(data);
            return true;
        }

        public bool RemoveState(int state, int effect_id)
        {
            StateData data = StateSystem.GetState(state);
            if (data == null)
                return false;
            List<int> list;
            if (!m_states.TryGetValue(state, out list))
                return false;
            if (!list.Remove(effect_id))
                return false;
            if (list.Count == 0)
            {
                //m_states.Remove(state);
                DeactivateState(data);
            }
            return true;
        }

        public bool HasState(int state)
        {
            List<int> list;
            m_states.TryGetValue(state, out list);
            return list != null && list.Count > 0;
        }

        public bool HasState(string state_name)
        {
            int state = StateSystem.StateName2ID(state_name);
            return HasState(state);
        }

        bool CanAddState(StateData data)
        {
            return true;
        }

        void ActivateState(StateData data)
        {
            for (int i = 0; i < data.m_disable_componnets.Count; ++i)
            {
                Component component = ParentObject.GetComponent(data.m_disable_componnets[i]);
                if (component != null)
                    component.Disable();
            }
        }

        void DeactivateState(StateData data)
        {
            for (int i = 0; i < data.m_disable_componnets.Count; ++i)
            {
                Component component = ParentObject.GetComponent(data.m_disable_componnets[i]);
                if (component != null)
                    component.Enable();
            }
        }
    }
}