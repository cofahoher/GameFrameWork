using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BeahviorTree : BTNode, IRecyclable
    {
        //配置数据
        int m_config_id = 0;
        List<BehaviorTreeEntryNodeExtraData> m_entries = new List<BehaviorTreeEntryNodeExtraData>();
        //运行数据
        bool m_active = false;
        int m_current_running_entry_index = -1;
        BeahviorTreeTask m_task = null;

        #region 创建
        public BeahviorTree(int config_id)
        {
            m_config_id = config_id;
        }

        public BeahviorTree(BeahviorTree prototype)
            : base(prototype)
        {
            m_config_id = prototype.m_config_id;
            for (int i = 0; i > prototype.m_entries.Count; ++i)
                m_entries.Add(prototype.m_entries[i]);
        }

        public override void ResetNode()
        {
            Deactivate();
            base.ResetNode();
            if (m_context != null)
            {
                BTContext context = m_context;
                SetContext(null);
                RecyclableObject.Recycle(context);
            }
            m_active = false;
            m_current_running_entry_index = -1;
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
        }

        public void Reset()
        {
            ResetNode();
        }

        public BeahviorTree CloneBehaviorTree()
        {
            BeahviorTree clone = Clone() as BeahviorTree;
            return clone;
        }

        public void AddEntry(BTNode node, BehaviorTreeEntryNodeExtraData entry_data)
        {
            m_entries.Add(entry_data);
            AddChild(node);
        }
        #endregion

        #region GETTER
        public int ConfigID
        {
            get { return m_config_id; }
        }

        public FixPoint UpdateInterval
        {
            get
            {
                if (m_current_running_entry_index < 0)
                    return FixPoint.MinusOne;
                return m_entries[m_current_running_entry_index].m_update_interval;
            }
        }
        #endregion

        public void Activate(LogicWorld logic_world)
        {
            if (m_active)
                return;
            if (m_context == null)
            {
                BTContext context = RecyclableObject.Create<BTContext>();
                context.SetLogicWorld(logic_world);
                SetContext(context);
            }
            if (m_task == null)
            {
                m_task = LogicTask.Create<BeahviorTreeTask>();
                m_task.Construct(this);
            }
            m_active = true;
        }

        public void Deactivate()
        {
            if (!m_active)
                return;
            if (m_context != null)
                m_context.GetActionBuffer().ExitAllAction();
            if (m_task != null)
                m_task.Cancel();
            if (m_current_running_entry_index >= 0)
                m_children[m_current_running_entry_index].ClearRunningTrace();
            m_current_running_entry_index = -1;
            m_active = false;
        }

        public bool HasEntry(int entrty_id)
        {
            int new_index = EntryID2Index(entrty_id);
            if (new_index == -1)
                return false;
            return true;
        }

        public bool Run(int entrty_id = 0)
        {
            if (!m_active)
                return false;
            int new_index = EntryID2Index(entrty_id);
            if (new_index == -1)
                return false;
            if (new_index == m_current_running_entry_index)
                return true;
            m_context.GetActionBuffer().ExitAllAction();
            if (m_current_running_entry_index >= 0)
                m_children[m_current_running_entry_index].ClearRunningTrace();
            m_current_running_entry_index = new_index;
            m_task.Cancel();
            FixPoint update_interval = UpdateInterval;
            if (update_interval > FixPoint.Zero)
            {
                LogicWorld logic_world = m_context.GetLogicWorld();
                var schedeler = logic_world.GetTaskScheduler();
                schedeler.Schedule(m_task, logic_world.GetCurrentTime(), update_interval, update_interval);
            }
            OnUpdate();
            return true;
        }

        public BTNodeStatus RunOnce(int entrty_id)
        {
            if (!m_active)
                return BTNodeStatus.False;
            int new_index = EntryID2Index(entrty_id);
            if (new_index == -1)
                return BTNodeStatus.True;
            BTActionBuffer action_buffer = m_context.GetActionBuffer();
            //ZZWTODO m_context的数据
            action_buffer.Backup();
            BTNodeStatus result = m_children[new_index].OnUpdate();
            action_buffer.SwapActions();
            action_buffer.ExitAllAction();
            action_buffer.Restore();
            m_children[new_index].ClearRunningTrace();
            return result;
        }

        int EntryID2Index(int entrty_id)
        {
            for (int i = 0; i < m_entries.Count; ++i)
            {
                if (m_entries[i].m_entry_name_id == entrty_id)
                    return i;
            }
            return -1;
        }

        public override BTNodeStatus OnUpdate()
        {
            if (m_current_running_entry_index < 0)
                return BTNodeStatus.False;
            BTNodeStatus status = m_children[m_current_running_entry_index].OnUpdate();
            m_context.GetActionBuffer().SwapActions();
            return status;
        }
    }

    public class BeahviorTreeTask : Task<LogicWorld>
    {
        BeahviorTree m_behavior_tree;

        public void Construct(BeahviorTree behavior_tree)
        {
            m_behavior_tree = behavior_tree;
        }

        public override void OnReset()
        {
            m_behavior_tree = null;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            m_behavior_tree.OnUpdate();
        }
    }
}