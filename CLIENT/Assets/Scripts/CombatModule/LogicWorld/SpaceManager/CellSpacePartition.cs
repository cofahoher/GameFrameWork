using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Cell
    {
        public List<PositionComponent> m_entities = new List<PositionComponent>();
    }

    public class CellSpacePartition : ISpacePartition
    {
        static readonly FixPoint CELL_SIZE = FixPoint.Ten;
        static readonly FixPoint MAX_ENTITY_RADIUS = FixPoint.One;

        LogicWorld m_logic_world;
        Vector3FP m_left_bottom_position = new Vector3FP();
        Cell[,] m_cells;
        int CELL_X_COUNT = 0;
        int CELL_Z_COUNT = 0;
        int MAX_X_INDEX = 0;
        int MAX_Z_INDEX = 0;

        List<int> m_collection = new List<int>();
        int m_min_x;
        int m_max_x;
        int m_min_z;
        int m_max_z;

        public CellSpacePartition(LogicWorld logic_world, FixPoint x_size, FixPoint z_size, Vector3FP left_bottom_position)
        {
            m_logic_world = logic_world;
            m_left_bottom_position = left_bottom_position;
            CELL_X_COUNT = (int)((x_size + CELL_SIZE - FixPoint.PrecisionFP) / CELL_SIZE);
            CELL_Z_COUNT = (int)((z_size + CELL_SIZE - FixPoint.PrecisionFP) / CELL_SIZE);
            MAX_X_INDEX = CELL_X_COUNT - 1;
            MAX_Z_INDEX = CELL_Z_COUNT - 1;
            m_cells = new Cell[CELL_X_COUNT, CELL_Z_COUNT];
            for (int x = 0; x < CELL_X_COUNT; ++x)
            {
                for (int z = 0; z < CELL_Z_COUNT; ++z)
                    m_cells[x, z] = new Cell();
            }
        }

        public void Destruct()
        {
            for (int x = 0; x <= MAX_X_INDEX; ++x)
            {
                for (int z = 0; z <= MAX_Z_INDEX; ++z)
                {
                    m_cells[x, z].m_entities.Clear();
                }
            }
            m_logic_world = null;
        }

        public void AddEntiy(PositionComponent entity)
        {
            Cell cell = Position2Cell(entity.CurrentPosition);
            if (cell == null)
                return;
            cell.m_entities.Add(entity);
        }

        public void RemoveEntity(PositionComponent entity)
        {
            Cell cell = Position2Cell(entity.CurrentPosition);
            if (cell == null)
                return;
            cell.m_entities.Remove(entity);
        }

        public void UpdateEntity(PositionComponent entity, Vector3FP new_position)
        {
            Cell old_cell = Position2Cell(entity.CurrentPosition);
            Cell new_cell = Position2Cell(new_position);
            if (old_cell == new_cell)
                return;
            if (old_cell != null)
                old_cell.m_entities.Remove(entity);
            if (new_cell != null)
                new_cell.m_entities.Add(entity);
        }

        #region 帮助函数
        Cell Position2Cell(Vector3FP position)
        {
            int x_index = (int)((position.x - m_left_bottom_position.x) / CELL_SIZE);
            int z_index = (int)((position.z - m_left_bottom_position.z) / CELL_SIZE);
            return GetCell(x_index, z_index);
        }

        Cell GetCell(int x_index, int z_index)
        {
            if (x_index < 0 || x_index > MAX_X_INDEX || z_index < 0 || z_index > MAX_Z_INDEX)
                return null;
            return m_cells[x_index, z_index];
        }

        Cell GetCellUncheck(int x_index, int z_index)
        {
            return m_cells[x_index, z_index];
        }

        void ComputeAreaXZ(Vector2FP pos1, Vector2FP pos2)
        {
            pos1.x -= m_left_bottom_position.x;
            pos1.z -= m_left_bottom_position.z;
            pos2.x -= m_left_bottom_position.x;
            pos2.z -= m_left_bottom_position.z;
            if (pos1.x < pos2.x)
            {
                pos1.x -= MAX_ENTITY_RADIUS;
                pos2.x += MAX_ENTITY_RADIUS;
                m_min_x = (int)(pos1.x / CELL_SIZE);
                m_max_x = (int)(pos2.x / CELL_SIZE);
            }
            else
            {
                pos2.x -= MAX_ENTITY_RADIUS;
                pos1.x += MAX_ENTITY_RADIUS;
                m_min_x = (int)(pos2.x / CELL_SIZE);
                m_max_x = (int)(pos1.x / CELL_SIZE);
            }
            if (pos1.z < pos2.z)
            {
                pos1.z -= MAX_ENTITY_RADIUS;
                pos2.z += MAX_ENTITY_RADIUS;
                m_min_z = (int)(pos1.z / CELL_SIZE);
                m_max_z = (int)(pos2.z / CELL_SIZE);
            }
            else
            {
                pos2.z -= MAX_ENTITY_RADIUS;
                pos1.z += MAX_ENTITY_RADIUS;
                m_min_z = (int)(pos2.z / CELL_SIZE);
                m_max_z = (int)(pos1.z / CELL_SIZE);
            }
            if (m_min_x < 0)
                m_min_x = 0;
            if (m_max_x > MAX_X_INDEX)
                m_max_x = MAX_X_INDEX;
            if (m_min_z < 0)
                m_min_z = 0;
            if (m_max_z > MAX_Z_INDEX)
                m_max_z = MAX_Z_INDEX;
        }
        #endregion

        public List<int> CollectEntity_All(int exclude_id)
        {
            m_collection.Clear();
            Cell cell;
            PositionComponent cmp;
            for (int x = 0; x <= MAX_X_INDEX; ++x)
            {
                for (int z = 0; z <= MAX_Z_INDEX; ++z)
                {
                    cell = m_cells[x, z];
                    for (int i = 0; i < cell.m_entities.Count; ++i)
                    {
                        cmp = cell.m_entities[i];
                        int id = cmp.GetOwnerEntityID();
                        if (id == exclude_id)
                            continue;
                        m_collection.Add(id);
                    }
                }
            }
            return m_collection;
        }

        public List<int> CollectEntity_ForwardRectangle(Vector3FP position, Vector2FP direction, FixPoint length, FixPoint width, int exclude_id)
        {
            m_collection.Clear();
            Vector2FP start_position = new Vector2FP(position);
            Vector2FP end_position = start_position + direction * length;
            ComputeAreaXZ(start_position, end_position);
            Vector2FP side = direction.Perpendicular();
            width = width >> 1;
            Cell cell;
            PositionComponent cmp;
            Vector2FP offset;
            for (int x = m_min_x; x <= m_max_x; ++x)
            {
                for (int z = m_min_z; z <= m_max_z; ++z)
                {
                    cell = m_cells[x, z];
                    for (int i = 0; i < cell.m_entities.Count; ++i)
                    {
                        cmp = cell.m_entities[i];
                        if (cmp.GetOwnerEntityID() == exclude_id)
                            continue;
                        offset.x = cmp.CurrentPosition.x - position.x;
                        offset.z = cmp.CurrentPosition.z - position.z;
                        FixPoint component = offset.Dot(ref direction);
                        FixPoint radius = cmp.Radius;
                        if (component < -radius || component > length + radius)
                            continue;
                        component = offset.Dot(ref side);
                        if (component < FixPoint.Zero)
                            component = -component;
                        if (component > width + radius)
                            continue;
                        m_collection.Add(cmp.GetOwnerEntityID());
                    }
                }
            }
            return m_collection;
        }

        public List<int> CollectEntity_SurroundingRing(Vector3FP position, FixPoint outer_radius, FixPoint inner_radius, int exclude_id)
        {
            m_collection.Clear();
            Vector2FP start_position = new Vector2FP(position.x - outer_radius, position.z - outer_radius);
            Vector2FP end_position = new Vector2FP(position.x + outer_radius, position.z + outer_radius);
            ComputeAreaXZ(start_position, end_position);
            Cell cell;
            PositionComponent cmp;
            for (int x = m_min_x; x <= m_max_x; ++x)
            {
                for (int z = m_min_z; z <= m_max_z; ++z)
                {
                    cell = m_cells[x, z];
                    for (int i = 0; i < cell.m_entities.Count; ++i)
                    {
                        cmp = cell.m_entities[i];
                        if (cmp.GetOwnerEntityID() == exclude_id)
                            continue;
                        Vector3FP offset = position - cmp.CurrentPosition;
                        FixPoint distance = FixPoint.FastDistance(offset.x, offset.z);
                        if (distance >= (outer_radius + cmp.Radius))
                            continue;
                        if (inner_radius > FixPoint.Zero && distance <= (inner_radius - cmp.Radius))
                            continue;
                        m_collection.Add(cmp.GetOwnerEntityID());
                    }
                }
            }
            return m_collection;
        }

        public List<int> CollectEntity_ForwardSector(Vector3FP position, Vector2FP facing, FixPoint radius, FixPoint degree, int exclude_id)
        {
            FixPoint cos = FixPoint.Cos(FixPoint.Degree2Radian(degree >> 1));
            m_collection.Clear();
            Vector2FP start_position = new Vector2FP(position.x - radius, position.z - radius);
            Vector2FP end_position = new Vector2FP(position.x + radius, position.z + radius);
            ComputeAreaXZ(start_position, end_position);
            Cell cell;
            Vector2FP source = new Vector2FP(position.x, position.z);
            Vector2FP target = new Vector2FP();
            PositionComponent cmp;
            for (int x = m_min_x; x <= m_max_x; ++x)
            {
                for (int z = m_min_z; z <= m_max_z; ++z)
                {
                    cell = m_cells[x, z];
                    for (int i = 0; i < cell.m_entities.Count; ++i)
                    {
                        cmp = cell.m_entities[i];
                        if (cmp.GetOwnerEntityID() == exclude_id)
                            continue;
                        target.x = cmp.CurrentPosition.x;
                        target.z = cmp.CurrentPosition.z;
                        Vector2FP to_target = target - source;
                        FixPoint distance = to_target.FastNormalize();
                        if (distance > radius + cmp.Radius)
                            continue;
                        if (to_target.Dot(ref facing) < cos)
                            continue;
                        m_collection.Add(cmp.GetOwnerEntityID());
                    }
                }
            }
            return m_collection;
        }
    }
}