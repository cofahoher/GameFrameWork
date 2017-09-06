using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BeahviorTree : BTNode, IRecyclable
    {
        //配置数据
        int m_config_id = 0;
        FixPoint m_update_interval = FixPoint.One;
        //运行数据
        bool m_active = false;

        #region 创建
        public BeahviorTree(int config_id, FixPoint update_interval)
        {
            m_config_id = config_id;
            m_update_interval = update_interval;
        }

        public BeahviorTree(BeahviorTree prototype)
            : base(prototype)
        {
            m_config_id = prototype.m_config_id;
            m_update_interval = prototype.m_update_interval;
        }

        public override void ResetNode()
        {
            base.ResetNode();
            m_active = false;
            if (m_context != null)
            {
                BTContext context = m_context;
                SetContext(null);
                RecyclableObject.Recycle(context);
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
        #endregion

        #region GETTER
        public int ConfigID
        {
            get { return m_config_id; }
        }

        public FixPoint UpdateInterval
        {
            get { return m_update_interval; }
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
            else
            {
            }
            m_active = true;
        }

        public void Deactivate()
        {
            if (!m_active)
                return;
            if (m_context != null)
            {
                m_context.GetActionBuffer().ExitAllAction();
            }
            m_active = false;
        }

        public override BTNodeStatus OnUpdate()
        {
            if (m_children == null)
                return BTNodeStatus.False;
            BTNodeStatus status = m_children[0].OnUpdate();
            m_context.GetActionBuffer().SwapActions();
            return status;
        }
    }
}