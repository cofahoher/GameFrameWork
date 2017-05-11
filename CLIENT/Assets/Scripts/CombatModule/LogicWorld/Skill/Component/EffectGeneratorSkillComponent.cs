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
        public override void InitializeComponent()
        {
            m_generator = GetLogicWorld().GetEffectManager().CreateGenerator(m_generator_cfgid, GetOwnerEntity());
        }

        protected override void OnDestruct()
        {
            if (m_generator != null)
            {
                GetLogicWorld().GetEffectManager().DestroyGenerator(m_generator.ID, GetOwnerEntityID());
                m_generator = null;
            }
        }
        #endregion

        public override void Inflict(FixPoint start_time)
        {
            if (m_generator == null)
                return;
            List<Target> targets = GetOwnerSkill().GetTargets();
            if (targets.Count == 0)
                return;
            Entity attacker = GetOwnerEntity();
            EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
            app_data.m_original_entity_id = attacker.ID;
            app_data.m_source_entity_id = attacker.ID;
            for (int i = 0; i < targets.Count; ++i)
            {
                m_current_target = targets[i].GetEntity();
                if (m_current_target == null)
                    continue;
                m_generator.Activate(app_data, m_current_target);
            }
            m_current_target = null;
            RecyclableObject.Recycle(app_data);
        }

        public override void Deactivate()
        {
            m_generator.Deactivate();
        }
    }
}
