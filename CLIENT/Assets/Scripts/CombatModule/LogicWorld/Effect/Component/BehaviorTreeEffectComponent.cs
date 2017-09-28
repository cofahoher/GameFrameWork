using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BehaviorTreeEffectComponent : EffectComponent
    {
        public static readonly int BTEntry_Apply = (int)CRC.Calculate("Apply");
        public static readonly int BTEntry_Unapply = (int)CRC.Calculate("Unapply");

        //配置数据
        int m_bahavior_tree_id = 0;

        //运行数据
        BehaviorTree m_behavior_tree = null;

        public override void InitializeComponent()
        {
            m_behavior_tree = BehaviorTreeFactory.Instance.CreateBehaviorTree(m_bahavior_tree_id);
            if (m_behavior_tree != null)
            {
                m_behavior_tree.Construct(GetLogicWorld());
                BTContext context = m_behavior_tree.Context;
                context.SetData<IExpressionVariableProvider>(BTContextKey.ExpressionVariableProvider, this);
                context.SetData<Entity>(BTContextKey.OwnerEntity, GetOwnerEntity());
                context.SetData<EffectComponent>(BTContextKey.OwnerEffectComponent, this);
                m_behavior_tree.Active();
            }
        }

        protected override void OnDestruct()
        {
            if (m_behavior_tree != null)
                BehaviorTreeFactory.Instance.RecycleBehaviorTree(m_behavior_tree);
            m_behavior_tree = null;
        }

        public override void Apply()
        {
            if (m_behavior_tree == null)
                return;
            m_behavior_tree.Run(BTEntry_Apply);
        }

        public override void Unapply()
        {
            if (m_behavior_tree == null)
                return;
            m_behavior_tree.Run(BTEntry_Unapply);
        }
    }
}