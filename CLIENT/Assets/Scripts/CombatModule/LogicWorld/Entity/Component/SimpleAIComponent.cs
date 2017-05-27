using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SimpleAIComponent : EntityComponent, INeedTaskService, ISignalListener
    {
        //配置数据
        FixPoint m_guard_range = FixPoint.Ten;
        //运行数据
        TargetGatheringParam m_target_gathering_param;
        TargetingComponent m_targeting_component;
        SignalListenerContext m_listener_context;
        ComponentCommonTask m_task;
        List<Target> m_targets = new List<Target>();
        Entity m_current_enemy;

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            m_target_gathering_param = new TargetGatheringParam();
            m_target_gathering_param.m_type = TargetGatheringType.SurroundingRing;
            m_target_gathering_param.m_param1 = m_guard_range;
            m_target_gathering_param.m_fation = FactionRelation.Enemy;
            m_targeting_component = ParentObject.GetComponent(TargetingComponent.ID) as TargetingComponent;
            if (m_targeting_component == null)
                return;
            m_listener_context = SignalListenerContext.CreateForEntityComponent(GetLogicWorld().GenerateSignalListenerID(), ParentObject.ID, m_component_type_id);
            Schedule();
        }

        protected override void OnDestruct()
        {
            m_targeting_component = null;
            SignalListenerContext.Recycle(m_listener_context);
            m_listener_context = null;
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
            ClearTargets();
            m_current_enemy = null;
        }

        void ClearTargets()
        {
            for (int i = 0; i < m_targets.Count; ++i)
                RecyclableObject.Recycle(m_targets[i]);
            m_targets.Clear();
        }
        #endregion

        #region ISignalListener
        public void ReceiveSignal(ISignalGenerator generator, int signal_type, System.Object signal = null)
        {
            switch (signal_type)
            {
            case SignalType.Die:
                OnTargetDie(generator as Entity);
                break;
            default:
                break;
            }
        }

        void OnTargetDie(Entity target)
        {
            if (target != m_current_enemy)
                return;
            m_current_enemy.RemoveListener(SignalType.Die, m_listener_context.ID);
            m_current_enemy = null;
            Retarget();
            if (m_current_enemy == null)
                Schedule();
        }

        public void OnGeneratorDestroyed(ISignalGenerator generator)
        {
        }
        #endregion

        void Schedule()
        {
            if (m_task == null)
            {
                m_task = LogicTask.Create<ComponentCommonTask>();
                m_task.Construct(this);
            }
            var schedeler = GetLogicWorld().GetTaskScheduler();
            schedeler.Schedule(m_task, GetCurrentTime(), FixPoint.One, FixPoint.One);
        }

        public void OnTaskService(FixPoint delta_time)
        {
            Retarget();
            if (m_current_enemy != null)
                m_task.Cancel();
        }

        void Retarget()
        {
            TargetGatheringManager manager = GetLogicWorld().GetTargetGatheringManager();
            manager.BuildTargetList(GetOwnerEntity(), m_target_gathering_param, m_targets);
            if (m_targets.Count == 0)
                return;
            Entity new_enemy = m_targets[0].GetEntity();
            ClearTargets();
            if (new_enemy == m_current_enemy)
                return;
            if (m_current_enemy != null)
            {
                m_current_enemy.RemoveListener(SignalType.Die, m_listener_context.ID);
                m_current_enemy = null;
            }
            m_current_enemy = new_enemy;
            m_current_enemy.AddListener(SignalType.Die, m_listener_context);
            m_targeting_component.StartTargeting(m_current_enemy);
        }
    }
}