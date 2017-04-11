using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public abstract class SkillComponent : Component
    {
        public Skill GetOwnerSkill()
        {
            return (Skill)ParentObject;
        }

        public int GetOwnerSkillID()
        {
            return ParentObject.ID;
        }

        #region 技能的Activate流程
        public virtual void Activate(FixPoint start_time)
        {

        }

        public virtual void PostActivate(FixPoint start_time)
        {

        }

        public virtual void Deactivate()
        {

        }
        #endregion
    }
}