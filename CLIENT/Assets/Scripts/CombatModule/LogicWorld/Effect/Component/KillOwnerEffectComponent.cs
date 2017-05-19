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
            DeathComponent death_component = owner.GetComponent(DeathComponent.ID) as DeathComponent;
            if (death_component != null)
                death_component.DieSilently = true;  //ZZWTODO
            EntityUtil.KillEntity(owner, owner.ID);
        }

        public override void Unapply()
        {
        }
    }
}