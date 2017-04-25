using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IGridGraph
    {
        /*
         * 生成一张平面图：
         * grid_size：格子边长
         * max_size_x：平面地图X轴方向的总长度
         * max_size_z：平面地图Z轴方向的总长度
         * height：平面地图在Y方向的高度
         * left_bottom_position：地图XZ坐标最小的点的位置
         * seeker_radius：如果不想看到角色一部分钻进不可入的地方，设置为角色的边长
         */
        void GenerateAsPlaneMap(FixPoint grid_size, FixPoint max_size_x, FixPoint max_size_z, FixPoint height = default(FixPoint), Vector3FP left_bottom_position = new Vector3FP(), FixPoint seeker_radius = default(FixPoint));
        /*
         * 加入一块不可靠近的阻挡：
         * center：中心
         * extent：不可以范围的半边长
         */
        void CoverArea(Vector3FP center, Vector3FP extent);
        /*
         * 取消一块不可靠近的阻挡：
         * center：中心
         * extent：不可以范围的半边长
         */
        void UncoverArea(Vector3FP center, Vector3FP extent);
        /*
         * 寻路：
         * start_pos：起点
         * end_pos：终点
         */
        bool FindPath(Vector3FP start_pos, Vector3FP end_pos);
        /*
         * 获得寻路结果：
         * 注意，不要直接保存这个List的引用，拷贝给自己的数据
         */
        List<Vector3FP> GetPath();
    }

    public class GridGraph : IGridGraph
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
        const int SEARCH_LIMITATION = 1000;
        const int NEIGHBOUR_COUNT = 8;
        const int SOUTH = 0;
        const int EAST = 1;
        const int NORTH = 2;
        const int WEST = 3;
        const int SOUTH_EAST = 4;
        const int NORTH_EAST = 5;
        const int NORTH_WEST = 6;
        const int SOUTH_WEST = 7;
        readonly int[] NEIGHBOUR_X_OFFSET = new int[NEIGHBOUR_COUNT] { 0, 1, 0, -1, 1, 1, -1, -1 };
        readonly int[] NEIGHBOUR_Z_OFFSET = new int[NEIGHBOUR_COUNT] { -1, 0, 1, 0, -1, 1, 1, -1 };
        readonly int[] NEIGHBOUR_COST = new int[8] { 100, 100, 100, 100, 141, 141, 141, 141 };
        readonly int[,] XZ_OFFSET_TO_DIRECTION = new int[3, 3] { { 7, 3, 6 }, { 0, 0, 2 }, { 4, 1, 5 } };

        List<GridNode> m_nodes = new List<GridNode>();
        Vector3FP m_left_bottom_position = new Vector3FP();
        FixPoint m_seeker_radius = FixPoint.Zero;
        FixPoint m_grid_size = FixPoint.One;
        int m_grid_x_count = 0;
        int m_grid_z_count = 0;
        FixPoint m_grid_half_size;
        int m_max_x_index = 0;
        int m_max_z_index = 0;

        int m_path_id = 0;
        Heap<GridNode> m_open_set = new Heap<GridNode>(Heap<GridNode>.CheckPriorityMethod.CPM_LESS);
        List<GridNode> m_node_path = new List<GridNode>();
        List<Vector3FP> m_position_path = new List<Vector3FP>();

        #region 测试用
        public List<GridNode> GetAllNodes()
        {
            return m_nodes;
        }

        public Vector3FP GetLeftBottomPosition()
        {
            return m_left_bottom_position;
        }

        public FixPoint GetGridSize()
        {
            return m_grid_size;
        }

        public int GetGridXCount()
        {
            return m_grid_x_count;
        }

        public int GetGridZCount()
        {
            return m_grid_z_count;
        }
        #endregion

        public void InitializeFromData()
        {
            //ZZWTODO
            m_grid_half_size = m_grid_size / FixPoint.Two;
            m_max_x_index = m_grid_x_count - 1;
            m_max_z_index = m_grid_z_count - 1;
        }


        public bool FindPath(Vector3FP start_pos, Vector3FP end_pos)
        {
            m_node_path.Clear();
            m_position_path.Clear();
            GridNode start_node = Position2Node(start_pos);
            GridNode end_node = Position2Node(end_pos);
            if (start_node == null || end_node == null)
                return false;
            if (!start_node.Walkable)
            {
                start_node = FindNearestWalkableNode(start_node, end_node);
                if (start_node == null)
                    return false;
            }
            if (!end_node.Walkable || end_node.m_area != start_node.m_area)
            {
                end_node = FindNearestWalkableNode(end_node, start_node, start_node.m_area);
                if (end_node == null)
                    return false;
            }
            AStarFindPath(start_node, end_node);
            SmoothPath(start_pos, end_pos);
            return true;
        }

        public List<Vector3FP> GetPath()
        {
            return m_position_path;
        }

        GridNode Position2Node(Vector3FP position)
        {
            int x_index = (int)((position.x - m_left_bottom_position.x) / m_grid_size);
            int z_index = (int)((position.z - m_left_bottom_position.z) / m_grid_size);
            return GetNode(x_index, z_index);
        }

        GridNode Position2NearestNode(Vector3FP position)
        {
            int x_index = (int)((position.x - m_left_bottom_position.x) / m_grid_size);
            if (x_index < 0)
                x_index = 0;
            else if (x_index > m_max_x_index)
                x_index = m_max_x_index;
            int z_index = (int)((position.z - m_left_bottom_position.z) / m_grid_size);
            if (z_index < 0)
                z_index = 0;
            else if (z_index > m_max_z_index)
                z_index = m_max_z_index;
            return GetNodeUnCheck(x_index, z_index);
        }

        public Vector3FP Node2Position(GridNode node)
        {
            return new Vector3FP((FixPoint)node.m_x * m_grid_size + m_grid_half_size + m_left_bottom_position.x, node.m_height, (FixPoint)node.m_z * m_grid_size + m_grid_half_size + m_left_bottom_position.z);
        }

        public GridNode GetNode(int x_index, int z_index)
        {
            if (x_index < 0 || x_index > m_max_x_index || z_index < 0 || z_index > m_max_z_index)
                return null;
            return m_nodes[z_index * m_grid_x_count + x_index];
        }

        public GridNode GetNodeUnCheck(int x_index, int z_index)
        {
            return m_nodes[z_index * m_grid_x_count + x_index];
        }

        GridNode FindNearestWalkableNode(GridNode start_node, GridNode end_node, int area = 0)
        {
            m_open_set.Clear();
            ChangePathID();
            int search_count = 0;
            GridNode current_node = null;
            start_node.m_path_id = m_path_id;
            m_open_set.Enqueue(start_node);
            while (!m_open_set.Empty())
            {
                current_node = m_open_set.Dequeue();
                if (current_node.Walkable && (area == 0 || current_node.m_area == area))
                    return current_node;
                for (int i = 0; i < NEIGHBOUR_COUNT; ++i)
                {
                    GridNode neighbour = GetNode(current_node.m_x + NEIGHBOUR_X_OFFSET[i], current_node.m_z + NEIGHBOUR_Z_OFFSET[i]);
                    if (neighbour == null)
                        continue;
                    if (neighbour.m_path_id != m_path_id)
                    {
                        neighbour.m_path_id = m_path_id;
                        neighbour.m_g = 0;
                        neighbour.m_h = CalculateHCost(current_node, end_node);
                        m_open_set.Enqueue(neighbour);
                    }
                }
                search_count += NEIGHBOUR_COUNT;
                if (search_count > SEARCH_LIMITATION)
                    break;
            }
            return null;
        }

        bool AStarFindPath(GridNode start_node, GridNode end_node)
        {
            m_open_set.Clear();
            ChangePathID();
            start_node.m_parent = null;
            end_node.m_parent = null;
            bool solved = false;
            int search_count = 0;
            GridNode current_node = null;
            start_node.m_path_id = m_path_id;
            start_node.Closed = false;
            m_open_set.Enqueue(start_node);
            while (!m_open_set.Empty())
            {
                current_node = m_open_set.Dequeue();
                current_node.Closed = true;
                if (current_node == end_node)
                {
                    solved = true;
                    break;
                }
                OpenNeighbors(current_node, end_node, ref search_count);
                if (search_count > SEARCH_LIMITATION)
                    break;
            }
            int i = 0;
            GridNode node = current_node;
            while (node != null)
            {
                ++i;
                node = node.m_parent;
                m_node_path.Add(null);
            }
            node = current_node;
            while (node != null)
            {
                m_node_path[--i] = node;
                node = node.m_parent;
            }
            return solved;
        }

        void ChangePathID()
        {
            ++m_path_id;
            if (m_path_id == 0)
            {
                for (int i = 0; i < m_nodes.Count; ++i)
                    m_nodes[i].m_path_id = 0;
                ++m_path_id;
            }
        }

        void OpenNeighbors(GridNode node, GridNode end_node, ref int search_count)
        {
            for (int i = 0; i < NEIGHBOUR_COUNT; ++i)
            {
                if (!node.HasConnection(i))
                    continue;
                GridNode neighbour = GetNode(node.m_x + NEIGHBOUR_X_OFFSET[i], node.m_z + NEIGHBOUR_Z_OFFSET[i]);
                if (neighbour == null)
                    continue;
                int neighbour_cost = NEIGHBOUR_COST[i];
                if (neighbour.m_path_id == m_path_id)
                {
                    if (!neighbour.Closed)
                    {
                        if (node.m_g + neighbour_cost < neighbour.m_g)
                        {
                            neighbour.m_parent = node;
                            neighbour.m_g = node.m_g + neighbour_cost;
                            m_open_set.UpdatePriority(neighbour);
                        }
                    }
                }
                else
                {
                    neighbour.m_path_id = m_path_id;
                    neighbour.m_parent = node;
                    neighbour.m_g = node.m_g + neighbour_cost;
                    neighbour.m_h = CalculateHCost(node, end_node);
                    neighbour.Closed = false;
                    m_open_set.Enqueue(neighbour);
                    ++search_count;
                }
            }
        }

        int CalculateHCost(GridNode start_node, GridNode end_node)
        {
            int dx = start_node.m_x - end_node.m_x;
            if (dx < 0)
                dx = -dx;
            int dz = start_node.m_z - end_node.m_z;
            if (dz < 0)
                dz = -dz;
            return (dx + dz) * 100;
        }

        void SmoothPath(Vector3FP start_pos, Vector3FP end_pos)
        {
            if (m_node_path.Count < 0)
                return;
            m_position_path.Add(start_pos);
            GridNode start_node = Position2Node(start_pos);
            GridNode last_output_node = m_node_path[0];
            if (start_node != last_output_node)
                m_position_path.Add(Node2Position(last_output_node));
            if (m_node_path.Count > 2)
            {
                int index = 2;
                GridNode current_node = m_node_path[index - 1];
                GridNode previous_node = m_node_path[index - 2];
                int previous_dx = current_node.m_x - previous_node.m_x;
                int previous_dz = current_node.m_z - previous_node.m_z;
                bool use_dxdz = true;
                while(index < m_node_path.Count)
                {
                    current_node = m_node_path[index];
                    previous_node = m_node_path[index - 1];
                    if (use_dxdz && current_node.m_x - previous_node.m_x == previous_dx && current_node.m_z - previous_node.m_z == previous_dz)
                    {
                        ++index;
                    }
                    else if (IsStraightLineWalkable(last_output_node, current_node))
                    {
                        ++index;
                        use_dxdz = false;
                    }
                    else
                    {
                        last_output_node = previous_node;
                        m_position_path.Add(Node2Position(previous_node));
                        ++index;
                        previous_dx = m_node_path[index - 1].m_x - m_node_path[index - 2].m_x;
                        previous_dz = m_node_path[index - 1].m_z - m_node_path[index - 2].m_z;
                        use_dxdz = true;
                    }
                }
            }
            GridNode end_node = Position2Node(end_pos);
            if (end_node != m_node_path[m_node_path.Count - 1])
                m_position_path.Add(Node2Position(m_node_path[m_node_path.Count - 1]));
            else
                m_position_path.Add(end_pos);
        }

        bool IsStraightLineWalkable(GridNode start_node, GridNode end_node)
        {
            int dx = end_node.m_x - start_node.m_x;
            int dz = end_node.m_z - start_node.m_z;
            int nx, nz, sign_x, sign_z;
            if (dx >= 0)
            {
                nx = dx;
                sign_x = 1;
            }
            else
            {
                nx = -dx;
                sign_x = -1;
            }
            if (dz >= 0)
            {
                nz = dz;
                sign_z = 1;
            }
            else
            {
                nz = -dz;
                sign_z = -1;
            }
            GridNode current_node = start_node;
            int x = start_node.m_x;
            int z = start_node.m_z;
            for (int ix = 0, iz = 0; ix < nx || iz < nz;)
            {
                int temp = (2 * ix + 1) * nz - (2 * iz + 1) * nx;
                if (temp == 0)
                {
                    if (!current_node.HasConnection(XZ_OFFSET_TO_DIRECTION[sign_x + 1, sign_z + 1]))
                        return false;
                    x += sign_x;
                    z += sign_z;
                    ++ix;
                    ++iz;
                    current_node = GetNodeUnCheck(x, z);
                }
                else if (temp < 0)
                {
                    if (!current_node.HasConnection(XZ_OFFSET_TO_DIRECTION[sign_x + 1, 1]))
                        return false;
                    x += sign_x;
                    ++ix;
                    current_node = GetNodeUnCheck(x, z);
                }
                else
                {
                    if (!current_node.HasConnection(XZ_OFFSET_TO_DIRECTION[1, sign_z + 1]))
                        return false;
                    z += sign_z;
                    ++iz;
                    current_node = GetNodeUnCheck(x, z);
                }
            }
            return true;
        }

        public void CoverArea(Vector3FP center, Vector3FP extent)
        {
            ChangeArea(center, extent, 1);
        }

        public void UncoverArea(Vector3FP center, Vector3FP extent)
        {
            ChangeArea(center, extent, -1);
        }

        void ChangeArea(Vector3FP center, Vector3FP extent, sbyte fill_count)
        {
            Vector3FP downleft = center - extent;
            downleft.x -= m_seeker_radius;
            downleft.z -= m_seeker_radius;
            Vector3FP upright = center + extent;
            upright.x += m_seeker_radius - FixPoint.PrecisionFP;
            upright.z += m_seeker_radius - FixPoint.PrecisionFP;
            GridNode min_node = Position2NearestNode(downleft);
            GridNode max_node = Position2NearestNode(upright);
            for (int z = min_node.m_z; z <= max_node.m_z; ++z)
            {
                for (int x = min_node.m_x; x <= max_node.m_x; ++x)
                    GetNodeUnCheck(x, z).m_fill_count += fill_count;
            }
            for (int z = System.Math.Max(min_node.m_z - 1, 0); z <= System.Math.Min(max_node.m_z + 1, m_max_z_index); ++z)
            {
                for (int x = System.Math.Max(min_node.m_x - 1, 0); x <= System.Math.Min(max_node.m_x + 1, m_max_x_index); ++x)
                    CalculateConnection(GetNodeUnCheck(x, z));
            }
        }

        void CalculateConnection(GridNode node)
        {
            node.ClearConnection();
            if (!node.Walkable)
                return;
            for (int i = 0; i < NEIGHBOUR_COUNT; ++i)
            {
                GridNode neighbour = GetNode(node.m_x + NEIGHBOUR_X_OFFSET[i], node.m_z + NEIGHBOUR_Z_OFFSET[i]);
                if (neighbour.Walkable)
                    node.SetConnection(i, true);
            }
        }

        public void GenerateAsPlaneMap(FixPoint grid_size, FixPoint max_size_x, FixPoint max_size_z, FixPoint height, Vector3FP left_bottom_position, FixPoint seeker_radius)
        {
            m_seeker_radius = seeker_radius;
            m_left_bottom_position = left_bottom_position;
            m_grid_size = grid_size;
            m_grid_x_count = (int)((max_size_x + grid_size - FixPoint.PrecisionFP) / grid_size);
            m_grid_z_count = (int)((max_size_z + grid_size - FixPoint.PrecisionFP) / grid_size);

            m_grid_half_size = m_grid_size / FixPoint.Two;
            m_max_x_index = m_grid_x_count - 1;
            m_max_z_index = m_grid_z_count - 1;

            m_nodes.Clear();
            m_nodes.Capacity = m_grid_x_count * m_grid_z_count;
            int index = 0;
            GridNode node = null;
            byte connection = 0xFF;
            for (index = 0; index < m_grid_x_count * m_grid_z_count; ++index)
                m_nodes.Add(new GridNode(index % m_grid_x_count, index / m_grid_x_count, height, connection, 0, 0));
            for (int x = 0; x < m_grid_x_count; ++x)
            {
                //down
                index = x;
                node = m_nodes[index];
                node.m_connection = 0;
                node.m_fill_count = 1;
                //up
                index = m_max_z_index * m_grid_x_count + x;
                node = m_nodes[index];
                node.m_connection = 0;
                node.m_fill_count = 1;
            }
            for (int z = 0; z < m_grid_z_count; ++z)
            {
                //left
                index = z * m_grid_x_count;
                node = m_nodes[index];
                node.m_connection = 0;
                node.m_fill_count = 1;
                //right
                index = z * m_grid_x_count + m_max_x_index;
                node = m_nodes[index];
                node.m_connection = 0;
                node.m_fill_count = 1;
            }
            byte down_connection = (byte)(1 << 6 | 1 << 2 | 1 << 5 | 1 << 3 | 1 << 1);
            byte up_connection = (byte)(1 << 7 | 1 << 0 | 1 << 4 | 1 << 3 | 1 << 1);
            byte left_connection = (byte)(1 << 5 | 1 << 1 | 1 << 4 | 1 << 2 | 1 << 0);
            byte rigth_connection = (byte)(1 << 6 | 1 << 3 | 1 << 7 | 1 << 2 | 1 << 0);
            byte downleft_connection = (byte)(1 << 2 | 1 << 5 | 1 << 1);
            byte downright_connection = (byte)(1 << 6 | 1 << 2 | 1 << 3);
            byte upleft_connection = (byte)(1 << 0 | 1 << 1 | 1 << 4);
            byte upright_connection = (byte)(1 << 7 | 1 << 0 | 1 << 3);
            for (int x = 1; x < m_max_x_index; ++x)
            {
                //down
                index = m_grid_x_count + x;
                node = m_nodes[index];
                node.m_connection = down_connection;
                //up
                index = (m_max_z_index - 1) * m_grid_x_count + x;
                node = m_nodes[index];
                node.m_connection = up_connection;
            }
            for (int z = 1; z < m_max_z_index; ++z)
            {
                //left
                index = z * m_grid_x_count + 1;
                node = m_nodes[index];
                node.m_connection = left_connection;
                //right
                index = z * m_grid_x_count + m_max_x_index - 1;
                node = m_nodes[index];
                node.m_connection = rigth_connection;
            }
            m_nodes[m_grid_x_count + 1].m_connection = downleft_connection;
            m_nodes[m_grid_x_count + (m_max_x_index - 1)].m_connection = downright_connection;
            m_nodes[(m_max_z_index - 1) * m_grid_x_count + 1].m_connection = upleft_connection;
            m_nodes[(m_max_z_index - 1) * m_grid_x_count + (m_max_x_index - 1)].m_connection = upright_connection;
        }
    }
}