using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class MovementByDirection : IMovementProvider
    {
        IMovementCallback m_callback = null;
        GridGraph m_grid_graph;
        PositionComponent m_position_component;
        FixPoint m_max_speed;
        Vector3FP m_direction;

        public void Reset()
        {
            m_callback = null;
            m_grid_graph = null;
            m_position_component = null;
            m_max_speed = FixPoint.Zero;
            m_direction.MakeZero();
        }

        public void SetCallback(IMovementCallback callback)
        {
            m_callback = callback;
            ILogicOwnerInfo owner_info = callback.GetOwnerInfo();
            m_grid_graph = owner_info.GetLogicWorld().GetGridGraph();
            m_position_component = owner_info.GetOwnerEntity().GetComponent(PositionComponent.ID) as PositionComponent;
        }

        public void SetMaxSpeed(FixPoint max_speed)
        {
            m_max_speed = max_speed;
        }

        public void MoveByDirection(Vector3FP direction)
        {
            m_direction = direction;
            m_position_component.SetAngle(FixPoint.XZToUnityRotationDegree(m_direction.x, m_direction.z));
        }

        public void MoveAlongPath(List<Vector3FP> path) { }

        public void Update(FixPoint delta_time)
        {
            Vector3FP new_position = m_position_component.CurrentPosition + m_direction * m_max_speed * delta_time;
            if (m_grid_graph != null)
            {
                GridNode node = m_grid_graph.Position2Node(new_position);
                if (node == null)
                    return;
                if (!node.Walkable && m_callback.AvoidObstacle())
                    return;
            }
            m_position_component.CurrentPosition = new_position;
        }

        public void FinishMovementWhenTargetInRange(PositionComponent target, FixPoint range) { }
    }
}