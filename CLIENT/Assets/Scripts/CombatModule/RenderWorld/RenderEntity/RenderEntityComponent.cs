using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public abstract class RenderEntityComponent : Component
    {
        #region ILogicOwnerInfo
        public override int GetOwnerPlayerID()
        {
            return ParentObject.GetOwnerPlayerID();
        }
        public override Player GetOwnerPlayer()
        {
            return ParentObject.GetOwnerPlayer();
        }
        public override int GetOwnerEntityID()
        {
            return ParentObject.ID;
        }
        public override Entity GetOwnerEntity()
        {
            return ParentObject.GetOwnerEntity();
        }
        #endregion

        #region Variable
        public override FixPoint GetVariable(ExpressionVariable variable, int index)
        {
            return FixPoint.Zero;
        }

        public override FixPoint GetVariable(int id)
        {
            return FixPoint.Zero;
        }

        public override bool GetVariable(int id, out FixPoint value)
        {
            value = FixPoint.Zero;
            return false;
        }

        public override bool SetVariable(int id, FixPoint value)
        {
            return false;
        }
        #endregion

        public RenderWorld GetRenderWorld()
        {
            return GetRenderEntity().GetRenderWorld();
        }

        public RenderEntity GetRenderEntity()
        {
            return (RenderEntity)ParentObject;
        }

        public Entity GetLogicEntity()
        {
            return ((RenderEntity)ParentObject).GetLogicEntity();
        }
    }
}