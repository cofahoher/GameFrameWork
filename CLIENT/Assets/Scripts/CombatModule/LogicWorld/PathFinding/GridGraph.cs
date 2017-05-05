using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IGridGraph : IDestruct
    {
        /*
         * 从二进制文件生成，尚未支持
         */
        void GenerateFromData();
        /*
         * 动态生成一张平面图：
         * grid_size：格子边长，六边形也是边长
         * max_size_x：平面地图X轴方向的总长度
         * max_size_z：平面地图Z轴方向的总长度
         * height：平面地图在Y方向的高度
         * left_bottom_position：地图XZ坐标最小的点的位置
         * seeker_radius：如果不想看到角色一部分钻进不可入的地方，设置为角色的半径
         */
        void GenerateAsPlaneMap(FixPoint grid_size, FixPoint max_size_x, FixPoint max_size_z, FixPoint height = default(FixPoint), Vector3FP left_bottom_position = new Vector3FP(), FixPoint seeker_radius = default(FixPoint));
        /*
         * 加入一块不可靠近的阻挡：
         * center：中心
         * extent：不可入范围的半边长
         */
        void CoverArea(Vector3FP center, Vector3FP extent);
        /*
         * 取消一块不可靠近的阻挡：
         * center：中心
         * extent：不可入范围的半边长
         */
        void UncoverArea(Vector3FP center, Vector3FP extent);
        /*
         * 寻路：
         * start_pos：起点
         * end_pos：终点
         * 返回值：是否成功
         */
        bool FindPath(Vector3FP start_pos, Vector3FP end_pos);
        /*
         * 获得寻路结果，注意：
         * 1、不要直接保存这个List的引用，拷贝给自己的数据
         * 2、结果的第一个是起点
         * 3、如果寻路失败，返回的长度是0
         */
        List<Vector3FP> GetPath();
        /*
         * 以下一些帮助函数
         */
        GridNode Position2Node(Vector3FP position);
        GridNode Position2NearestNode(Vector3FP position);
        Vector3FP Node2Position(GridNode node);
        GridNode GetNode(int x_index, int z_index);
    }

    public abstract class GridGraph : IGridGraph
    {
        public const int SqureNodeType = 0;
        public const int HexagonNodeType = 1;
        public static GridGraph CreateGraph(int type)
        {
            if (type == SqureNodeType)
                return new SquareGridGraph();
            else if (type == HexagonNodeType)
                return new HexagonGridGraph();
            else
                return null;
        }

        protected int SEARCH_LIMITATION = 1000;
        protected int NEIGHBOUR_COUNT;
        protected int[] NEIGHBOUR_COST;

        protected GridNode[,] m_nodes;
        protected FixPoint GRID_SIZE = FixPoint.One;
        protected int GRID_X_COUNT = 0;
        protected int GRID_Z_COUNT = 0;
        protected int MAX_X_INDEX = 0;
        protected int MAX_Z_INDEX = 0;
        protected Vector3FP m_left_bottom_position = new Vector3FP();
        protected FixPoint m_seeker_radius = FixPoint.Zero;

        protected int m_path_id = 1;
        protected Heap<GridNode> m_open_set = new Heap<GridNode>(Heap<GridNode>.CheckPriorityMethod.CPM_LESS);
        protected List<GridNode> m_node_path = new List<GridNode>();
        protected List<Vector3FP> m_position_path = new List<Vector3FP>();

        public void Destruct()
        {
        }

        #region 测试用
        public GridNode[,] GetAllNodes()
        {
            return m_nodes;
        }

        public int GetGridXCount()
        {
            return GRID_X_COUNT;
        }

        public int GetGridZCount()
        {
            return GRID_Z_COUNT;
        }

        public FixPoint GetGridSize()
        {
            return GRID_SIZE;
        }

        public Vector3FP GetLeftBottomPosition()
        {
            return m_left_bottom_position;
        }

        public int GetPathID()
        {
            return m_path_id;
        }
        #endregion

        public abstract int GetGraphType();

        public abstract void GenerateFromData();

        public abstract void GenerateAsPlaneMap(FixPoint grid_size, FixPoint max_size_x, FixPoint max_size_z, FixPoint height, Vector3FP left_bottom_position, FixPoint seeker_radius);

        public void CoverArea(Vector3FP center, Vector3FP extent)
        {
            ChangeArea(center, extent, 1);
        }

        public void UncoverArea(Vector3FP center, Vector3FP extent)
        {
            ChangeArea(center, extent, -1);
        }

        public bool FindPath(Vector3FP start_pos, Vector3FP end_pos)
        {
            m_node_path.Clear();
            m_position_path.Clear();
            GridNode start_node = Position2NearestNode(start_pos);
            GridNode end_node = Position2NearestNode(end_pos);
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

        public abstract GridNode Position2Node(Vector3FP position);

        public abstract GridNode Position2NearestNode(Vector3FP position);

        public abstract Vector3FP Node2Position(GridNode node);

        public GridNode GetNode(int x_index, int z_index)
        {
            if (x_index < 0 || x_index > MAX_X_INDEX || z_index < 0 || z_index > MAX_Z_INDEX)
                return null;
            return m_nodes[x_index, z_index];
        }

        protected GridNode GetNodeUncheck(int x_index, int z_index)
        {
            return m_nodes[x_index, z_index];
        }

        protected GridNode FindNearestWalkableNode(GridNode start_node, GridNode end_node, int area = 0)
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
                    GridNode neighbour = GetNeighbourNode(current_node, i);
                    if (neighbour == null)
                        continue;
                    int neighbour_cost = NEIGHBOUR_COST[i];
                    if (neighbour.m_path_id != m_path_id)
                    {
                        neighbour.m_path_id = m_path_id;
                        neighbour.m_g = current_node.m_g + neighbour_cost;
                        if (!neighbour.Walkable)
                            neighbour.m_g += 1000;
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
            //ZZWTODO 看着比较丑
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
                GridNode node;
                for (int x = 0; x < GRID_X_COUNT; ++x)
                {
                    for (int z = 0; z < GRID_Z_COUNT; ++z)
                    {
                        node = m_nodes[x, z];
                        if (node != null)
                            node.m_path_id = 0;
                    }
                }
                ++m_path_id;
            }
        }

        void OpenNeighbors(GridNode node, GridNode end_node, ref int search_count)
        {
            for (int i = 0; i < NEIGHBOUR_COUNT; ++i)
            {
                if (!node.HasConnection(i))
                    continue;
                GridNode neighbour = GetNeighbourNode(node, i);
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
                    neighbour.m_h = CalculateHCost(neighbour, end_node);
                    neighbour.Closed = false;
                    m_open_set.Enqueue(neighbour);
                    ++search_count;
                }
            }
        }

        protected abstract GridNode GetNeighbourNode(GridNode node, int direction_index);

        protected abstract int CalculateHCost(GridNode start_node, GridNode end_node);

        protected abstract void SmoothPath(Vector3FP start_pos, Vector3FP end_pos);

        protected abstract void ChangeArea(Vector3FP center, Vector3FP extent, sbyte fill_count);

        protected void CalculateConnection(GridNode node)
        {
            node.ClearConnection();
            if (!node.Walkable)
                return;
            for (int i = 0; i < NEIGHBOUR_COUNT; ++i)
            {
                GridNode neighbour = GetNeighbourNode(node, i);
                if (neighbour != null && neighbour.Walkable)
                    node.SetConnection(i, true);
            }
        }
    }
}