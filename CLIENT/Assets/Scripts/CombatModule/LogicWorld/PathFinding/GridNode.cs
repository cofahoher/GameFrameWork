using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class GridNode : HeapItem, IDestruct
    {
        public int m_x = 0;
        public int m_z = 0;
        public FixPoint m_height;
        public byte m_connection = 0;
        public sbyte m_fill_count = 0;
        public byte m_area = 0;
        public byte m_closed = 0;

        public int m_path_id = 0;
        public GridNode m_parent = null;
        public int m_g = 0;
        public int m_h = 0;

        public GridNode()
        {
        }

        public GridNode(int x, int z, FixPoint height, byte connection, sbyte fill_count, byte area)
        {
            m_x = x;
            m_z = z;
            m_height = height;
            m_connection = connection;
            m_fill_count = fill_count;
            m_area = area;
        }

        public void Destruct()
        {
            m_parent = null;
        }

        public override int CompareTo(object obj)
        {
            GridNode item = obj as GridNode;
            if (item == null)
                return -1;
            int result = F - item.F;
            if (result != 0)
                return result;
            else
                return _insertion_index - item._insertion_index;
        }

        public bool Walkable
        {
            get { return m_fill_count <= 0; }
        }

        public int F
        {
            get { return m_g + m_h; }
        }

        public int G
        {
            get { return m_g; }
            set { m_g = value; }
        }

        public int H
        {
            get { return m_h; }
            set { m_h = value; }
        }

        public bool Closed
        {
            get { return m_closed != 0; }
            set
            {
                if (value)
                    m_closed = 1;
                else
                    m_closed = 0;
            }
        }

        public void ClearConnection()
        {
            m_connection = 0;
        }

        public bool HasConnection(int direction_index)
        {
            return ((m_connection >> direction_index) & 1) != 0;
        }

        public void SetConnection(int direction_index, bool connected)
        {
            m_connection &= (byte)(~(1 << direction_index));
            if (connected)
                m_connection |= (byte)(1 << direction_index);
        }
    }
}