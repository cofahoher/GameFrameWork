using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ISkillComponent
    {
        void Activate(FixPoint start_time);
        void PostActivate(FixPoint start_time);
        void Deactivate();
    }

    public abstract class SkillComponent : Component, ISkillComponent
    {
        public Skill GetOwnerSkill()
        {
            return (Skill)ParentObject;
        }

        public int GetOwnerSkillID()
        {
            return ParentObject.ID;
        }

        #region ISkillComponent
        public virtual void Activate(FixPoint start_time)
        {
        }

        public virtual void PostActivate(FixPoint start_time)
        {
        }

        public virtual void Inflict(FixPoint start_time)
        {
        }

        public virtual void Deactivate()
        {
        }
        #endregion
    }
}