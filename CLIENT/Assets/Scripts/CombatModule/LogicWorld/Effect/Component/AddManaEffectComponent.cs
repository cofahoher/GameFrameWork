using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class AddManaEffectComponent : EffectComponent
    {
        int m_mana_type = 0;
        Formula m_mana_amount = RecyclableObject.Create<Formula>();

        protected override void OnDestruct()
        {
            RecyclableObject.Recycle(m_mana_amount);
            m_mana_amount = null;
        }

        public override void Apply()
        {
            Entity owner = GetOwnerEntity();
            ManaComponent mana_component = owner.GetComponent(ManaComponent.ID) as ManaComponent;
            if (mana_component == null)
                return;
            FixPoint amount = m_mana_amount.Evaluate(this);
            mana_component.ChangeMana(m_mana_type, amount);
        }

        public override void Unapply()
        {
        }
    }
}