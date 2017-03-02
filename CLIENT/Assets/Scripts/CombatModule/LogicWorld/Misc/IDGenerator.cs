﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class IDGenerator
    {
        public const int DEFAULT_FIRST_ID             = -1000000;
        public const int PLAYER_FIRST_ID              =  1;
        public const int ENTITY_FIRST_ID              =  100;
        public const int ABILITY_FIRST_ID             =  1000000;
        public const int EFFECT_GENERATOR_FIRST_ID    =  2000000;
        public const int EFFECT_FIRST_ID              =  3000000;
        public const int ATTRIBUTE_MODIFIER_FIRST_ID  =  4000000;
        public const int DAMAGE_MODIFICATION_FIRST_ID =  5000000;

        int m_next_id = 0;

        public IDGenerator(int first_id = DEFAULT_FIRST_ID)
        {
            m_next_id = first_id;
        }

        public int GenID()
        {
            return m_next_id++;
        }
    }
}