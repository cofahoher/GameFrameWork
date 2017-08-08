using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class MovementAlongPath : IMovementProvider
    {
        const int FIRST_VALID_WAYPOINT_INDEX = 1;
        const int INVALID_WAYPOINT_INDEX = -1;

        IMovementCallback m_callback = null;
        PositionComponent m_position_component;
        FixPoint m_max_speed = FixPoint.Zero;
        FixPoint m_remain_time = FixPoint.Zero;
        List<Vector3FP> m_path = new List<Vector3FP>();
        int m_cur_way_point = INVALID_WAYPOINT_INDEX;
        Vector3FP m_direction;
        PositionComponent m_target;
        FixPoint m_range;

        public void Reset()
        {
            m_callback = null;
            m_position_component = null;
            m_max_speed = FixPoint.Zero;
            m_remain_time = FixPoint.Zero;
            m_path.Clear();
            m_cur_way_point = INVALID_WAYPOINT_INDEX;
            m_direction.MakeZero();
            m_target = null;
            m_range = FixPoint.Zero;
        }

        public void SetCallback(IMovementCallback callback)
        {
            m_callback = callback;
            ILogicOwnerInfo owner_info = callback.GetOwnerInfo();
            m_position_component = owner_info.GetOwnerEntity().GetComponent(PositionComponent.ID) as PositionComponent;
        }

        public void SetMaxSpeed(FixPoint max_speed)
        {
            m_remain_time = m_remain_time * m_max_speed / max_speed;
            m_max_speed = max_speed;
        }

        public void MoveByDirection(Vector3FP direction) { }

        public void MoveAlongPath(List<Vector3FP> path)
        {
            m_target = null;
            //不可以保存path
            m_path.Clear();
            for (int i = 0; i < path.Count; ++i)
                m_path.Add(path[i]);
            m_cur_way_point = 0;
            AdvanceWayPoint();
        }

        public void Update(FixPoint delta_time)
        {
            if (m_cur_way_point == INVALID_WAYPOINT_INDEX)
                return;
            if (delta_time > m_remain_time)
            {
                Vector3FP new_position = m_position_component.CurrentPosition + m_direction * m_max_speed * m_remain_time;
                delta_time -= m_remain_time;
                AdvanceWayPoint();
                if (m_cur_way_point != INVALID_WAYPOINT_INDEX)
                {
                    new_position += m_direction * m_max_speed * delta_time;
                    m_remain_time -= delta_time;
                }
                m_position_component.CurrentPosition = new_position;
            }
            else
            {
                Vector3FP new_position = m_position_component.CurrentPosition + m_direction * m_max_speed * delta_time;
                m_remain_time -= delta_time;
                m_position_component.CurrentPosition = new_position;
            }
            if (m_target != null)
            {
                FixPoint distance = m_position_component.CurrentPosition.FastDistance(m_target.CurrentPosition) - m_position_component.Radius - m_target.Radius;  //ZZWTODO 多处距离计算
                if (distance < m_range)
                    m_callback.MovementFinished();
            }
        }

        void AdvanceWayPoint()
        {
            m_cur_way_point += 1;
            if (m_cur_way_point > m_path.Count - 1)
            {
                m_cur_way_point = INVALID_WAYPOINT_INDEX;
                m_callback.MovementFinished();
                return;
            }
            m_direction = m_path[m_cur_way_point] - m_path[m_cur_way_point - 1];
            FixPoint distance = m_direction.Normalize();
            m_remain_time = distance / m_max_speed;
            m_position_component.SetFacing(m_direction);
        }

        public void FinishMovementWhenTargetInRange(PositionComponent target, FixPoint range)
        {
            m_target = target;
            m_range = range;
        }
    }
}