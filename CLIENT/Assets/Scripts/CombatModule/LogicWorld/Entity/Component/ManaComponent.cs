using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class ManaComponent : EntityComponent
    {
        public FixPoint GetCurrentManaPoint(int mana_type)
        {
            return FixPoint.Zero;
        }

        public bool ChangeMana(int mana_type, FixPoint delta_mana)
        {
            return true;
        }
    }
}