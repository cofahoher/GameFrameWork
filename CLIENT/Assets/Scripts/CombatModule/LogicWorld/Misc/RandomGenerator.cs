using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class RandomGenerator
    {
        int m_seed = 0;

        public RandomGenerator(int seed = 0)
        {
            m_seed = seed;
        }

        public void ResetSeed(int seed)
        {
            m_seed = seed;
        }

        public int Rand()
        {
            m_seed = m_seed * 214013 + 2531011;
            int rand = (m_seed >> 16) & 0x7fff;
            return rand;
        }

        public int RandBetween(int min_value, int max_value)
        {
            if (max_value > min_value)
            {
                int temp = max_value;
                max_value = min_value;
                min_value = temp;
            }
            return min_value + Rand() % (max_value - min_value + 1);
        }
    }
}