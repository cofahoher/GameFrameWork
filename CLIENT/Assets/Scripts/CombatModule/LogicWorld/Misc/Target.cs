using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum TargetType
    {
        InvalidType = 0,
        EntityType,
        PositionType,
    }

    public class Target : IRecyclable, IDestruct
    {
        LogicWorld m_logic_world;
        TargetType m_target_type = TargetType.InvalidType;
        int m_object_id = -1;
        Vector3FP m_position;

        public void Construct(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
        }

        public void Destruct()
        {
            Reset();
        }

        public void Reset()
        {
            m_logic_world = null;
            m_target_type = TargetType.InvalidType;
            m_object_id = -1;
        }

        public bool IsPositionTarget()
        {
            return m_target_type == TargetType.PositionType;
        }

        public bool IsEntityTarget()
        {
            return m_target_type == TargetType.EntityType;
        }

        #region Setter
        public void SetPositionTarget(Vector3FP position)
        {
            m_target_type = TargetType.PositionType;
            m_position = position;
        }

        public void SetEntityTarget(Entity entity)
        {
            m_target_type = TargetType.EntityType;
            m_object_id = entity.ID;
        }

        public void SetEntityTarget(int entity_id)
        {
            m_target_type = TargetType.EntityType;
            m_object_id = entity_id;
        }
        #endregion

        #region Getter
        public Entity GetEntity()
        {
            if (m_target_type == TargetType.EntityType)
                return m_logic_world.GetEntityManager().GetObject(m_object_id);
            else
                return null;
        }
        public int GetEntityID()
        {
            if (m_target_type == TargetType.EntityType)
                return m_object_id;
            return 0;
        }

        public Vector3FP GetPosition()
        {
            if (m_target_type == TargetType.EntityType)
            {
                Entity entity = m_logic_world.GetEntityManager().GetObject(m_object_id);
                if (entity == null)
                    return new Vector3FP(FixPoint.Zero, FixPoint.Zero, FixPoint.Zero);
                else
                    return entity.GetComponent<PositionComponent>().CurrentPosition;
            }
            else
            {
                return m_position;
            }
        }
        #endregion
    }
}