using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class HexagonGridGraph : GridGraph
    {
        /*
         *          Z
         *          |
         *       2  |  1
         *        \ | /
         * ----3 -- N -- 0---- X
         *        / | \
         *       4  |  5
         *          |
         */
        const int HEXAGON_GRID_NEIGHBOUR_COUNT = 6;
        static readonly int[,] NEIGHBOUR_X_OFFSET = new int[2, HEXAGON_GRID_NEIGHBOUR_COUNT] { { 1, 0, -1, -1, -1, 0 }, { 1, 1, 0, -1, 0, 1 } };
        static readonly int[] NEIGHBOUR_Z_OFFSET = new int[HEXAGON_GRID_NEIGHBOUR_COUNT] { 0, 1, 1, 0, -1, -1 };
        static readonly FixPoint Sqrt3 = FixPoint.Sqrt(FixPoint.FixPointDigit[3]);
        static readonly FixPoint HalfSqrt3 = Sqrt3 / FixPoint.Two;
        FixPoint HALF_GRID_SIZE;
        FixPoint SQRT3_GRID_SIZE;
        FixPoint HALF_SQRT3_GRID_SIZE;
        FixPoint THREE_QUARTERS_GRID_SIZE;
        FixPoint OVERLAP_MIN_AREA;
        int m_odd_x_count;
        int m_even_x_count;
        GridNode m_invalid_node = new GridNode();

        public HexagonGridGraph()
        {
            m_invalid_node.m_x = -1;
            m_invalid_node.m_z = -1;
            m_invalid_node.m_fill_count = 1;
            NEIGHBOUR_COUNT = HEXAGON_GRID_NEIGHBOUR_COUNT;
            NEIGHBOUR_COST = new int[HEXAGON_GRID_NEIGHBOUR_COUNT] { 100, 100, 100, 100, 100, 100 };
        }

        public override int GetGraphType()
        {
            return GridGraph.HexagonNodeType;
        }

        public override void GenerateFromData()
        {
            //ZZWTODO
        }

        public override GridNode Position2Node(Vector3FP position)
        {
            int x_index, z_index;
            Position2NodeIndex(position, out x_index, out z_index);
            return GetNode(x_index, z_index);
        }

        public override GridNode Position2NearestNode(Vector3FP position)
        {
            int x_index, z_index;
            Position2NodeIndex(position, out x_index, out z_index);
            if (z_index < 0)
                z_index = 0;
            else if (z_index > MAX_Z_INDEX)
                z_index = MAX_Z_INDEX;
            if (x_index < 0)
            {
                x_index = 0;
            }
            else
            {
                if (z_index % 2 == 0)
                {
                    if (x_index > m_odd_x_count - 1)
                        x_index = m_odd_x_count - 1;
                }
                else
                {
                    if (x_index > m_even_x_count - 1)
                        x_index = m_even_x_count - 1;
                }
            }
            return GetNodeUncheck(x_index, z_index);
        }

        void Position2NodeIndex(Vector3FP position, out int x_index, out int z_index)
        {
            //ZZWTODO 有没有简单清晰的算法
            position.x -= m_left_bottom_position.x;
            position.z -= m_left_bottom_position.z;
            int temp = (int)(position.z / HALF_GRID_SIZE);
            z_index = temp / 3;
            if (temp % 3 == 0)
            {
                FixPoint dz = position.z % HALF_GRID_SIZE;
                if (z_index % 2 == 0)
                {
                    x_index = (int)(position.x / SQRT3_GRID_SIZE);
                    FixPoint dx = position.x % SQRT3_GRID_SIZE;
                    if (dx > HALF_SQRT3_GRID_SIZE)
                    {
                        dx -= HALF_SQRT3_GRID_SIZE;
                        if (dx > dz * Sqrt3)
                        {
                            x_index += NEIGHBOUR_X_OFFSET[0, 5];
                            z_index += NEIGHBOUR_Z_OFFSET[5];
                        }
                    }
                    else
                    {
                        dx = HALF_SQRT3_GRID_SIZE - dx;
                        if (dx > dz * Sqrt3)
                        {
                            x_index += NEIGHBOUR_X_OFFSET[0, 4];
                            z_index += NEIGHBOUR_Z_OFFSET[4];
                        }
                    }
                }
                else
                {
                    x_index = (int)((position.x - HALF_SQRT3_GRID_SIZE) / SQRT3_GRID_SIZE);
                    FixPoint dx = (position.x - HALF_SQRT3_GRID_SIZE) % SQRT3_GRID_SIZE;
                    if (position.x < HALF_SQRT3_GRID_SIZE)
                    {
                        x_index += NEIGHBOUR_X_OFFSET[1, 5];
                        z_index += NEIGHBOUR_Z_OFFSET[5];
                    }
                    else if (dx > HALF_SQRT3_GRID_SIZE)
                    {
                        dx -= HALF_SQRT3_GRID_SIZE;
                        if (dx > dz * Sqrt3)
                        {
                            x_index += NEIGHBOUR_X_OFFSET[1, 5];
                            z_index += NEIGHBOUR_Z_OFFSET[5];
                        }
                    }
                    else
                    {
                        dx = HALF_SQRT3_GRID_SIZE - dx;
                        if (dx > dz * Sqrt3)
                        {
                            x_index += NEIGHBOUR_X_OFFSET[1, 4];
                            z_index += NEIGHBOUR_Z_OFFSET[4];
                        }
                    }
                }
            }
            else
            {
                if (z_index % 2 == 1)
                    position.x -= HALF_SQRT3_GRID_SIZE;
                x_index = (int)(position.x / SQRT3_GRID_SIZE);
            }
        }

        public override Vector3FP Node2Position(GridNode node)
        {
            if (node.m_z % 2 == 0)
                return new Vector3FP(SQRT3_GRID_SIZE * (FixPoint)node.m_x + m_left_bottom_position.x + HALF_SQRT3_GRID_SIZE, node.m_height, (FixPoint)(3 * node.m_z / 2 + 1) * GRID_SIZE + m_left_bottom_position.z);
            else
                return new Vector3FP(SQRT3_GRID_SIZE * (FixPoint)(node.m_x + 1) + m_left_bottom_position.x, node.m_height, (FixPoint)(3 * node.m_z + 2) / FixPoint.Two * GRID_SIZE + m_left_bottom_position.z);
        }

        protected override GridNode GetNeighbourNode(GridNode node, int direction_index)
        {
            return GetNode(node.m_x + NEIGHBOUR_X_OFFSET[node.m_z % 2, direction_index], node.m_z + NEIGHBOUR_Z_OFFSET[direction_index]);
        }

        protected override int CalculateHCost(GridNode start_node, GridNode end_node)
        {
            int dx = start_node.m_x - end_node.m_x;
            if (dx < 0)
                dx = -dx;
            int dz = start_node.m_z - end_node.m_z;
            if (dz < 0)
                dz = -dz;
            return (dx + dz) * 100;
        }

        protected override void SmoothPath(Vector3FP start_pos, Vector3FP end_pos)
        {
            for (int i = 0; i < m_node_path.Count; ++i)
                m_position_path.Add(Node2Position(m_node_path[i]));
        }        

        protected override void ChangeArea(Vector3FP center, Vector3FP extent, sbyte fill_count)
        {
            Vector3FP downleft = center - extent;
            Vector3FP upright = center + extent;
            if (m_seeker_radius > 0)
            {
                downleft.x -= m_seeker_radius;
                downleft.z -= m_seeker_radius;
                upright.x += m_seeker_radius - FixPoint.PrecisionFP;
                upright.z += m_seeker_radius - FixPoint.PrecisionFP;
            }
            GridNode min_node = Position2NearestNode(downleft);
            GridNode max_node = Position2NearestNode(upright);
            int minz = System.Math.Max(min_node.m_z - 1, 0);
            int maxz = System.Math.Min(max_node.m_z + 1, MAX_Z_INDEX);
            int minx = System.Math.Max(min_node.m_x - 1, 0);
            int maxx = System.Math.Min(max_node.m_x + 1, MAX_X_INDEX);
            GridNode node;
            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    node = GetNodeUncheck(x, z);
                    if (node != null)
                    {
                        if (CalculateOverlapArea(node, ref downleft, ref upright) > OVERLAP_MIN_AREA)
                            node.m_fill_count += fill_count;
                    }
                }
            }
            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    node = GetNodeUncheck(x, z);
                    if (node != null)
                        CalculateConnection(node);
                }
            }
        }

        FixPoint CalculateOverlapArea(GridNode node, ref Vector3FP downleft, ref Vector3FP upright)
        {
            Vector3FP center = Node2Position(node);
            FixPoint minx = FixPoint.Max(downleft.x, center.x - HALF_SQRT3_GRID_SIZE);
            FixPoint maxx = FixPoint.Min(upright.x, center.x + HALF_SQRT3_GRID_SIZE);
            if (minx > maxx)
                return FixPoint.Zero;
            FixPoint minz = FixPoint.Max(downleft.z, center.z - THREE_QUARTERS_GRID_SIZE);
            FixPoint maxz = FixPoint.Min(upright.z, center.z + THREE_QUARTERS_GRID_SIZE);
            if (minz > maxz)
                return FixPoint.Zero;
            return (maxx - minx) * (maxz - minz);
        }

        public override void GenerateAsPlaneMap(FixPoint grid_size, FixPoint max_size_x, FixPoint max_size_z, FixPoint height, Vector3FP left_bottom_position, FixPoint seeker_radius)
        {
            GRID_SIZE = grid_size;
            m_left_bottom_position = left_bottom_position;
            m_seeker_radius = seeker_radius;
            HALF_GRID_SIZE = GRID_SIZE / FixPoint.Two;
            SQRT3_GRID_SIZE = GRID_SIZE * Sqrt3;
            HALF_SQRT3_GRID_SIZE = GRID_SIZE * HalfSqrt3;
            THREE_QUARTERS_GRID_SIZE = GRID_SIZE * (FixPoint)3 / (FixPoint)4;
            OVERLAP_MIN_AREA = (FixPoint)3 * HALF_SQRT3_GRID_SIZE * GRID_SIZE / (FixPoint)6;

            m_odd_x_count = (int)(max_size_x / SQRT3_GRID_SIZE);
            m_even_x_count = (int)((max_size_x - HALF_SQRT3_GRID_SIZE) / SQRT3_GRID_SIZE);
            GRID_X_COUNT = m_odd_x_count;
            GRID_Z_COUNT = (int)((FixPoint.Two * max_size_z - GRID_SIZE) / (FixPoint.FixPointDigit[3] * GRID_SIZE));
            MAX_X_INDEX = GRID_X_COUNT - 1;
            MAX_Z_INDEX = GRID_Z_COUNT - 1;

            m_nodes = new GridNode[GRID_X_COUNT, GRID_Z_COUNT];
            GridNode node = null;
            byte connection = 0xFF;
            for (int z = 0; z < GRID_Z_COUNT; ++z)
            {
                int x_count;
                if (z % 2 ==0)
                    x_count = m_odd_x_count;
                else
                    x_count = m_even_x_count;
                for (int x = 0; x < x_count; ++x)
                    m_nodes[x, z] = new GridNode(x, z, height, connection, 0, 0);
            }
            for (int x = 0; x < GRID_X_COUNT; ++x)
            {
                node = m_nodes[x, 0];
                if (node != null)
                    CalculateConnection(node);
                node = m_nodes[x, MAX_Z_INDEX];
                if (node != null)
                    CalculateConnection(node);
            }
            for (int z = 0; z < GRID_Z_COUNT; ++z)
            {
                node = m_nodes[0, z];
                if (node != null)
                    CalculateConnection(node);
                if (z % 2 == 0)
                    node = m_nodes[m_odd_x_count - 1, z];
                else
                    node = m_nodes[m_even_x_count - 1, z];
                if (node != null)
                    CalculateConnection(node);
            }
        }
    }
}