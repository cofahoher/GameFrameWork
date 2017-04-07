using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class TurnManagerComponent : Component
    {
        //运行数据
        FixPoint m_current_turn_index = FixPoint.Zero;
        FixPoint m_current_turn_time = FixPoint.Zero;
        TaskScheduler<LogicWorld> m_turn_scheduler;

        public FixPoint CurrentTurnIndex
        {
            get { return m_current_turn_index; }            
        }

        public FixPoint CurrentTurnTime
        {
            get { return m_current_turn_index; }   
        }

        public TaskScheduler<LogicWorld> GetTaskScheduler()
        {
            return m_turn_scheduler;
        }

        public void OnTurnBegin()
        {
            m_current_turn_index += FixPoint.One;
            m_current_turn_time = m_current_turn_index * FixPoint.Ten;
            m_turn_scheduler.Update(m_current_turn_time);
        }

        public void OnTurnEnd()
        {
            m_current_turn_time += FixPoint.One;
            m_turn_scheduler.Update(m_current_turn_time);
        }
    }
}