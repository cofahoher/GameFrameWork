using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTAIAction_GatherTarget : BTAction
    {
        //配置数据
        TargetGatheringParam m_target_gathering_param = new TargetGatheringParam();

        //运行数据
        List<Target> m_targets = new List<Target>();

        public BTAIAction_GatherTarget()
        {
        }

        public BTAIAction_GatherTarget(BTAIAction_GatherTarget prototype)
            : base(prototype)
        {
            m_target_gathering_param.CopyFrom(prototype.m_target_gathering_param);
        }

        void ClearTargets()
        {
            for (int i = 0; i < m_targets.Count; ++i)
                RecyclableObject.Recycle(m_targets[i]);
            m_targets.Clear();
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
            TargetGatheringManager target_gathering_manager = GetLogicWorld().GetTargetGatheringManager();
            target_gathering_manager.BuildTargetList(GetOwnerEntity(), m_target_gathering_param, m_targets);
            int current_target_id = 0;
            if (m_targets.Count > 0)
            {
                current_target_id = m_targets[0].GetEntityID();
                m_status = BTNodeStatus.True;
            }
            else
            {
                m_status = BTNodeStatus.False;
            }
            m_context.SetData(BTContextKey.CurrentTargetID, (FixPoint)current_target_id);
            ClearTargets();
        }

        protected override void OnActionExit()
        {
        }
    }
}