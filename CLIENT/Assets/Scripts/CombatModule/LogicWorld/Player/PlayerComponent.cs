using System.Collections;
namespace Combat
{
    public class PlayerComponent : Component
    {
        #region ILogicOwnerInfo
        public override int GetOwnerPlayerID()
        {
            return ParentObject.ID;
        }
        public override Player GetOwnerPlayer()
        {
            return (Player)ParentObject;
        }
        public override int GetOwnerEntityID()
        {
            return 0;
        }
        public override Entity GetOwnerEntity()
        {
            return null;
        }
        #endregion
    }
}