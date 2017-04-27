using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class SquareGridGraph : GridGraph
    {
        /*
         *          Z
         *          |
         *          |
         *       6  2  5
         *        \ | /
         * ----3 -- N -- 1---- X
         *        / | \
         *       7  0  4
         *          |
         *          |
         */
        const int SQUARE_GRID_NEIGHBOUR_COUNT = 8;
        static readonly int[] NEIGHBOUR_X_OFFSET = new int[SQUARE_GRID_NEIGHBOUR_COUNT] { 0, 1, 0, -1, 1, 1, -1, -1 };
        static readonly int[] NEIGHBOUR_Z_OFFSET = new int[SQUARE_GRID_NEIGHBOUR_COUNT] { -1, 0, 1, 0, -1, 1, 1, -1 };
        static readonly int[,] XZ_OFFSET_TO_DIRECTION = new int[3, 3] { { 7, 3, 6 }, { 0, 0, 2 }, { 4, 1, 5 } };
        const int SOUTH = 0;
        const int EAST = 1;
        const int NORTH = 2;
        const int WEST = 3;
        const int SOUTH_EAST = 4;
        const int NORTH_EAST = 5;
        const int NORTH_WEST = 6;
        const int SOUTH_WEST = 7;
        FixPoint HALF_GRID_SIZE;

        public SquareGridGraph()
        {
            NEIGHBOUR_COUNT = SQUARE_GRID_NEIGHBOUR_COUNT;
            NEIGHBOUR_COST = new int[SQUARE_GRID_NEIGHBOUR_COUNT] { 100, 100, 100, 100, 141, 141, 141, 141 };
        }

        public override int GetGraphType()
        {
            return GridGraph.SqureNodeType;
        }

        public override void GenerateFromData()
        {
            //ZZWTODO
        }

        public override GridNode Position2Node(Vector3FP position)
        {
            int x_index = (int)((position.x - m_left_bottom_position.x) / GRID_SIZE);
            int z_index = (int)((position.z - m_left_bottom_position.z) / GRID_SIZE);
            return GetNode(x_index, z_index);
        }

        public override GridNode Position2NearestNode(Vector3FP position)
        {
            int x_index = (int)((position.x - m_left_bottom_position.x) / GRID_SIZE);
            if (x_index < 0)
                x_index = 0;
            else if (x_index > MAX_X_INDEX)
                x_index = MAX_X_INDEX;
            int z_index = (int)((position.z - m_left_bottom_position.z) / GRID_SIZE);
            if (z_index < 0)
                z_index = 0;
            else if (z_index > MAX_Z_INDEX)
                z_index = MAX_Z_INDEX;
            return GetNodeUncheck(x_index, z_index);
        }

        public override Vector3FP Node2Position(GridNode node)
        {
            return new Vector3FP((FixPoint)node.m_x * GRID_SIZE + HALF_GRID_SIZE + m_left_bottom_position.x, node.m_height, (FixPoint)node.m_z * GRID_SIZE + HALF_GRID_SIZE + m_left_bottom_position.z);
        }

        protected override GridNode GetNeighbourNode(GridNode node, int direction_index)
        {
            return GetNode(node.m_x + NEIGHBOUR_X_OFFSET[direction_index], node.m_z + NEIGHBOUR_Z_OFFSET[direction_index]);
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

            //int dx = (start_node.m_x - end_node.m_x) * 100;
            //if (dx < 0) dx = -dx;
            //int dz = (start_node.m_z - end_node.m_z) * 100;
            //if (dz < 0) dz = -dz;
            //int min_val = dx;
            //if (dz < min_val) min_val = dz;
            //return dx + dz - (min_val >> 1) - (min_val >> 2) + (min_val >> 4);
        }

        protected override void SmoothPath(Vector3FP start_pos, Vector3FP end_pos)
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
                while (index < m_node_path.Count)
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
            for (int ix = 0, iz = 0; ix < nx || iz < nz; )
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
                    current_node = GetNodeUncheck(x, z);
                }
                else if (temp < 0)
                {
                    if (!current_node.HasConnection(XZ_OFFSET_TO_DIRECTION[sign_x + 1, 1]))
                        return false;
                    x += sign_x;
                    ++ix;
                    current_node = GetNodeUncheck(x, z);
                }
                else
                {
                    if (!current_node.HasConnection(XZ_OFFSET_TO_DIRECTION[1, sign_z + 1]))
                        return false;
                    z += sign_z;
                    ++iz;
                    current_node = GetNodeUncheck(x, z);
                }
            }
            return true;
        }

        protected override void ChangeArea(Vector3FP center, Vector3FP extent, sbyte fill_count)
        {
            Vector3FP downleft = center - extent;
            downleft.x -= m_seeker_radius;
            downleft.z -= m_seeker_radius;
            Vector3FP upright = center + extent;
            upright.x += m_seeker_radius - FixPoint.PrecisionFP;
            upright.z += m_seeker_radius - FixPoint.PrecisionFP;
            GridNode min_node = Position2NearestNode(downleft);
            GridNode max_node = Position2NearestNode(upright);
            GridNode node;
            for (int z = min_node.m_z; z <= max_node.m_z; ++z)
            {
                for (int x = min_node.m_x; x <= max_node.m_x; ++x)
                {
                    node = GetNodeUncheck(x, z);
                    if(node != null)
                        node.m_fill_count += fill_count;
                }
            }
            for (int z = System.Math.Max(min_node.m_z - 1, 0); z <= System.Math.Min(max_node.m_z + 1, MAX_Z_INDEX); ++z)
            {
                for (int x = System.Math.Max(min_node.m_x - 1, 0); x <= System.Math.Min(max_node.m_x + 1, MAX_X_INDEX); ++x)
                {
                    node = GetNodeUncheck(x, z);
                    if (node != null)
                        CalculateConnection(node);
                }
            }
        }

        public override void GenerateAsPlaneMap(FixPoint grid_size, FixPoint max_size_x, FixPoint max_size_z, FixPoint height, Vector3FP left_bottom_position, FixPoint seeker_radius)
        {
            GRID_SIZE = grid_size;
            m_left_bottom_position = left_bottom_position;
            m_seeker_radius = seeker_radius;
            HALF_GRID_SIZE = GRID_SIZE / FixPoint.Two;

            GRID_X_COUNT = (int)((max_size_x + grid_size - FixPoint.PrecisionFP) / grid_size);
            GRID_Z_COUNT = (int)((max_size_z + grid_size - FixPoint.PrecisionFP) / grid_size);
            MAX_X_INDEX = GRID_X_COUNT - 1;
            MAX_Z_INDEX = GRID_Z_COUNT - 1;

            m_nodes = new GridNode[GRID_X_COUNT, GRID_Z_COUNT];
            GridNode node = null;
            byte connection = 0xFF;
            for (int x = 0; x < GRID_X_COUNT; ++x)
            {
                for (int z = 0; z < GRID_Z_COUNT; ++z)
                    m_nodes[x, z] = new GridNode(x, z, height, connection, 0, 0);
            }
            for (int x = 0; x < GRID_X_COUNT; ++x)
            {
                //down
                node = m_nodes[x, 0];
                node.m_connection = 0;
                node.m_fill_count = 1;
                //up
                node = m_nodes[x, MAX_Z_INDEX];
                node.m_connection = 0;
                node.m_fill_count = 1;
            }
            for (int z = 0; z < GRID_Z_COUNT; ++z)
            {
                //left
                node = m_nodes[0, z];
                node.m_connection = 0;
                node.m_fill_count = 1;
                //right
                node = m_nodes[MAX_X_INDEX, z];
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
            for (int x = 1; x < MAX_X_INDEX; ++x)
            {
                //down
                node = m_nodes[x, 1];
                node.m_connection = down_connection;
                //up
                node = m_nodes[x, MAX_Z_INDEX - 1];
                node.m_connection = up_connection;
            }
            for (int z = 1; z < MAX_Z_INDEX; ++z)
            {
                //left
                node = m_nodes[1, z];
                node.m_connection = left_connection;
                //right
                node = m_nodes[MAX_X_INDEX - 1, z];
                node.m_connection = rigth_connection;
            }
            m_nodes[1, 1].m_connection = downleft_connection;
            m_nodes[MAX_X_INDEX - 1, 1].m_connection = downright_connection;
            m_nodes[1, MAX_Z_INDEX - 1].m_connection = upleft_connection;
            m_nodes[MAX_X_INDEX - 1, MAX_Z_INDEX - 1].m_connection = upright_connection;
        }
    }
}