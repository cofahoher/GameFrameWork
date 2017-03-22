using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class SkillManagerComponent : EntityComponent, ISignalListener
    {
        SignalListenerContext m_listener_context;

        #region 初始化
        protected override void PostInitializeComponent()
        {
            m_listener_context = SignalListenerContext.CreateForEntityComponent(GetLogicWorld().GenerateSignalListenerID(), ParentObject.ID, m_component_type_id);
            ParentObject.AddListener(SignalType.StartMoving, m_listener_context);
            ParentObject.AddListener(SignalType.StopMoving, m_listener_context);
        }

        public override void OnDestruct()
        {
            ParentObject.RemoveListener(SignalType.StartMoving, m_listener_context.ID);
            ParentObject.RemoveListener(SignalType.StopMoving, m_listener_context.ID);
        }
        #endregion

        #region ISignalListener
        public void ReceiveSignal(ISignalGenerator generator, int signal_type, Signal signal = null)
        {
            switch (signal_type)
            {
            case SignalType.StartMoving:
                break;
            case SignalType.StopMoving:
                break;
            default:
                break;
            }
        }

        public void OnGeneratorDestroyed(ISignalGenerator generator)
        {
        }
        #endregion
    }
}