using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum BTNodeStatus
    {
        Unreach,
        False,
        True,
        Running,
    }

    public abstract class BTNode : IExpressionVariableProvider
    {
        static public readonly FixPoint LOGIC_UPDATE_INTERVAL = Component.LOGIC_UPDATE_INTERVAL;

        //配置数据
        protected List<BTNode> m_children = null;
        //运行数据
        protected BTNodeStatus m_status = BTNodeStatus.Unreach;
        //非节点数据
        protected BTContext m_context;

        #region 创建
        public BTNode()
        {
        }

        public BTNode(BTNode prototype)
        {
            if (prototype.m_children != null)
            {
                for (int i = 0; i < prototype.m_children.Count; ++i)
                {
                    BTNode clone = prototype.m_children[i].Clone();
                    AddChild(clone);
                }
            }
        }
        public virtual void InitializeVariable(Dictionary<string, string> variables)
        {
        }

        public BTNode Clone()
        {
            System.Type t = this.GetType();
            BTNode clone = System.Activator.CreateInstance(t, this) as BTNode;
            return clone;
        }

        public virtual void ResetNode()
        {
            m_status = BTNodeStatus.Unreach;
            if (m_children != null)
            {
                for (int i = 0; i < m_children.Count; ++i)
                    m_children[i].ResetNode();
            }
        }

        public virtual void ClearRunningTrace()
        {
            if (m_children != null)
            {
                for (int i = 0; i < m_children.Count; ++i)
                    m_children[i].ClearRunningTrace();
            }
        }

        public void SetContext(BTContext context)
        {
            m_context = context;
            if (m_children != null)
            {
                for (int i = 0; i < m_children.Count; ++i)
                    m_children[i].SetContext(context);
            }
        }
        #endregion

        #region Getter
        public BTNodeStatus CurrentStatus
        {
            get { return m_status; }
        }

        public BTContext Context
        {
            get { return m_context; }
        }

        public List<BTNode> GetChildren()
        {
            return m_children;
        }

        public LogicWorld GetLogicWorld()
        {
            if (m_context != null)
                return m_context.GetLogicWorld();
            else
                return null;
        }

        public AIComponent GetAIComponent()
        {
            if (m_context == null)
                return null;
            return m_context.GetData<AIComponent>(BTContextKey.OwnerAIComponent);
        }

        public SkillComponent GetSkillComponent()
        {
            if (m_context == null)
                return null;
            return m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
        }

        public EffectComponent GetEffectComponent()
        {
            if (m_context == null)
                return null;
            return m_context.GetData<EffectComponent>(BTContextKey.OwnerEffectComponent);
        }

        public Skill GetOwnerSkill()
        {
            if (m_context == null)
                return null;
            SkillComponent skill_component = m_context.GetData<SkillComponent>(BTContextKey.OwnerSkillComponent);
            if (skill_component == null)
                return null;
            return skill_component.GetOwnerSkill();
        }
        #endregion

        public void AddChild(BTNode node)
        {
            if (node == null)
                return;
            if (m_children == null)
                m_children = new List<BTNode>();
            m_children.Add(node);
        }

        public abstract BTNodeStatus OnUpdate(FixPoint delta_time);

        public virtual void ShutDown()
        {
            if (m_children != null)
            {
                for (int i = 0; i < m_children.Count; ++i)
                    m_children[i].ShutDown();
            }
        }

        #region Variable
        public FixPoint GetVariable(ExpressionVariable variable, int index)
        {
            if (m_context == null)
                return FixPoint.Zero;
            IExpressionVariableProvider provider = m_context.GetData<IExpressionVariableProvider>(BTContextKey.ExpressionVariableProvider);
            if (provider == null)
                return FixPoint.Zero;
            return provider.GetVariable(variable, index);
        }
        #endregion
    }
}