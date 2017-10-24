using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class MovementByDirection : IMovementProvider
    {
        IMovementCallback m_callback = null;
        PositionComponent m_position_component;
        FixPoint m_max_speed;
        Vector3FP m_direction;

        public void Reset()
        {
            m_callback = null;
            m_position_component = null;
            m_max_speed = FixPoint.Zero;
            m_direction.MakeZero();
        }

        public void SetCallback(IMovementCallback callback)
        {
            m_callback = callback;
            ILogicOwnerInfo owner_info = callback.GetOwnerInfo();
            m_position_component = owner_info.GetOwnerEntity().GetComponent(PositionComponent.ID) as PositionComponent;
        }

        public void SetMaxSpeed(FixPoint max_speed)
        {
            m_max_speed = max_speed;
        }

        public void MoveByDirection(Vector3FP direction)
        {
            m_direction = direction;
            m_position_component.SetFacing(direction);
        }

        public void MoveAlongPath(List<Vector3FP> path) { }

        public void Update(FixPoint delta_time)
        {
            Vector3FP offset = m_direction * m_max_speed * delta_time;
            Vector3FP new_position = m_position_component.CurrentPosition + offset;
            GridGraph grid_graph = m_position_component.GetGridGraph();
            if (grid_graph != null)
            {
                GridNode node = grid_graph.Position2Node(new_position);
                if (node == null)
                {
                    if (!m_position_component.GetLogicWorld().OnEntityOutOfEdge(m_position_component.GetOwnerEntity(), ref new_position))
                        return;
                }
                else if (!node.Walkable && m_callback.AvoidObstacle())
                {
                    //try z
                    new_position.x -= offset.x;
                    node = grid_graph.Position2Node(new_position);
                    if (node == null || !node.Walkable)
                    {
                        //try x
                        new_position.x += offset.x;
                        new_position.z -= offset.z;
                        node = grid_graph.Position2Node(new_position);
                        if (node == null || !node.Walkable)
                            return;
                    }
                }
            }
            m_position_component.CurrentPosition = new_position;
        }

        public void FinishMovementWhenTargetInRange(PositionComponent target, FixPoint range) { }

        public Vector3FP GetCurrentDirection()
        {
            return m_direction;
        }
    }
}