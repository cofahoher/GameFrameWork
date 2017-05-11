using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public abstract class Component : ILogicOwnerInfo, IDestruct, IExpressionVariableProvider
    {
        static public readonly FixPoint LOGIC_UPDATE_INTERVAL = new FixPoint(SyncParam.FRAME_TIME) / FixPoint.Thousand;

        protected Object m_parent_object;
        protected int m_component_type_id = -1;
        protected int m_disable_count = 0;

        #region GETTER
        public Object ParentObject
        {
            get { return m_parent_object; }
            set { m_parent_object = value; }
        }
        public int ComponentTypeID
        {
            get { return m_component_type_id; }
            set { m_component_type_id = value; }
        }
        #endregion

        #region ILogicOwnerInfo
        public LogicWorld GetLogicWorld()
        {
            return m_parent_object.GetLogicWorld();
        }
        public FixPoint GetCurrentTime()
        {
            return m_parent_object.GetCurrentTime();
        }
        public int GetOwnerObjectID()
        {
            return m_parent_object.ID;
        }
        public Object GetOwnerObject()
        {
            return m_parent_object;
        }
        public virtual int GetOwnerPlayerID()
        {
            return m_parent_object.GetOwnerPlayerID();
        }
        public virtual Player GetOwnerPlayer()
        {
            return m_parent_object.GetOwnerPlayer();
        }
        public virtual int GetOwnerEntityID()
        {
            return m_parent_object.GetOwnerEntityID();
        }
        public virtual Entity GetOwnerEntity()
        {
            return m_parent_object.GetOwnerEntity();
        }
        #endregion

        #region 初始化/销毁
        public virtual void InitializeVariable(Dictionary<string, string> variables)
        {
            //ZZWTODO 有没有更好的方式？List？crc？
        }

        public virtual void InitializeComponent()
        {
        }

        public virtual void OnObjectCreated()
        {
            PostInitializeComponent();
            if (m_disable_count == 0)
                OnEnable();
            else if (m_disable_count > 0)
                OnDisable();
#if ALLOW_UPDATE
            ILogicNeedUpdateEveryFrame iupdate = this as ILogicNeedUpdateEveryFrame;
            if (iupdate != null)
                GetLogicWorld().GetComponent<LogicWorldEveryFrameUpdater>().Register(iupdate);
#endif
        }

        protected virtual void PostInitializeComponent()
        {
        }

        public virtual void OnDeletePending()
        {
        }

        public virtual void Destruct()
        {
#if ALLOW_UPDATE
            ILogicNeedUpdateEveryFrame iupdate = this as ILogicNeedUpdateEveryFrame;
            if (iupdate != null)
                GetLogicWorld().GetComponent<LogicWorldEveryFrameUpdater>().Unregister(iupdate);
#endif
            OnDestruct();
            m_parent_object = null;
        }

        protected virtual void OnDestruct()
        {
        }
        #endregion

        #region Enable/Disable
        public void Enable()
        {
            if (m_disable_count > 0)
            {
                --m_disable_count;
                if (m_disable_count == 0)
                    OnEnable();
            }
        }

        public void Disable()
        {
            ++m_disable_count;
            if (m_disable_count == 1)
                OnDisable();
        }

        public bool IsEnable()
        {
            return m_disable_count == 0;
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }
        #endregion

        #region Variable
        public virtual FixPoint GetVariable(ExpressionVariable variable, int index)
        {
            int vid = variable[index];
            if (index == variable.MaxIndex)
            {
                FixPoint value;
                if (GetVariable(vid, out value))
                    return value;
                else
                    return ObjectUtil.GetVariable(GetOwnerObject(), vid);
            }
            else if (vid == ExpressionVariable.VID_LevelTable)
            {
                return GetLogicWorld().GetConfigProvider().GetLevelBasedNumber(variable[index + 1], ObjectUtil.GetLevel(GetOwnerObject()));
            }
            else if (vid == ExpressionVariable.VID_Object)
            {
                Object owner_object = GetOwnerObject();
                if (owner_object != null)
                    return owner_object.GetVariable(variable, index + 1);
            }
            else if (vid == ExpressionVariable.VID_Entity)
            {
                Object owner_entity = GetOwnerEntity();
                if (owner_entity != null)
                    return owner_entity.GetVariable(variable, index + 1);
            }
            else if (vid == ExpressionVariable.VID_Player)
            {
                Object owner_player = GetOwnerPlayer();
                if (owner_player != null)
                    return owner_player.GetVariable(variable, index + 1);
            }
            Object owner = GetOwnerObject();
            if (owner != null)
                return owner.GetVariable(variable, index);
            else
                return FixPoint.Zero;
        }

        public virtual FixPoint GetVariable(int id)
        {
            FixPoint value;
            GetVariable(id, out value);
            return value;
        }

        public virtual bool GetVariable(int id, out FixPoint value)
        {
            value = FixPoint.Zero;
            return false;
        }

        public virtual bool SetVariable(int id, FixPoint value)
        {
            return false;
        }
        #endregion
    }
}