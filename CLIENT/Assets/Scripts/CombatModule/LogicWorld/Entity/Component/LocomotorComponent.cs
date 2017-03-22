using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class LocomotorComponent : EntityComponent
    {
        enum MovingMode
        {
            Invalid = 0,
            ByDirection = 1,
            ByDestination = 2,
        }
        //配置数据
        int m_max_speed = 0;
        //运行数据
        PositionComponent m_position_component;
        MovingMode m_mode = MovingMode.Invalid;
        bool m_is_moving = false;
        Vector3I m_direction;
        Vector3I m_destination;
        LocomoterTask m_task;
        int m_remain_time = 0;

        #region GETTER
        public int MaxSpeed
        {
            get { return m_max_speed; }
        }
        public bool IsMoving
        {
            get { return m_is_moving; }
        }
        #endregion

        #region 初始化
        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("max_speed", out value))
                m_max_speed = int.Parse(value);
        }

        protected override void PostInitializeComponent()
        {
            m_position_component = ParentObject.GetComponent<PositionComponent>();
        }
        #endregion

        #region 接口操作
        public void MoveByDirection(Vector3I direction)
        {
            if (!IsEnable())
                return;
            m_mode = MovingMode.ByDirection;
            m_direction = direction;
            m_direction.Normalize();
            m_position_component.SetAngle(IntMath.XZToDegree(m_direction.x, -m_direction.z));
            StartMoving();
        }

        public void MoveByDestination(Vector3I destination)
        {
            if (!IsEnable())
                return;
            m_mode = MovingMode.ByDestination;
            m_direction = destination - m_position_component.CurrentPosition;
            int length = m_direction.Normalize();
            m_destination = destination;
            m_position_component.SetAngle(IntMath.XZToDegree(m_direction.x, -m_direction.z));
            m_remain_time = length * 1000 / m_max_speed;
            StartMoving();
        }

        public void StopMoving()
        {
            if (!m_is_moving)
                return;
            m_task.Cancel();
            m_is_moving = false;
            OnMovementStopped();
        }
        #endregion

        #region 实现
        void StartMoving()
        {
            if (m_task == null)
                m_task = new LocomoterTask(this);
            var schedeler = GetLogicWorld().GetTaskScheduler();
            schedeler.Schedule(m_task, GetCurrentTime(), SyncParam.FRAME_TIME, SyncParam.FRAME_TIME);
            if (!m_is_moving)
            {
                m_is_moving = true;
                OnMovementStarted();
            }
        }

        protected void OnMovementStarted()
        {
            ParentObject.SendSignal(SignalType.StartMoving);
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.StartMoving, ParentObject.ID);
        }

        protected void OnMovementStopped()
        {
            ParentObject.SendSignal(SignalType.StopMoving);
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.StopMoving, ParentObject.ID);
        }

        public void UpdatePosition(int delta_ms)
        {
            m_position_component.CurrentPosition += m_direction * m_max_speed * delta_ms / 100000;
            if (m_mode == MovingMode.ByDestination)
            {
                m_remain_time -= delta_ms;
                if (m_remain_time <= 0)
                    StopMoving();
            }
        }

        protected override void OnDisable()
        {
            StopMoving();
        }
        #endregion
    }


    class LocomoterTask : Task<LogicWorld>
    {
        LocomotorComponent m_locomotor_component;
        public LocomoterTask(LocomotorComponent locomotor_component)
        {
            m_locomotor_component = locomotor_component;
        }

        public override void Run(LogicWorld logic_world, int current_time, int delta_time)
        {
            m_locomotor_component.UpdatePosition(delta_time);
        }
    }
}