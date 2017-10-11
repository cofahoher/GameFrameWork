using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public abstract class SkillComponent : Component
    {
        protected Entity m_current_target = null;

        public Entity CurrentTarget
        {
            get { return m_current_target; }
            set { m_current_target = value; }
        }

        public Skill GetOwnerSkill()
        {
            return (Skill)ParentObject;
        }

        public int GetOwnerSkillID()
        {
            return ParentObject.ID;
        }

        public virtual bool CanActivate()
        {
            return true;
        }

        public virtual void Activate(FixPoint start_time)
        {
        }

        public virtual void PostActivate(FixPoint start_time)
        {
        }

        public virtual void Inflict(FixPoint start_time)
        {
        }

        public virtual void Deactivate(bool force)
        {
        }

        #region Variable
        public override FixPoint GetVariable(ExpressionVariable variable, int index)
        {
            int vid = variable[index];
            if (vid == ExpressionVariable.VID_Target)
            {
                if (m_current_target != null)
                    return m_current_target.GetVariable(variable, index + 1);
                else
                    return FixPoint.Zero;
            }
            else if (vid == ExpressionVariable.VID_Source)
            {
                Object owner_entity = GetOwnerEntity();
                if (owner_entity != null)
                    return owner_entity.GetVariable(variable, index + 1);
                else
                    return FixPoint.Zero;
            }
            return base.GetVariable(variable, index);
        }
        #endregion
    }
}