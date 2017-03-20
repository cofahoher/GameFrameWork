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