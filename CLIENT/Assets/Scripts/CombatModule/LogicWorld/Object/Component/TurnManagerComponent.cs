using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class TurnManagerComponent
    {
        int m_current_turn_index = 0;
        int m_current_turn_time = 0;
        TaskScheduler m_turn_scheduler;

        public int CurrentTurnIndex
        {
            get { return m_current_turn_index; }            
        }

        public int CurrentTurnTime
        {
            get { return m_current_turn_index; }   
        }

        public TaskScheduler GetTaskScheduler()
        {
            return m_turn_scheduler;
        }

        public void OnTurnBegin()
        {
            ++m_current_turn_index;
            m_current_turn_time = m_current_turn_index * 10;
            m_turn_scheduler.Update(m_current_turn_time);
        }

        public void OnTurnEnd()
        {
            m_current_turn_time += 1;
            m_turn_scheduler.Update(m_current_turn_time);
        }
    }
}