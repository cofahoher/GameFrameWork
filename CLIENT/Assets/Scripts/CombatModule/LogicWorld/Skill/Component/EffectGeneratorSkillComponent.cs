using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EffectGeneratorSkillComponent : SkillComponent
    {
        //配置数据
        int m_generator_cfgid = 0;

        //运行数据
        EffectGenerator m_generator;

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

        public override void PostActivate(FixPoint start_time)
        {
        }
    }
}
