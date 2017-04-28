using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class TargetingComponent : EntityComponent, ISignalListener
    {
        SignalListenerContext m_listener_context;
        Entity m_current_target;

        #region GETTER
        public Entity GetTarget()
        {
            return m_current_target;
        }
        #endregion

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            m_listener_context = SignalListenerContext.CreateForEntityComponent(GetLogicWorld().GenerateSignalListenerID(), ParentObject.ID, m_component_type_id);
        }

        protected override void OnDestruct()
        {
            SignalListenerContext.Recycle(m_listener_context);
            m_listener_context = null;
            m_current_target = null;
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
            if (target != m_current_target)
                return;
            StopTargeting();
        }

        public void OnGeneratorDestroyed(ISignalGenerator generator)
        {
        }
        #endregion

        public void StartTargeting(Entity target)
        {
            if (m_current_target != null && target.ID == m_current_target.ID)
                return;
            StopTargeting();
            m_current_target = target;
            target.AddListener(SignalType.Die, m_listener_context);
        }

        public void StopTargeting()
        {
            if (m_current_target == null)
                return;
            m_current_target.RemoveListener(SignalType.Die, m_listener_context.ID);
            m_current_target = null;
        }
    }
}