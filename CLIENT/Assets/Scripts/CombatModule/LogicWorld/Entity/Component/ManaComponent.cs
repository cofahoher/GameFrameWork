using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class ManaComponent : EntityComponent
    {
        public FixPoint GetCurrentManaPoint()
        {
            return FixPoint.Zero;
        }

        public void ChangeMana(FixPoint delta_mana)
        {

        }
    }
}