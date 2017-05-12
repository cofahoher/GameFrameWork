using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class StateSystem : Singleton<StateSystem>
    {
        static Dictionary<int, StateData> m_states = new Dictionary<int, StateData>();
        static Dictionary<string, int> m_name2id = new Dictionary<string, int>();
        public static readonly int DEAD_STATE = (int)CRC.Calculate("Dead");

        private StateSystem()
        {
        }

        public override void Destruct()
        {
        }

        public static void RegisterState(StateData data)
        {
            m_states[data.m_state_id] = data;
            m_name2id[data.m_state_name] = data.m_state_id;
        }

        public static StateData GetState(int state_id)
        {
            StateData data;
            if (!m_states.TryGetValue(state_id, out data))
                return null;
            else
                return data;
        }

        public static int StateName2ID(string state_name)
        {
            int state_id;
            if (!m_name2id.TryGetValue(state_name, out state_id))
                return 0;
            else
                return state_id;
        }
    }
}