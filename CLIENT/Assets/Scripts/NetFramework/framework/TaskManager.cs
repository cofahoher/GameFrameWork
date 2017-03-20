using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TaskResult
{
    public object result;
}

#if !CONSOLE_CLIENT

public class Task
{
    public delegate void TaskDelegate(int task_id);


    public enum TaskState
    {
        Running,
        Suspend,
        Stop,
    }

    //IEnumerator m_coroutine;
    Stack<IEnumerator> m_stack = new Stack<IEnumerator>();
    List<int> m_join_task_list;

    TaskDelegate m_finish_callback;
    public Task.TaskDelegate FinishCallback
    {
        get { return m_finish_callback; }
        set { m_finish_callback = value; }
    }
    
    int m_id;
    public int Id
    {
        get { return m_id; }
    }

    TaskState m_state;
    public Task.TaskState State
    {
        get { return m_state; }
    }

    object m_data;
    public object Data
    {
        get { return m_data; }
        set { m_data = value; }
    }

    public Task(int id, IEnumerator coroutine)
    {
        m_id = id;
        m_stack.Push(coroutine);

        TaskManager.Instance.StartCoroutine(Update());
    }
    
    public IEnumerator Suspend()
    {
        if (m_state == TaskState.Running)
        {
            m_state = TaskState.Suspend;
            yield return null;
        }
        yield break;
    }

    public void Resume()
    {
        if (m_state == TaskState.Suspend)
        {
            m_state = TaskState.Running;
        }
    }

    void Stop()
    {
        m_state = TaskState.Stop;
    }

    public IEnumerator Join(int task_id)
    {
        Task task = TaskManager.Instance.FindTask(task_id);
        if (task != null)
        {
            if (task.m_join_task_list == null)
            {
                task.m_join_task_list = new List<int>();
            }
            task.m_join_task_list.Add(m_id);
            yield return Suspend();
        }
        yield break;
    }

    IEnumerator Update()
    {
        while(State != TaskState.Stop)
        {
            if (State == TaskState.Suspend)
            {
                yield return null;
            }
            else
            {
                IEnumerator e = m_stack.Peek();
                Task last_task = TaskManager.Instance.CurrTask;
                TaskManager.Instance.CurrTask = this;
                bool move_next = e.MoveNext();
                TaskManager.Instance.CurrTask = last_task;
                if (move_next)
                {
                    if (e.Current is IEnumerator)
                    {
                        m_stack.Push(e.Current as IEnumerator);
                        continue;
                    }
                    yield return e.Current;
                }
                else
                {
                    m_stack.Pop();
                    if (m_stack.Count == 0)
                    {
                        Stop();
                    }
                }
            }
        }
        if (m_finish_callback != null)
        {
            m_finish_callback(m_id);
        }
        TaskManager.Instance.RemoveTask(m_id);
        if (m_join_task_list != null)
        {
            foreach (int task_id in m_join_task_list)
            {
                TaskManager.Instance.ResumeTask(task_id);
            }
        }
    }
}

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance
    {
        get { return GameGlobal.TaskManager; }
    }

    void Awake()
    {
        Init();
    }


    Dictionary<int, Task> m_tasks = new Dictionary<int, Task>();
    int m_seq = 0;
    Task m_curr_task;
    public Task CurrTask
    {
        get { return m_curr_task; }
        set { m_curr_task = value; }
    }

    public int TaskCount
    {
        get { return m_tasks.Count; }
    }

    public void Init()
    {
    }

    public Task FindTask(int task_id)
    {
        Task task;
        m_tasks.TryGetValue(task_id, out task);
        return task;
    }

    public int StartTask(IEnumerator coroutine)
    {
        if(m_seq < 0)
        {
            m_seq = 0;
        }
        int id = m_seq++;
        Task task = new Task(id, coroutine);
        if (task.State != Task.TaskState.Stop)
        {
            m_tasks[id] = task;
            return id;
        }
        return -1;
    }

    public void RemoveTask(int task_id)
    {
        m_tasks.Remove(task_id);
    }

    //public void StopTask(int task_id)
    //{
    //    Task task = FindTask(task_id);
    //    if (task != null)
    //    {
    //        task.Stop();
    //    }
    //}

    public void SuspendTask(int task_id)
    {
        Task task = FindTask(task_id);
        if (task != null)
        {
            task.Suspend();
        }
    }

    public void ResumeTask(int task_id)
    {
        Task task = FindTask(task_id);
        if (task != null)
        {
            task.Resume();
        }
    }

}

#else

public class Task
{
    public delegate void TaskDelegate(int task_id);

