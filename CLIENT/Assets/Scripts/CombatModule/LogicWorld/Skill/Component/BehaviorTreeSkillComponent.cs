using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BehaviorTreeSkillComponent : SkillComponent
    {
        //配置数据
        int m_bahavior_tree_id = 0;

        //运行数据
        BeahviorTree m_behavior_tree = null;

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            m_behavior_tree = BehaviorTreeFactory.Instance.CreateBehaviorTree(m_bahavior_tree_id);
        }

        protected override void OnDestruct()
        {
            if (m_behavior_tree != null)
                BehaviorTreeFactory.Instance.RecycleBehaviorTree(m_behavior_tree);
        }
        #endregion

        public override void Inflict(FixPoint start_time)
        {
            if (m_behavior_tree != null)
                m_behavior_tree.Activate(GetLogicWorld());
        }

        public override void Deactivate(bool force)
        {
            if (m_behavior_tree != null)
                m_behavior_tree.Deactivate();
        }
    }
}