using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    public partial class PredictLogicComponent : RenderEntityComponent
    {
        readonly float m_frame_time = SyncParam.FRAME_TIME / 1000f;
        readonly float m_follow_logic_predict_time = 9999f;

        public const int NoCopy = 0;
        public const int WaitCopy = 1;
        public const int DoCopy = 2;
        //配置数据
        const float m_max_predict_time = 0.5f;
        const float m_interpolation_speed = 0.5f;
        const float m_min_threshold = 0.2f;
        const float m_max_threshold = 3f;
        //运行数据
        Transform m_interpolation_tr;
        ModelComponent m_model_component;
        PositionComponent m_position_component;
        LocomotorComponent m_locomotor_component;
        List<MovementPredict> m_movement_predicts = new List<MovementPredict>();
        int m_block_locomotor_predict = 0;
        int m_copy_state = NoCopy;
        Vector3 m_accumulated_offset = Vector3.zero;
        Vector3 m_offset_dir = Vector3.zero;
        float m_interpolation_time = -1f;

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            m_model_component = ParentObject.GetComponent(ModelComponent.ID) as ModelComponent;
            if (m_model_component == null)
                return;
            GameObject go = m_model_component.GetUnityGameObject();
            if (go == null)
                return;
            m_interpolation_tr = go.transform;
            m_model_component.SetPredictComponent(this);
            Entity entity = GetLogicEntity();
            m_position_component = entity.GetComponent(PositionComponent.ID) as PositionComponent;
            m_locomotor_component = entity.GetComponent(LocomotorComponent.ID) as LocomotorComponent;
        }

        protected override void OnDestruct()
        {
            for (int i = 0; i < m_movement_predicts.Count; ++i)
            {
                MovementPredict predict = m_movement_predicts[i];
                RecyclableObject.Recycle(predict);
            }
            m_movement_predicts.Clear();
        }

        public void ClearAllPrediction()
        {
            m_interpolation_tr.localPosition = Vector3.zero;
            m_copy_state = NoCopy;
            m_accumulated_offset = Vector3.zero;
            m_offset_dir = Vector3.zero;
            m_interpolation_time = -1f;
            for (int i = 0; i < m_movement_predicts.Count; ++i)
            {
                MovementPredict predict = m_movement_predicts[i];
                RecyclableObject.Recycle(predict);
            }
            m_movement_predicts.Clear();
        }
        #endregion

        #region GETTER
        public bool HasMovementPredict
        {
            get { return m_copy_state != DoCopy && m_movement_predicts.Count > 0; }
        }

        public LocomotorComponent GetLocomotorComponent()
        {
            return m_locomotor_component;
        }

        public bool CanPredictLocomotor
        {
            get { return m_block_locomotor_predict <= 0; }
        }
        #endregion

        public void PredictCommand(Command cmd)
        {
            switch (cmd.Type)
            {
            case CommandType.EntityMove:
                PredictEntityMove(cmd as EntityMoveCommand);
                break;
            //case CommandType.EntityAttack:
            //    ++m_block_locomotor_predict;
            //    break;
            default:
                break;
            }
        }

        public void ConfirmCommand(Command cmd, bool result)
        {
            switch (cmd.Type)
            {
            case CommandType.EntityMove:
                ConfirmEntityMove(cmd as EntityMoveCommand, result);
                break;
            //case CommandType.EntityAttack:
            //    --m_block_locomotor_predict;
            //    break;
            default:
                break;
            }
        }

        #region 移动预测
        public void OnLogicUpdatePosition(Vector3 offset)
        {
            if (m_copy_state == DoCopy/* || m_movement_predicts.Count == 0*/)
            {
                Interpolate();
                return;
            }

            bool can_interpolate = false;
            bool done_offset = false;
            for (int i = 0; i < m_movement_predicts.Count; ++i)
            {
                MovementPredict predict = m_movement_predicts[i];
                if (!can_interpolate)
                {
                    if (predict.m_state == MovementPredict.AccumulateOffsetState || predict.m_state == MovementPredict.FollowLogicState)
                        can_interpolate = true;
                }
                if (!done_offset)
                {
                    if (predict.m_state == MovementPredict.FollowLogicState || predict.m_state == MovementPredict.EliminateOffsetState)
                    {
                        done_offset = true;
                        predict.m_offset -= offset;
                        m_interpolation_tr.localPosition -= offset;
                        //LogWrapper.LogDebug("OnLogicUpdatePosition, predict.m_offset = ", predict.m_offset.ToString(), ", m_interpolation_tr = ", m_interpolation_tr.localPosition.ToString());
                    }
                }
            }
            if (!done_offset)
            {
                m_accumulated_offset -= offset;
                m_interpolation_tr.localPosition -= offset;
                ResetOffsetDirection();
                //LogWrapper.LogError("OnLogicUpdatePosition, UNDONE, offset = ", offset.ToString());
            }
            if (can_interpolate)
                Interpolate();
        }

        public void OnLogicMove()
        {
            m_copy_state = DoCopy;
        }

        void Interpolate()
        {
            if (m_interpolation_time < 0)
                return;
            float delta_time;
            if (m_interpolation_time < m_frame_time)
            {
                delta_time = m_interpolation_time;
                m_interpolation_time = 0;
            }
            else
            {
                delta_time = m_frame_time;
                m_interpolation_time -= m_frame_time;
            }
            Vector3 elimination = m_offset_dir * m_interpolation_speed * delta_time;
            m_accumulated_offset -= elimination;
            m_interpolation_tr.localPosition -= elimination;
        }

        void PredictEntityMove(EntityMoveCommand cmd)
        {
            switch (cmd.m_move_type)
            {
            case EntityMoveCommand.DirectionType:
                {
                    m_copy_state = NoCopy;
                    bool exist = false;
                    uint crc = CalculateMoveCommandCRC(cmd);
                    //LogWrapper.LogDebug("PredictEntityMove, DirectionType, time =", GetCurrentTime(), ", dir = ", cmd.m_vector.ToString(), ", crc = ", crc);
                    for (int i = 0; i < m_movement_predicts.Count; ++i)
                    {
                        MovementPredict predict = m_movement_predicts[i];
                        if (predict.m_command_crc == crc)
                        {
                            if (predict.m_state == MovementPredict.AccumulateOffsetState || predict.m_state == MovementPredict.FollowLogicState)
                                exist = true;
                        }
                        else if (predict.m_state == MovementPredict.AccumulateOffsetState || predict.m_state == MovementPredict.FollowLogicState)
                        {
                            predict.m_state = MovementPredict.EliminateOffsetState;
                            predict.m_task.Cancel();
                        }
                    }
                    if (!exist)
                    {
                        Vector3 direction = RenderWorld.Vector3FP_To_Vector3(cmd.m_vector);
                        PredictLocomotionTask task = RenderTask.Create<PredictLocomotionTask>();
                        task.Construct(this, direction, m_max_predict_time);
                        var task_scheduler = GetRenderWorld().GetTaskScheduler();
                        task_scheduler.Schedule(task, GetRenderWorld().CurrentTime, FixPoint.PrecisionFP);
                        MovementPredict predict = RecyclableObject.Create<MovementPredict>();
                        predict.m_state = MovementPredict.AccumulateOffsetState;
                        predict.m_command_crc = crc;
                        predict.m_task = task;
                        m_movement_predicts.Add(predict);
                        PlayMoveAnimation(direction);
                    }
                }
                break;
            case EntityMoveCommand.DestinationType:
                {
                    if (m_copy_state == NoCopy)
                        m_copy_state = WaitCopy;
                    //下面这段代码和EntityMoveCommand.StopMoving差不多
                    for (int i = 0; i < m_movement_predicts.Count; ++i)
                    {
                        MovementPredict predict = m_movement_predicts[i];
                        if (predict.m_state == MovementPredict.AccumulateOffsetState || predict.m_state == MovementPredict.FollowLogicState)
                        {
                            predict.m_state = MovementPredict.EliminateOffsetState;
                            predict.m_task.Cancel();
                        }
                    }
                    MovementPredict temp = RecyclableObject.Create<MovementPredict>();
                    temp.m_state = MovementPredict.CopyLogicState;
                    m_movement_predicts.Add(temp);
                    Vector3 direction = RenderWorld.Vector3FP_To_Vector3(cmd.m_vector) - m_model_component.GetCurrentPosition();
                    PlayMoveAnimation(direction);
                }
                break;
            case EntityMoveCommand.StopMoving:
                {
                    m_copy_state = NoCopy;
                    for (int i = 0; i < m_movement_predicts.Count; ++i)
                    {
                        MovementPredict predict = m_movement_predicts[i];
                        if (predict.m_state == MovementPredict.AccumulateOffsetState || predict.m_state == MovementPredict.FollowLogicState)
                        {
                            predict.m_state = MovementPredict.EliminateOffsetState;
                            predict.m_task.Cancel();
                        }
                    }
                    MovementPredict temp = RecyclableObject.Create<MovementPredict>();
                    temp.m_state = MovementPredict.StopState;
                    m_movement_predicts.Add(temp);
                    StopMoveAnimation();
                    //LogWrapper.LogDebug("PredictEntityMove, StopMoving, time =", GetCurrentTime());
                }
                break;
            default:
                break;
            }
        }

        void ConfirmEntityMove(EntityMoveCommand cmd, bool result)
        {
            switch (cmd.m_move_type)
            {
            case EntityMoveCommand.DirectionType:
                {
                    uint crc = CalculateMoveCommandCRC(cmd);
                    //LogWrapper.LogDebug("ConfirmEntityMove, DirectionType, time =", GetCurrentTime(), ", crc = ", crc);
                    for (int i = 0; i < m_movement_predicts.Count; )
                    {
                        MovementPredict predict = m_movement_predicts[i];
                        if (predict.m_command_crc != crc)
                        {
                            m_accumulated_offset += predict.m_offset;
                            if (predict.m_state != MovementPredict.EliminateOffsetState)
                                LogWrapper.LogError("REMOVE UNCONFIRMED PREDICT ", predict.m_command_crc, ", predict.m_offset = ", predict.m_offset.ToString());
                            ResetOffsetDirection();
                            RecyclableObject.Recycle(predict);
                            m_movement_predicts.RemoveAt(i);
                            continue;
                        }
                        else
                        {
                            if (predict.m_state == MovementPredict.AccumulateOffsetState)
                            {
                                predict.m_state = MovementPredict.FollowLogicState;
                                predict.m_task.ResetMaxPredictTime(GetRenderWorld().CurrentTime, m_follow_logic_predict_time);
                            }
                            break;
                        }
                    }
                }
                break;
            case EntityMoveCommand.DestinationType:
                {
                    if (m_copy_state == WaitCopy)
                        m_copy_state = DoCopy;
                    if (!result && m_copy_state == DoCopy)
                        StopMoveAnimation();
                    //下面这段代码和EntityMoveCommand.StopMoving差不多
                    for (int i = 0; i < m_movement_predicts.Count; )
                    {
                        MovementPredict predict = m_movement_predicts[i];
                        if (predict.m_state != MovementPredict.CopyLogicState)
                        {
                            m_accumulated_offset += predict.m_offset;
                            if (predict.m_state != MovementPredict.EliminateOffsetState)
                                LogWrapper.LogError("REMOVE UNCONFIRMED PREDICT ", predict.m_command_crc, ", predict.m_offset = ", predict.m_offset.ToString());
                            ResetOffsetDirection();
                            RecyclableObject.Recycle(predict);
                            m_movement_predicts.RemoveAt(i);
                            continue;
                        }
                        else
                        {
                            m_movement_predicts.RemoveAt(i);
                            break;
                        }
                    }
                    if (m_movement_predicts.Count == 0)
                    {
                        //LogWrapper.LogDebug("ConfirmEntityMove, BEFOREE SET offset = ", m_accumulated_offset.ToString());
                        m_accumulated_offset = m_interpolation_tr.localPosition;
                        ResetOffsetDirection();
                        //LogWrapper.LogDebug("ConfirmEntityMove, all stoped, m_accumulated_offset = ", m_accumulated_offset.ToString());
                    }
                }
                break;
            case EntityMoveCommand.StopMoving:
                {
                    //LogWrapper.LogDebug("ConfirmEntityMove, STOP, time = ", GetCurrentTime());
                    for (int i = 0; i < m_movement_predicts.Count; )
                    {
                        MovementPredict predict = m_movement_predicts[i];
                        if (predict.m_state != MovementPredict.StopState)
                        {
                            m_accumulated_offset += predict.m_offset;
                            if (predict.m_state != MovementPredict.EliminateOffsetState)
                                LogWrapper.LogError("REMOVE UNCONFIRMED PREDICT ", predict.m_command_crc, ", predict.m_offset = ", predict.m_offset.ToString());
                            ResetOffsetDirection();
                            RecyclableObject.Recycle(predict);
                            m_movement_predicts.RemoveAt(i);
                            continue;
                        }
                        else
                        {
                            m_movement_predicts.RemoveAt(i);
                            break;
                        }
                    }
                    if (m_movement_predicts.Count == 0)
                    {
                        //LogWrapper.LogDebug("ConfirmEntityMove, BEFOREE SET offset = ", m_accumulated_offset.ToString());
                        m_accumulated_offset = m_interpolation_tr.localPosition;
                        ResetOffsetDirection();
                        //LogWrapper.LogDebug("ConfirmEntityMove, all stoped, m_accumulated_offset = ", m_accumulated_offset.ToString());
                    }
                }
                break;
            default:
                break;
            }
        }

        public void AddPredictOffset(Vector3 offset)
        {
            GridGraph grid_graph = m_position_component.GetGridGraph();
            if (grid_graph != null)
            {
                Vector3 interpolation_position = m_interpolation_tr.localPosition + offset;
                Vector3 entity_position = m_model_component.GetCurrentPosition() + interpolation_position;
                Vector3FP entity_position_fp = RenderWorld.Vector3_To_Vector3FP(entity_position);
                GridNode node = grid_graph.Position2Node(entity_position_fp);
                if (node == null)
                {
                    if (!GetRenderWorld().OnEntityOutOfEdge(GetRenderEntity()))
                        return;
                }
                else if (!node.Walkable && m_locomotor_component.AvoidObstacle())
                {
                    Vector3FP offset_fp = RenderWorld.Vector3_To_Vector3FP(offset);
                    //try z
                    entity_position_fp.x -= offset_fp.x;
                    node = grid_graph.Position2Node(entity_position_fp);
                    if (node == null || !node.Walkable)
                    {
                        //try x
                        entity_position_fp.x += offset_fp.x;
                        entity_position_fp.z -= offset_fp.z;
                        node = grid_graph.Position2Node(entity_position_fp);
                        if (node == null || !node.Walkable)
                            return;
                        else
                            offset.z = 0;
                    }
                    else
                    {
                        offset.x = 0;
                    }
                }
            }

            for (int i = 0; i < m_movement_predicts.Count; ++i)
            {
                MovementPredict predict = m_movement_predicts[i];
                if (predict.m_state == MovementPredict.AccumulateOffsetState || predict.m_state == MovementPredict.FollowLogicState)
                {
                    predict.m_offset += offset;
                    m_interpolation_tr.localPosition += offset;
                    //LogWrapper.LogDebug("AddPredictOffset, predict.m_offset = ", predict.m_offset.ToString(), ", m_interpolation_tr = ", m_interpolation_tr.localPosition.ToString());
                    break;
                }
            }
        }

        uint CalculateMoveCommandCRC(EntityMoveCommand cmd)
        {
            uint crc = CRC.Calculate(cmd.m_move_type);
            crc = cmd.m_vector.GetCRC(crc);
            return crc;
        }

        void ResetOffsetDirection()
        {
            float offset_distance = m_accumulated_offset.magnitude;
            if (offset_distance < m_min_threshold)
            {
                m_offset_dir = Vector3.zero;
                m_interpolation_time = -1f;
                return;
            }
            else if (offset_distance > m_max_threshold)
            {
                m_interpolation_tr.localPosition = Vector3.zero;
                m_accumulated_offset = Vector3.zero;
                m_offset_dir = Vector3.zero;
                m_interpolation_time = -1f;
            }
            else
            {
                m_offset_dir = m_accumulated_offset;
                m_offset_dir.Normalize();
                m_interpolation_time = offset_distance / m_interpolation_speed;
            }
        }

        void PlayMoveAnimation(Vector3 direction)
        {
            if (m_locomotor_component.IsAnimationBlocked)
                return;
            m_model_component.SetBaseAngle(Mathf.Atan2(-direction.z, direction.x) * 180 / Mathf.PI);
            AnimationComponent animation_component = ParentObject.GetComponent(AnimationComponent.ID) as AnimationComponent;
            if (animation_component != null)
                animation_component.PlayerAnimation(animation_component.LocomotorAnimationName, true);
            AnimatorComponent animator_component = ParentObject.GetComponent(AnimatorComponent.ID) as AnimatorComponent;
            if (animator_component != null)
                animator_component.PlayAnimation(animator_component.LocomotorAnimationName);
        }

        void StopMoveAnimation()
        {
            if (m_locomotor_component.IsAnimationBlocked)
                return;
            AnimationComponent animation_component = ParentObject.GetComponent(AnimationComponent.ID) as AnimationComponent;
            if (animation_component != null)
                animation_component.PlayerAnimation(AnimationName.IDLE, true);
            AnimatorComponent animator_component = ParentObject.GetComponent(AnimatorComponent.ID) as AnimatorComponent;
            if (animator_component != null)
                animator_component.PlayAnimation(AnimationName.IDLE);
        }
        #endregion
    }

    class MovementPredict : IRecyclable
    {
        public const int StopState = 0;
        public const int AccumulateOffsetState = 1; //预测，积累offset
        public const int FollowLogicState = 2;      //预测，积累offset；改变逻辑位置，减少offset
        public const int EliminateOffsetState = 3;  //改变逻辑位置，减少offset
        public const int CopyLogicState = 4;        //完全根据逻辑来动

        public int m_state = AccumulateOffsetState;
        public uint m_command_crc = 0;
        public Vector3 m_offset = Vector3.zero;
        public PredictLocomotionTask m_task;

        public void Reset()
        {
            m_state = AccumulateOffsetState;
            m_command_crc = 0;
            m_offset = Vector3.zero;
            if (m_task != null)
            {
                m_task.Cancel();
                RenderTask.Recycle(m_task);
                m_task = null;
            }
        }
    }

    public class PredictLocomotionTask : Task<RenderWorld>
    {
        PredictLogicComponent m_predict_component;
        Vector3 m_direction;
        float m_remain_predict_time = -1f;

        public void Construct(PredictLogicComponent predict_component, Vector3 direction, float max_predict_time)
        {
            m_predict_component = predict_component;
            m_direction = direction;
            m_remain_predict_time = max_predict_time;
        }

        public void ResetMaxPredictTime(FixPoint current_time, float max_predict_time)
        {
            if (m_remain_predict_time < 0)
                Schedule(current_time, FixPoint.PrecisionFP);
            m_remain_predict_time = max_predict_time;
        }

        public override void OnReset()
        {
            m_predict_component = null;
            m_direction = Vector3.zero;
            m_remain_predict_time = -1f;
        }

        public override void Run(RenderWorld context, FixPoint current_time, FixPoint delta_time_fp)
        {
            bool over = false;
            float delta_time = (float)delta_time_fp;
            if (delta_time > m_remain_predict_time)
            {
                delta_time = m_remain_predict_time;
                m_remain_predict_time = -1f;
                over = true;
            }
            else
            {
                m_remain_predict_time -= delta_time;
            }
            LocomotorComponent locomotor_component = m_predict_component.GetLocomotorComponent();
            if (m_predict_component.CanPredictLocomotor && locomotor_component.IsEnable())
            {
                Vector3 offset = m_direction * (float)locomotor_component.MaxSpeed * delta_time;
                m_predict_component.AddPredictOffset(offset);
            }
            if (!over)
                Schedule(current_time, FixPoint.PrecisionFP);
        }
    }
}