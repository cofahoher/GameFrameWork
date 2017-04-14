using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class LevelTableData
    {
        public int m_max_level = 0;
        public FixPoint[] m_table = null;
        public FixPoint this[int level]
        {
            get
            {
                if (level < 0)
                    return m_table[0];
                if (level > m_max_level)
                    return m_table[m_max_level];
                else
                    return m_table[level];
            }
        }
    }
}