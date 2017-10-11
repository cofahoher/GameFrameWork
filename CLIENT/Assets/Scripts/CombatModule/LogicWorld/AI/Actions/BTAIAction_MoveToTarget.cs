using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTAIAction_MoveToTarget : BTAction
    {
        //配置数据
        FixPoint m_range = FixPoint.One;

        //运行数据

        public BTAIAction_MoveToTarget()
        {
        }

        public BTAIAction_MoveToTarget(BTAIAction_MoveToTarget prototype)
            : base(prototype)
        {
            m_range = prototype.m_range;
        }

        protected override void ResetRuntimeData()
        {
        }

        public override void ClearRunningTrace()
        {
        }

        protected override void OnActionEnter()
        {
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
            int current_target_id = (int)(m_context.GetData(BTContextKey.CurrentTargetID));
            if (current_target_id <= 0)
                return;
            Entity current_target = GetLogicWorld().GetEntityManager().GetObject(current_target_id);
            if (current_target == null)
                return;
            Entity owner_entity = GetOwnerEntity();
            if (owner_entity == null)
                return;
            PositionComponent position_cmp = owner_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            LocomotorComponent locomotor_cmp = owner_entity.GetComponent(LocomotorComponent.ID) as LocomotorComponent;
            PositionComponent target_position_cmp = current_target.GetComponent(PositionComponent.ID) as PositionComponent;

            Vector3FP direction = target_position_cmp.CurrentPosition - position_cmp.CurrentPosition;
            FixPoint distance = direction.FastLength() - target_position_cmp.Radius - position_cmp.Radius;  //ZZWTODO 多处距离计算
            if (distance <= m_range)
                return;

            PathFindingComponent pathfinding_component = owner_entity.GetComponent(PathFindingComponent.ID) as PathFindingComponent;
            if (pathfinding_component != null)
            {
                if (pathfinding_component.FindPath(target_position_cmp.CurrentPosition))
                    locomotor_cmp.GetMovementProvider().FinishMovementWhenTargetInRange(target_position_cmp, m_range);
            }
            else
            {
                List<Vector3FP> path = new List<Vector3FP>();
                path.Add(position_cmp.CurrentPosition);
                path.Add(target_position_cmp.CurrentPosition);
                locomotor_cmp.MoveAlongPath(path, false);
                locomotor_cmp.GetMovementProvider().FinishMovementWhenTargetInRange(target_position_cmp, m_range);
            }
        }

        protected override void OnActionExit()
        {
        }
    }
}