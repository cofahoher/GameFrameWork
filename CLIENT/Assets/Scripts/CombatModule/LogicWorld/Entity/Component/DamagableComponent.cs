using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class DamagableComponent : EntityComponent
    {
        //运行数据
        FixPoint m_current_max_health = FixPoint.Zero;
        FixPoint m_current_health = FixPoint.MinusOne;
        Formula m_test_formula = Formula.Create();

        #region 初始化
        protected override void OnDestruct()
        {
            Formula.Recycle(m_test_formula);
        }

        public override void InitializeComponent()
        {
            if (m_current_health < FixPoint.Zero)
                m_current_health = m_current_max_health;
        }
        #endregion
    }
}