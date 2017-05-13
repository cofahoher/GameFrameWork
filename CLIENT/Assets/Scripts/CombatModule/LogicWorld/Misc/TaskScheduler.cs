using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class TaskScheduler<TContext> : IDestruct
    {
        TContext m_context;
        Heap<Task<TContext>> m_queue = new Heap<Task<TContext>>(Heap<Task<TContext>>.CheckPriorityMethod.CPM_LESS);

        public TaskScheduler(TContext context)
        {
            m_context = context;
        }

        public void Destruct()
        {
            m_context = default(TContext);
            m_queue.Clear();
        }

        public void Schedule(Task<TContext> task, FixPoint current_time, FixPoint delay = default(FixPoint), FixPoint period = default(FixPoint))
        {
            task.SetTaskScheduler(this);
            task.ScheduleTime = current_time;
            task.Period = period;
            task.NextExecutionTime = current_time + delay;
            m_queue.Enqueue(task);
        }

        public void Cancel(Task<TContext> task)
        {
            m_queue.Remove(task);
        }

        public void Update(FixPoint current_time)
        {
            while (true)
            {
                Task<TContext> task = m_queue.Peek();
                if (task == null || task.NextExecutionTime > current_time)
                    break;
                FixPoint delta_time;
                if (task.Period.RawValue > 0)
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

    public abstract class Task<TContext> : HeapItem, IRecyclable
    {
        protected TaskScheduler<TContext> m_scheduler;
        protected FixPoint m_schedule_time = FixPoint.Zero;
        protected FixPoint m_period = FixPoint.Zero;
        protected FixPoint m_next_execution_time = FixPoint.Zero;

        public FixPoint ScheduleTime
        {
            get { return m_schedule_time; }
            set { m_schedule_time = value; }
        }
        public FixPoint Period
        {
            get { return m_period; }
            set { m_period = value; }
        }
        public FixPoint NextExecutionTime
        {
            get { return m_next_execution_time; }
            set { m_next_execution_time = value; }
        }

        public TaskScheduler<TContext> GetTaskScheduler()
        {
            return m_scheduler;
        }
        public void SetTaskScheduler(TaskScheduler<TContext> scheduler)
        {
            m_scheduler = scheduler;
        }

        public void Reset()
        {
            OnReset();

            if (m_scheduler != null && _heap_index >= 0)
                m_scheduler.Cancel(this);
            m_scheduler = null;

            m_schedule_time = FixPoint.Zero;
            m_period = FixPoint.Zero;
            m_next_execution_time = FixPoint.Zero;

            _heap_index = -1;
            _insertion_index = -1;
        }

        public abstract void OnReset();

        public virtual void Schedule(FixPoint current_time, FixPoint delay = default(FixPoint), FixPoint period = default(FixPoint))
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

        public abstract void Run(TContext context, FixPoint current_time, FixPoint delta_time);

        public override int CompareTo(object obj)
        {
            Task<TContext> item = obj as Task<TContext>;
            if (item == null)
                return -1;
            int result = m_next_execution_time.CompareTo(item.m_next_execution_time);
            if (result != 0)
                return result;
            else
                return _insertion_index - item._insertion_index;
        }
    }
}