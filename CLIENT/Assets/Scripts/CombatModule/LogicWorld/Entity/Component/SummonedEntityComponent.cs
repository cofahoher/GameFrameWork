using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SummonedEntityComponent : EntityComponent, ISignalListener
    {
        //配置数据
        bool m_die_with_master = true;

        //运行数据
        SignalListenerContext m_listener_context;
        int m_master_id = 0;

        #region GETTER
        public int MasterID
        {
            get { return m_master_id; }
            set { m_master_id = value; }
        }
        #endregion

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            if (m_die_with_master)
                m_listener_context = SignalListenerContext.CreateForEntityComponent(GetLogicWorld().GenerateSignalListenerID(), ParentObject.ID, m_component_type_id);
        }

        protected override void OnDestruct()
        {
            if (m_listener_context != null)
            {
                if (m_master_id > 0)
                {
                    Entity master = GetLogicWorld().GetEntityManager().GetObject(m_master_id);
                    if (master != null)
                        master.RemoveListener(SignalType.Die, m_listener_context.ID);
                }
                SignalListenerContext.Recycle(m_listener_context);
                m_listener_context = null;
            }
        }

        public override void OnDeletePending()
        {
            //ZZWTODO
        }

        public override void OnResurrect()
        {
            //ZZWTODO
        }
        #endregion

        #region ISignalListener
        public void ReceiveSignal(ISignalGenerator generator, int signal_type, System.Object signal = null)
        {
            switch (signal_type)
            {
            case SignalType.Die:
                OnMasterDie(generator as Entity);
                break;
            default:
                break;
            }
        }

        void OnMasterDie(Entity master)
        {
            if (master.ID != m_master_id)
                return;
            master.RemoveListener(SignalType.Die, m_listener_context.ID);
            m_master_id = 0;
            EntityUtil.KillEntity((Entity)ParentObject, ParentObject.ID);
        }

        public void OnGeneratorDestroyed(ISignalGenerator generator)
        {
        }
        #endregion

        public void SetMaster(Entity master)
        {
            m_master_id = master.ID;
            if (m_die_with_master)
                master.AddListener(SignalType.Die, m_listener_context);
        }
    }
}