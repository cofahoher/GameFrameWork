using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EffectRegionComponent : EntityComponent, IRegionCallback, INeedTaskService
    {
        //配置数据
        TargetGatheringParam m_target_gathering_param = new TargetGatheringParam();
        int m_enter_generator_cfgid = 0;
        int m_period_generator_cfgid = 0;
        FixPoint m_period = FixPoint.One;
        FixPoint m_region_update_interval = FixPoint.One;

        //运行数据
        EffectGenerator m_enter_generator;
        EffectGenerator m_period_generator;
        EntityGatheringRegion m_region;
        ComponentCommonTask m_task;

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            Entity owner = GetOwnerEntity();
            PositionComponent position_component = owner.GetComponent(PositionComponent.ID) as PositionComponent;
            if (position_component == null)
                return;

            EffectManager effect_manager = GetLogicWorld().GetEffectManager();
            if (m_enter_generator_cfgid > 0)
                m_enter_generator = effect_manager.CreateGenerator(m_enter_generator_cfgid, owner);
            if (m_period_generator_cfgid > 0)
                m_period_generator = effect_manager.CreateGenerator(m_period_generator_cfgid, owner);

            m_region = GetLogicWorld().GetRegionCallbackManager().CreateRegion();
            m_region.Construct(this, owner);
            m_region.SetUpdateInterval(m_region_update_interval);
            m_region.SetTargetGatheringParam(m_target_gathering_param);
            m_region.Activate();

            if (m_period_generator != null)
            {
                m_task = LogicTask.Create<ComponentCommonTask>();
                m_task.Construct(this);
                var schedeler = GetLogicWorld().GetTaskScheduler();
                schedeler.Schedule(m_task, GetCurrentTime(), m_period, m_period);
            }
        }

        protected override void OnDestruct()
        {
            EffectManager effect_manager = GetLogicWorld().GetEffectManager();
            if (m_enter_generator != null)
            {
                effect_manager.DestroyGenerator(m_enter_generator.ID, GetOwnerEntityID());
                m_enter_generator = null;
            }
            if (m_period_generator != null)
            {
                effect_manager.DestroyGenerator(m_period_generator.ID, GetOwnerEntityID());
                m_period_generator = null;
            }
            if (m_region != null)
            {
                m_region.Destruct();
                m_region = null;
            }
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
        }
        #endregion

        #region IRegionCallback
        public void OnEntityEnter(int entity_id)
        {
            Entity owner = GetOwnerEntity();
            if (ObjectUtil.IsDead(owner))
                return;
            if (m_enter_generator == null)
                return;
            Entity entity = GetLogicWorld().GetEntityManager().GetObject(entity_id);
            if (entity == null)
                return;
            if (ObjectUtil.IsDead(entity))
                return;
            EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
            app_data.m_original_entity_id = ParentObject.ID;
            app_data.m_source_entity_id = ParentObject.ID;
            m_enter_generator.Activate(app_data, entity);
            RecyclableObject.Recycle(app_data);
        }

        public void OnEntityExit(int entity_id)
        {
            Entity owner = GetOwnerEntity();
            if (ObjectUtil.IsDead(owner))
                return;
            if (m_enter_generator == null)
                return;
            Entity entity = GetLogicWorld().GetEntityManager().GetObject(entity_id);
            if (entity == null)
                return;
            m_enter_generator.DeactivateOnOneTarget(entity);
        }
        #endregion

        public void OnTaskService(FixPoint delta_time)
        {
            List<int> ids = m_region.GetCurrentEnteredObjects();
            if (ids.Count == 0)
                return;
            EntityManager entity_manager = GetLogicWorld().GetEntityManager();
            EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
            app_data.m_original_entity_id = ParentObject.ID;
            app_data.m_source_entity_id = ParentObject.ID;
            for (int i = 0; i < ids.Count; ++i)
            {
                Entity entity = entity_manager.GetObject(ids[i]);
                if (entity == null)
                    continue;
                m_period_generator.Activate(app_data, entity);
            }
            RecyclableObject.Recycle(app_data);
        }
    }
}