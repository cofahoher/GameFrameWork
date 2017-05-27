using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class KillOwnerEffectComponent : EffectComponent
    {
        public override void Apply()
        {
            Entity owner = GetOwnerEntity();
            if (owner == null)
                return;
            EntityUtil.KillEntity(owner, owner.ID);
        }

        public override void Unapply()
        {
        }
    }
}