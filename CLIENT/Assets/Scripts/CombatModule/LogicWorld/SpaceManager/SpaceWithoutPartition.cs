using System.Collections;
using System.Collections.Generic;
namespace Combat
{

    public class SpaceWithoutPartition : ISpacePartition
    {
        LogicWorld m_logic_world;
        public List<PositionComponent> m_entities = new List<PositionComponent>();
        List<int> m_collection = new List<int>();

        public SpaceWithoutPartition(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
        }

        public void Destruct()
        {
            m_entities.Clear();
            m_logic_world = null;
        }

        public void AddEntiy(PositionComponent entity)
        {
            m_entities.Add(entity);
        }

        public void RemoveEntity(PositionComponent entity)
        {
            m_entities.Remove(entity);
        }

        public void UpdateEntity(PositionComponent entity, Vector3FP new_position)
        {
        }

        public List<int> CollectEntity_All(int exclude_id)
        {
            m_collection.Clear();
            PositionComponent cmp;
            for (int i = 0; i < m_entities.Count; ++i)
            {
                cmp = m_entities[i];
                int id = cmp.GetOwnerEntityID();
                if (id == exclude_id)
                    continue;
                m_collection.Add(id);
            }
            return m_collection;
        }

        public List<int> CollectEntity_ForwardRectangle(Vector3FP position, Vector2FP direction, FixPoint length, FixPoint width, int exclude_id)
        {
            m_collection.Clear();
            Vector2FP side = direction.Perpendicular();
            width = width >> 1;
            PositionComponent cmp;
            Vector2FP offset;
            for (int i = 0; i < m_entities.Count; ++i)
            {
                cmp = m_entities[i];
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
            return m_collection;
        }

        public List<int> CollectEntity_SurroundingRing(Vector3FP position, FixPoint outer_radius, FixPoint inner_radius, int exclude_id)
        {
            m_collection.Clear();
            PositionComponent cmp;
            for (int i = 0; i < m_entities.Count; ++i)
            {
                cmp = m_entities[i];
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
            return m_collection;
        }

        public List<int> CollectEntity_ForwardSector(Vector3FP position, Vector2FP facing, FixPoint radius, FixPoint degree, int exclude_id)
        {
            FixPoint cos = FixPoint.Cos(FixPoint.Degree2Radian(degree >> 1));
            m_collection.Clear();
            Vector2FP source = new Vector2FP(position.x, position.z);
            Vector2FP target = new Vector2FP();
            PositionComponent cmp;
            for (int i = 0; i < m_entities.Count; ++i)
            {
                cmp = m_entities[i];
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
            return m_collection;
        }
    }
}