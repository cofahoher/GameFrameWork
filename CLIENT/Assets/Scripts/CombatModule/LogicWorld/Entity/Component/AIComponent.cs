using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class AIComponent : EntityComponent
    {
        public static readonly int BTEntry_AIMain = (int)CRC.Calculate("AIMain");

        //配置数据
        int m_bahavior_tree_id = 0;

        //运行数据
        BeahviorTree m_behavior_tree = null;

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            m_behavior_tree = BehaviorTreeFactory.Instance.CreateBehaviorTree(m_bahavior_tree_id);
            if (m_behavior_tree != null)
            {
                m_behavior_tree.Construct(GetLogicWorld());
                BTContext context = m_behavior_tree.Context;
                context.SetData<IExpressionVariableProvider>(BTContextKey.ExpressionVariableProvider, this);
                context.SetData<Entity>(BTContextKey.OwnerEntity, GetOwnerEntity());
                context.SetData<AIComponent>(BTContextKey.OwnerAIComponent, this);
            }
        }

        protected override void OnDestruct()
        {
            if (m_behavior_tree != null)
                BehaviorTreeFactory.Instance.RecycleBehaviorTree(m_behavior_tree);
            m_behavior_tree = null;
        }
        #endregion

        protected override void OnEnable()
        {
            if (m_behavior_tree == null)
                return;
            m_behavior_tree.Run(BTEntry_AIMain);
        }

        protected override void OnDisable()
        {
            if (m_behavior_tree == null)
                return;
            m_behavior_tree.ClearRunningTrace();
        }
    }
}