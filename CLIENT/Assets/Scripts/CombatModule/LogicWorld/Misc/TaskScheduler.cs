using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class TaskScheduler : IDestruct
    {
        System.Object m_context;
        Heap<Task> m_queue = new Heap<Task>(Heap<Task>.CheckPriorityMethod.CPM_LESS);

        public TaskScheduler(System.Object context)
        {
            m_context = context;
        }

        public void Destruct()
        {
            m_context = null;
            m_queue.Clear();
        }

        public void Schedule(Task task, int current_time, int delay = 0, int period = -1)
        {
            task.ScheduleTime = current_time;
            task.Period = period;
            task.NextExecutionTime = current_time + delay;
            m_queue.Enqueue(task);
        }

        public void Cancel(Task task)
        {
            m_queue.Remove(task);
        }

        public void Update(int current_time)
        {
            while (true)
            {
                Task task = m_queue.Peek();
                if (task == null || task.NextExecutionTime > current_time)
                    break;
                int delta_time = 0;
                if (task.Period > 0)
                {
                    delta_time = task.NextExecutionTime - task.ScheduleTime;
                    task.ScheduleTime = task.NextExecutionTime;
                    task.NextExecutionTime = task.NextExecutionTime + task.Period;
                    m_queue.UpdatePriorityByIndex(0);
                }
                else
                {
                    delta_time = current_time - task.ScheduleTime;
                    m_queue.Dequeue();
                }
                task.Run(m_context, current_time, delta_time);
            }
        }
    }
    
    public class Task : HeapItem, IRecyclable
    {
        protected TaskScheduler m_scheduler;
        protected int m_schedule_time = -1;
        protected int m_period = -1;
        protected int m_next_execution_time = -1;

        public int ScheduleTime
        {
            get { return m_schedule_time; }
            set { m_schedule_time = value; }
        }
        public int Period
        {
            get { return m_period; }
            set { m_period = value; }
        }
        public int NextExecutionTime
        {
            get { return m_next_execution_time; }
            set { m_next_execution_time = value; }
        }

        public TaskScheduler GetTaskScheduler()
        {
            return m_scheduler;
        }
        public void SetTaskScheduler(TaskScheduler scheduler)
        {
            m_scheduler = scheduler;
        }

        public void Destruct()
        {
            if (m_scheduler != null && _heap_index >= 0)
                m_scheduler.Cancel(this);
            m_scheduler = null;
        }

        public virtual void Reset()
        {
            Destruct();

            _heap_index = -1;
            _insertion_index = -1;

            m_scheduler = null;
            m_schedule_time = -1;
            m_period = -1;
            m_next_execution_time = -1;
        }

        public virtual void Schedule(int current_time, int delay = 0, int period = -1)
        {
            if (m_scheduler != null)
                m_scheduler.Schedule(this, current_time, delay, period);
        }

        public void Cancel()
        {
            if (m_scheduler != null)
                m_scheduler.Cancel(this);
        }

        public bool IsInSchedule()
        {
            return IsInHeap();
        }

        public virtual void Run(System.Object context, int current_time, int delta_time)
        {
        }

        public override int CompareTo(object obj)
        {
            Task item = obj as Task;
            if (item == null)
                return -1;
            int result = m_next_execution_time - item.m_next_execution_time;
            if (result != 0)
                return result;
            else
                return _insertion_index - item._insertion_index;
        }
    }
}