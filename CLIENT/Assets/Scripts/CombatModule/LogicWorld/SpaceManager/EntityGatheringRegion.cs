using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IRegionCallback
    {
        void OnEntityEnter(int entity_id);
        void OnEntityExit(int entity_id);
    }

    public class EntityGatheringRegion : IRecyclable
    {
        RegionCallbackManager m_manager;
        int m_id = 0;

        IRegionCallback m_callback;
        Entity m_binding_object;
        ISpacePartition m_partition;
        Vector3FP m_fixed_position = Vector3FP.Zero;
        Vector2FP m_fixed_facing = Vector2FP.Zero;
        TargetGatheringParam m_target_gathering_param;

        bool m_active = false;
        List<int> m_previous_entered_entities = new List<int>();
        List<int> m_current_entered_entities = new List<int>();

        #region 初始化/销毁
        public void PreConstruct(RegionCallbackManager manager)
        {
            m_manager = manager;
            m_id = manager.GenerateID();
        }

        public void Construct(IRegionCallback callback, Entity binding_object)
        {
            m_callback = callback;
            m_binding_object = binding_object;
        }

        public void Construct(IRegionCallback callback, ISpacePartition partition, Vector3FP fixed_position, Vector2FP fixed_facing)
        {
            m_callback = callback;
            m_binding_object = null;
            m_partition = partition;
            m_fixed_position = fixed_position;
            m_fixed_facing = fixed_facing;
        }

        public void Construct(IRegionCallback callback, ISpacePartition partition, Vector3FP fixed_position)
        {
            m_callback = callback;
            m_binding_object = null;
            m_partition = partition;
            m_fixed_position = fixed_position;
        }

        public void Destruct()
        {
            m_manager.DestroyRegion(this);
        }

        public void Reset()
        {
            m_manager = null;
            m_id = 0;

            m_callback = null;
            m_binding_object = null;
            m_fixed_position.MakeZero();
            m_fixed_facing.MakeZero();
            m_target_gathering_param = null;

            m_active = false;
            m_previous_entered_entities.Clear();
            m_current_entered_entities.Clear();
        }
        #endregion

        #region GETTER
        public int ID
        {
            get { return m_id; }
        }

        public bool Active
        {
            get { return m_active; }
        }

        public void SetTargetGatheringParam(TargetGatheringParam param)
        {
            m_target_gathering_param = param;
        }

        public int CurrentEnteredCount
        {
            get { return m_previous_entered_entities.Count; }
        }

        public List<int> GetCurrentEnteredObjects()
        {
            return m_previous_entered_entities;
        }
        #endregion

        public void Activate()
        {
            if (m_active)
                return;
            m_manager.Activate(this);
            m_active = true;
        }

        public void Deactivate()
        {
            if (!m_active)
                return;
            for (int i = 0; i < m_previous_entered_entities.Count; ++i)
                m_callback.OnEntityExit(m_previous_entered_entities[i]);
            m_previous_entered_entities.Clear();
            m_manager.Deactivate(this);
            m_active = false;
        }

        public void OnUpdate()
        {
            if (!m_active)
                return;
            TargetGatheringManager target_gathering_manager = m_manager.GetLogicWorld().GetTargetGatheringManager();
            if (target_gathering_manager == null)
                return;
            if (m_binding_object != null)
                target_gathering_manager.BuildTargetList(m_binding_object, m_target_gathering_param, m_current_entered_entities);
            else
                target_gathering_manager.BuildTargetList(m_partition, null, m_fixed_position, m_fixed_facing, m_target_gathering_param, m_current_entered_entities);
            for (int i = 0; i < m_previous_entered_entities.Count; ++i)
            {
                if (!m_current_entered_entities.Contains(m_previous_entered_entities[i]))
                    m_callback.OnEntityExit(m_previous_entered_entities[i]);
            }
            for (int i = 0; i < m_current_entered_entities.Count; ++i)
            {
                if (!m_previous_entered_entities.Contains(m_current_entered_entities[i]))
                    m_callback.OnEntityEnter(m_current_entered_entities[i]);
            }
            List<int> temp = m_previous_entered_entities;
            m_previous_entered_entities = m_current_entered_entities;
            m_current_entered_entities = temp;
            m_current_entered_entities.Clear();
        }
    }
}