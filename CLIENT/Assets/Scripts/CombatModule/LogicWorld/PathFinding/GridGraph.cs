using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class GridGraph
    {
        /*
         *         Z
         *         |
         *         |
         *      6  2  5
         *       \ | /
         * ----3 - X - 1---- X
         *       / | \
         *      7  0  4
         *         |
         *         |
         */
        int[] NEIGHBOUR_X_OFFSET = new int[8]{0, 1, 0, -1, 1, 1, -1, -1};
        int[] NEIGHBOUR_Z_OFFSET = new int[8]{-1, 0, 1, 0, -1, 1, 1, -1};
        int[] NEIGHBOUR_COST = new int[8]{100, 100, 100, 100, 141, 141, 141, 141};
        Heap<GridNode> m_open_set = new Heap<GridNode>(Heap<GridNode>.CheckPriorityMethod.CPM_LESS);
        List<GridNode> m_nodes = new List<GridNode>();
        FixPoint m_grid_size = FixPoint.One;
        int m_grid_x_size = 0;
        int m_grid_z_size = 0;
        int m_max_x = 0;
        int m_max_z = 0;
        int m_path_id = 0;

        public void Initialize()
        {
            m_grid_x_size = 0;
            m_grid_z_size = 0;
            m_max_x = m_grid_x_size - 1;
            m_max_z = m_grid_z_size - 1;
        }

        public bool FindPath(Vector2FP start_pos, Vector2FP end_pos)
        {
            GridNode start_node = Position2Node(start_pos);
            GridNode end_node = Position2Node(end_pos);
            if (start_node.m_fill_count < 0)
                return false;
            if (end_node.m_fill_count < 0 && end_node.m_area != start_node.m_area)
                return false;
            return AStarFindPath(start_node, end_node);
        }

        GridNode Position2Node(Vector2FP position)
        {
            int x_index = (int)(position.x / m_grid_size);
            int z_index = (int)(position.z / m_grid_size);
            return GetNode(x_index, z_index);
        }

        GridNode GetNode(int x_index, int z_index)
        {
            if (x_index < 0 || x_index >= m_max_x || z_index < 0 || z_index >= m_max_z)
                return null;
            return m_nodes[z_index * m_grid_x_size + x_index];
        }

        bool AStarFindPath(GridNode start_node, GridNode end_node)
        {
            start_node.m_parent = null;
            end_node.m_parent = null;
            ++m_path_id;
            bool solved = false;
            int search_count = 0;
            GridNode current_node = null;
            m_open_set.Enqueue(start_node);
            while (!m_open_set.Empty())
            {
                current_node = m_open_set.Dequeue();
                if (current_node == end_node)
                {
                    solved = true;
                    break;
                }
                OpenNeighbors(current_node, end_node, ref search_count);
            }
            return solved;
        }

        void OpenNeighbors(GridNode node, GridNode end_node, ref int search_count)
        {
            for (int i = 0; i < 8; ++i)
            {
            }
        }
    }
}