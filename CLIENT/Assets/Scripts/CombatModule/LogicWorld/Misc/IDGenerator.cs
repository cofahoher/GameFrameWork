using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class IDGenerator : IDestruct
    {
        public const int INVALID_FIRST_ID             = -1000000;
        public const int PLAYER_FIRST_ID              =  1;
        public const int ENTITY_FIRST_ID              =  100;
        public const int SKILL_FIRST_ID               =  1000000;
        public const int EFFECT_GENERATOR_FIRST_ID    =  2000000;
        public const int EFFECT_FIRST_ID              =  3000000;
        public const int SIGNAL_LISTENER_FIRST_ID     =  4000000;
        public const int ATTRIBUTE_MODIFIER_FIRST_ID  =  5000000;
        public const int DAMAGE_MODIFIER_FIRST_ID     =  6000000;
        public const int REGION_CALLBACK_FIRST_ID     =  7000000;
        public const int BEHAVIOR_TREE_FIRST_ID       =  8000000;

        int m_next_id = 0;

        public IDGenerator(int first_id = INVALID_FIRST_ID)
        {
            m_next_id = first_id;
        }

        public void Destruct()
        {
        }

        public int GenID()
        {
            return m_next_id++;
        }
    }
}