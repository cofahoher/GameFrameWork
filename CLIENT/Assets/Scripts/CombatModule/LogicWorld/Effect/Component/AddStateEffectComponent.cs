using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class AddStateEffectComponent : EffectComponent
    {
        int m_state;

        public override void Apply()
        {
            Entity target = GetOwnerEntity();
            if (target == null)
                return;
            if (ObjectUtil.IsDead(target))
                return;
            StateComponent state_component = target.GetComponent(StateComponent.ID) as StateComponent;
            if (state_component == null)
                return;
            state_component.AddState(m_state, ParentObject.ID);
        }

        public override void Unapply()
        {
            Entity target = GetOwnerEntity();
            if (target == null)
                return;
            if (ObjectUtil.IsDead(target))
                return;
            StateComponent state_component = target.GetComponent(StateComponent.ID) as StateComponent;
            if (state_component == null)
                return;
            state_component.RemoveState(m_state, ParentObject.ID);
        }
    }
}