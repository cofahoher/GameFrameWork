using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EffectGeneratorSkillComponent : SkillComponent
    {
        #region 配置数据
        #endregion

        #region 运行数据
        EffectGenerator m_generator;
        #endregion

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {

        }

        protected override void OnDestruct()
        {
            RecyclableObject.Recycle(m_generator);
            m_generator = null;
        }
        #endregion

        #region 技能的Activate流程
        public override void PostActivate(FixPoint start_time)
        {
        }
        #endregion
    }
}
