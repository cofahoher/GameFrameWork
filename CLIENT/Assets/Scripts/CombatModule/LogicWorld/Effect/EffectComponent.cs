using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectComponent : Component
    {
        public virtual void Apply()
        {
        }

        public virtual void Unapply()
        {
        }

        #region Variable
        public override FixPoint GetVariable(ExpressionVariable variable, int index)
        {
            int vid = variable[index];
            if (vid == ExpressionVariable.VID_OriginalSource)
            {
                Entity original = GetLogicWorld().GetEntityManager().GetObject(((Effect)ParentObject).GetDefinitionComponent().OriginalEntityID);
                if (original != null)
                    return original.GetVariable(variable, index + 1);
                else
                    return FixPoint.Zero;
            }
            else if (vid == ExpressionVariable.VID_Source)
            {
                Entity source = GetLogicWorld().GetEntityManager().GetObject(((Effect)ParentObject).GetDefinitionComponent().SourceEntityID);
                if (source != null)
                    return source.GetVariable(variable, index + 1);
                else
                    return FixPoint.Zero;
            }
            else if (vid == ExpressionVariable.VID_Target)
            {
                Entity target = GetLogicWorld().GetEntityManager().GetObject(((Effect)ParentObject).GetDefinitionComponent().TargetEntityID);
                if (target != null)
                    return target.GetVariable(variable, index + 1);
                else
                    return FixPoint.Zero;
            }
            return base.GetVariable(variable, index);
        }
        #endregion
    }
}