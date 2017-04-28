using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class ManaComponent : EntityComponent
    {
        public const string DEFAULT_MANA_TYPE_NAME = "mana";
        public static readonly int DEFAULT_MANA_TYPE_ID = (int)CRC.Calculate(DEFAULT_MANA_TYPE_NAME);

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