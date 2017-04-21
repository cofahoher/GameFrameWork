using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class GridNode : HeapItem, IDestruct
    {
        //固定数据
        public int m_x = 0;
        public int m_y = 0;
        public byte m_connection = 0;
        public sbyte m_fill_count = -1;
        public short m_area = 0;
        public FixPoint m_height;
        //临时数据
        public GridNode m_parent = null;
        public int m_g = 0;
        public int m_h = 0;
        public int m_cost = 0;
        public int m_path_id = 0;

        public void Destruct()
        {
            m_parent = null;
        }
    }
}