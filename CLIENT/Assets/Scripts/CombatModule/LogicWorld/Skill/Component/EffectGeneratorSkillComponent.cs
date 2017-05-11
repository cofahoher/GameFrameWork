using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EffectGeneratorSkillComponent : SkillComponent
    {
        //配置数据
        int m_generator_cfgid = 0;

        //运行数据
        int m_generator_id = 0;
        Entity m_current_target = null;

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            EffectGenerator generator = GetLogicWorld().GetEffectManager().CreateGenerator(m_generator_cfgid, GetOwnerEntity());
            if (generator != null)
                m_generator_id = generator.ID;
        }

        protected override void OnDestruct()
        {
            if (m_generator_id > 0)
                GetLogicWorld().GetEffectManager().DestroyGenerator(m_generator_id, GetOwnerEntityID());
        }
        #endregion

        public override void Inflict(FixPoint start_time)
        {
            List<Target> targets = GetOwnerSkill().GetTargets();
            if (targets.Count == 0)
                return;
            EffectGenerator generator = GetLogicWorld().GetEffectManager().GetGenerator(m_generator_id);
            if (generator == null)
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
                generator.Activate(app_data, m_current_target);
            }
            m_current_target = null;
            RecyclableObject.Recycle(app_data);
        }

        public override void Deactivate()
        {
        }

        public override FixPoint GetVariable(ExpressionVariable variable, int index)
        {
            if (variable[index] == ExpressionVariable.VID_Target)
            {
                if (m_current_target != null)
                    return m_current_target.GetVariable(variable, index + 1);
                else
                    return FixPoint.Zero;
            }
            return base.GetVariable(variable, index);
        }
    }
}
