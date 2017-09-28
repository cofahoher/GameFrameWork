using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BehaviorTreeSkillComponent : SkillComponent
    {
        public static readonly int BTEntry_CanActivate = (int)CRC.Calculate("CanActivate");
        public static readonly int BTEntry_Activate = (int)CRC.Calculate("Activate");
        public static readonly int BTEntry_PostActivate = (int)CRC.Calculate("PostActivate");
        public static readonly int BTEntry_Inflict = (int)CRC.Calculate("Inflict");
        public static readonly int BTEntry_Deactivate = (int)CRC.Calculate("Deactivate");

        //配置数据
        int m_bahavior_tree_id = 0;

        //运行数据
        BehaviorTree m_behavior_tree = null;

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
                context.SetData<SkillComponent>(BTContextKey.OwnerSkillComponent, this);
                m_behavior_tree.Active();
            }
        }

        protected override void OnDestruct()
        {
            if (m_behavior_tree != null)
                BehaviorTreeFactory.Instance.RecycleBehaviorTree(m_behavior_tree);
            m_behavior_tree = null;
        }
        #endregion

        public override bool CanActivate()
        {
            if (m_behavior_tree == null)
                return false;
            if (!m_behavior_tree.HasEntry(BTEntry_CanActivate))
                return true;
            BTNodeStatus bt_status = m_behavior_tree.RunOnce(BTEntry_CanActivate);
            return bt_status == BTNodeStatus.True;
        }

        public override void Activate(FixPoint start_time)
        {
            if (m_behavior_tree == null)
                return;
            m_behavior_tree.Run(BTEntry_Activate);
        }

        public override void PostActivate(FixPoint start_time)
        {
            if (m_behavior_tree == null)
                return;
            m_behavior_tree.Run(BTEntry_PostActivate);
        }

        public override void Inflict(FixPoint start_time)
        {
            if (m_behavior_tree == null)
                return;
            m_behavior_tree.Run(BTEntry_Inflict);
        }

        public override void Deactivate(bool force)
        {
            if (m_behavior_tree == null)
                return;
            m_behavior_tree.Run(BTEntry_Deactivate);
            m_behavior_tree.ClearRunningTrace();
        }
    }
}