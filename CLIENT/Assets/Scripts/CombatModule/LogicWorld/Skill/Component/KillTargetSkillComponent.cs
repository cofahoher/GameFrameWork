using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class KillTargetSkillComponent : SkillComponent
    {
        //配置数据

        //运行数据

        #region 初始化/销毁
        protected override void OnDestruct()
        {
        }
        #endregion

        public override void Inflict(FixPoint start_time)
        {
            Skill skill = GetOwnerSkill();
            List<Target> targets = skill.GetTargets();
            LogicWorld logic_world = GetLogicWorld();
            for (int i = 0; i < targets.Count; ++i)
            {
                m_current_target = targets[i].GetEntity(logic_world);
                if (m_current_target == null)
                    continue;
                EntityUtil.KillEntity(m_current_target, GetOwnerEntityID());
            }
            m_current_target = null;
        }

        public override void Deactivate(bool force)
        {
        }
    }
}
