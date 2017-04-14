using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum TargetType
    {
        InvalidType = 0,
        PositionType,
        EntityType,
    }
    public class Target : IRecyclable, IDestruct
    {
        LogicWorld m_logic_world;
        TargetType m_target_type = TargetType.InvalidType;
        int m_object_id = -1;

        public void Construct(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
        }

        public void Reset()
        {
            m_logic_world = null;
            m_target_type = TargetType.InvalidType;
            m_object_id = -1;
        }

        public void Destruct()
        {
            Reset();
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
        public void SetPositionTarget(Vector3FP pos)
        {
            m_target_type = TargetType.PositionType;
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
                m_logic_world.GetEntityManager().GetObject(m_object_id);
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
            return Vector3FP.Zero;
        }
        #endregion
    }
}