using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class LocomotorComponent : EntityComponent
    {
        public const int StartMovingReason_Command = 1;
        public const int StartMovingReason_Logic = 2;
        public const int StopMovingReason_Command = 3;
        public const int StopMovingReason_Logic = 4;

        enum MovingMode
        {
            Invalid = 0,
            ByDirection = 1,
            ByDestination = 2,
        }
        //需要备份的初始数据
        //运行数据
        FixPoint m_current_max_speed = FixPoint.Zero;
        PositionComponent m_position_component;
        MovingMode m_mode = MovingMode.Invalid;
        bool m_is_moving = false;
        Vector3FP m_direction;
        Vector3FP m_destination;
        LocomoterTask m_task;
        FixPoint m_remain_time = FixPoint.Zero;

        #region GETTER
        public bool IsMoving
        {
            get { return m_is_moving; }
        }
        #endregion

        #region 初始化/销毁
        public override void OnDeletePending()
        {
            StopMoving(StopMovingReason_Logic);
        }

        protected override void OnDestruct()
        {
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
        }
 
        protected override void PostInitializeComponent()
        {
            m_position_component = ParentObject.GetComponent<PositionComponent>();
        }
        #endregion

        #region 接口操作
        public void MoveByDirection(Vector3FP direction, int reason)
        {
            if (!IsEnable())
                return;
            m_mode = MovingMode.ByDirection;
            m_direction = direction;
            m_direction.Normalize();
            m_position_component.SetAngle(FixPoint.Radian2Degree(FixPoint.Atan2(-m_direction.z, m_direction.x)));
            StartMoving(reason);
        }

        public void MoveByDestination(Vector3FP destination, int reason)
        {
            if (!IsEnable())
                return;
            m_mode = MovingMode.ByDestination;
            m_direction = destination - m_position_component.CurrentPosition;
            FixPoint length = m_direction.Normalize();
            m_destination = destination;
            m_position_component.SetAngle(FixPoint.Radian2Degree(FixPoint.Atan2(-m_direction.z, m_direction.x)));
            m_remain_time = length / m_current_max_speed;
            StartMoving(reason);
        }

        public void StopMoving(int reason)
        {
            if (!m_is_moving)
                return;
            m_task.Cancel();
            m_is_moving = false;
            OnMovementStopped(reason);
        }
        #endregion

        #region 实现
        void StartMoving(int reason)
        {
            if (m_task == null)
            {
                m_task = LogicTask.Create<LocomoterTask>();
                m_task.Construct(this);
            }
            var schedeler = GetLogicWorld().GetTaskScheduler();
            FixPoint period = new FixPoint(SyncParam.FRAME_TIME) / FixPoint.Thousand;
            schedeler.Schedule(m_task, GetCurrentTime(), period, period);
            if (!m_is_moving)
            {
                m_is_moving = true;
                OnMovementStarted(reason);
            }
            else
            {
                OnMovementChanged();
            }
        }

        protected void OnMovementStarted(int reason)
        {
            ParentObject.SendSignal(SignalType.StartMoving);
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.StartMoving, ParentObject.ID, reason);
        }

        protected void OnMovementStopped(int reason)
        {
            ParentObject.SendSignal(SignalType.StopMoving);
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.StopMoving, ParentObject.ID, reason);
        }

        protected void OnMovementChanged()
        {
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.ChangeMoving, ParentObject.ID);
        }

        public void UpdatePosition(FixPoint delta_time)
        {
            m_position_component.CurrentPosition += m_direction * m_current_max_speed * delta_time;
            if (m_mode == MovingMode.ByDestination)
            {
                m_remain_time -= delta_time;
                if (m_remain_time <= FixPoint.Zero)
                    StopMoving(StopMovingReason_Logic);
            }
        }

        protected override void OnDisable()
        {
            StopMoving(StopMovingReason_Logic);
        }
        #endregion
    }

    class LocomoterTask : Task<LogicWorld>
    {
        LocomotorComponent m_locomotor_component;

        public void Construct(LocomotorComponent locomotor_component)
        {
            m_locomotor_component = locomotor_component;
        }

        public override void OnReset()
        {
            m_locomotor_component = null;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            m_locomotor_component.UpdatePosition(delta_time);
        }
    }
}