    public enum TaskState
    {
        Running,
        Suspend,
        Stop,
    }

    //IEnumerator m_coroutine;
    Stack<IEnumerator> m_stack = new Stack<IEnumerator>();
    List<int> m_join_task_list;

    int m_id;
    public int Id
    {
        get { return m_id; }
    }

    TaskState m_state;
    public Task.TaskState State
    {
        get { return m_state; }
    }

    object m_data;
    public object Data
    {
        get { return m_data; }
        set { m_data = value; }
    }

    TaskDelegate m_finish_callback;
    public Task.TaskDelegate FinishCallback
    {
        get { return m_finish_callback; }
        set { m_finish_callback = value; }
    }

    public string Name { get; private set; }
    public Task(int id, IEnumerator coroutine)
    {
        m_id = id;
        Name = coroutine.GetType().Name;
        m_stack.Push(coroutine);
    }

    public int Suspend()
    {
        if (m_state == TaskState.Running)
        {
            m_state = TaskState.Suspend;
        }
        return 0;
    }

    public void Resume()
    {
        if (m_state == TaskState.Suspend)
        {
            m_state = TaskState.Running;
        }
    }

    public void Stop()
    {
        m_state = TaskState.Stop;
    }

    public IEnumerator Join(int task_id)
    {
        Task task = TaskManager.Instance.FindTask(task_id);
        if (task != null)
        {
            if (task.m_join_task_list == null)
            {
                task.m_join_task_list = new List<int>();
            }
            task.m_join_task_list.Add(m_id);
            yield return Suspend();
        }
        yield break;
    }

    public override string ToString()
    {
        return Name + State;
    }
    public void Update()
    {
        while (State != TaskState.Stop)
        {
            if (State == TaskState.Suspend)
            {
                return;
            }
            else
            {
                Task last_task = TaskManager.Instance.CurrTask;
                IEnumerator e = m_stack.Peek();
                TaskManager.Instance.CurrTask = this;
                bool move_next = e.MoveNext();
                TaskManager.Instance.CurrTask = last_task;
                if (move_next)
                {
                    if (e.Current is IEnumerator)
                    {
                        m_stack.Push(e.Current as IEnumerator);
                        continue;
                    }
                    return;
                }
                else
                {
                    m_stack.Pop();
                    if (m_stack.Count == 0)
                    {
                        Stop();
                    }
                }
            }
        }
        if (m_finish_callback != null)
        {
            m_finish_callback(m_id);
        }
        TaskManager.Instance.RemoveTask(m_id);
        if (m_join_task_list != null)
        {
            foreach (int task_id in m_join_task_list)
            {
                TaskManager.Instance.ResumeTask(task_id);
            }
        }
    }
}

public class TaskManager
{
    public static TaskManager Instance
    {
        get { return GameGlobal.TaskManager; }
    }


    public void Init()
    {
    }

    public void Update()
    {
        int i = m_tasks.Count - 1;
        if (i < 0) return;
        m_tasks[i].Value.Update();
    }

    List<KeyValuePair<int, Task>> m_tasks = new List<KeyValuePair<int, Task>>();
    int m_seq = 0;
    Task m_curr_task;
    public Task CurrTask
    {
        get { return m_curr_task; }
        set { m_curr_task = value; }
    }

    public int TaskCount
    {
        get { return m_tasks.Count; }
    }

    public Task FindTask(int task_id)
    {
        for (int i = 0; i < m_tasks.Count; ++i)
        {
            if (m_tasks[i].Key == task_id)
            {
                return m_tasks[i].Value;
            }
        }
        return null;
    }

    public int StartTask(IEnumerator coroutine)
    {
        int id = m_seq++;
        var pair = new KeyValuePair<int, Task>(id, new Task(id, coroutine));
        m_tasks.Add(pair);
        pair.Value.Update();
        return id;
    }

    public void RemoveTask(int task_id)
    {
        for (int i = 0; i < m_tasks.Count; ++i)
        {
            if (m_tasks[i].Key == task_id)
            {
                m_tasks.RemoveAt(i);
                break;
            }
        }
    }

    public void StopTask(int task_id)
    {
        Task task = FindTask(task_id);
        if (task != null)
        {
            task.Stop();
        }
    }

    public void SuspendTask(int task_id)
    {
        Task task = FindTask(task_id);
        if (task != null)
        {
            task.Suspend();
        }
    }

    public void ResumeTask(int task_id)
    {
        Task task = FindTask(task_id);
        if (task != null)
        {
            task.Resume();
        }
    }
	public void Clear()
	{
		m_tasks.Clear();
	}
}
#endif
