using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public abstract class EntityComponent : Component
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
            return (Entity)ParentObject;
        }
        #endregion
    }
}