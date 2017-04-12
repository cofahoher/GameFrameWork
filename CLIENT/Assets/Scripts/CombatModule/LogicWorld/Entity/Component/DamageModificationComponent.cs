using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class DamageModificationComponent : EntityComponent
    {
        public FixPoint ApplyModifiersToDamage(Damage damage, FixPoint damage_amount, Object opponent, bool is_attacker)
        {
            return damage_amount;
        }
    }
}