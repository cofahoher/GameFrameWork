using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectGeneratorEntry : IRecyclable
    {
        EffectGenerator m_generator;
        EffectGeneratorEntryData m_data;
        int m_index = -1;
        SortedDictionary<int, int> m_effect2entity = new SortedDictionary<int, int>();
        List<Target> m_targets;

        #region GETTER
        public bool Idle
        {
            get { return m_effect2entity.Count == 0; }
        }
        #endregion

        #region 初始化/销毁
        public void Construct(EffectGenerator generator, EffectGeneratorEntryData data, int index)
        {
            m_generator = generator;
            m_data = data;
            m_index = index;
        }

        public void Reset()
        {
            m_generator = null;
            m_data = null;
            m_index = -1;
            m_effect2entity.Clear();
            ClearTargets();
        }

        void ClearTargets()
        {
            if (m_targets == null)
                return;
            for (int i = 0; i < m_targets.Count; ++i)
                RecyclableObject.Recycle(m_targets[i]);
            m_targets.Clear();
        }

        Effect CreateEffect(int target_entity_id)
        {
            LogicWorld logic_world = m_generator.GetLogicWorld();
            EffectManager effect_manager = logic_world.GetEffectManager();
            IConfigProvider config = logic_world.GetConfigProvider();
            ObjectTypeData effect_data = config.GetEffectData(m_data.m_effect_id);
            if (effect_data == null)
                return null;
            ObjectCreationContext object_context = new ObjectCreationContext();
            //m_object_proxy_id
            object_context.m_object_type_id = m_data.m_effect_id;
            object_context.m_type_data = effect_data;
            object_context.m_logic_world = logic_world;
            object_context.m_owner_id = target_entity_id;
            Effect effect = effect_manager.CreateObject(object_context);
            return effect;
        }
        #endregion

        public void Activate(EffectApplicationData app_data, List<Target> default_targets)
        {
            if (m_data.m_target_gathering_param.m_type == TargetGatheringType.Default || m_data.m_target_gathering_param.m_type == 0)
            {
                for (int i = 0; i < default_targets.Count; ++i)
                {
                    Entity entity = default_targets[i].GetEntity();
                    if (entity != null)
                        ActivateOnOneTatget(app_data, entity);
                }
            }
            else
            {
                GatherTargetsAndActivate(app_data);
            }
        }

        public void Activate(EffectApplicationData app_data, Entity target)
        {
            if (m_data.m_target_gathering_param.m_type == TargetGatheringType.Default || m_data.m_target_gathering_param.m_type == 0)
                ActivateOnOneTatget(app_data, target);
            else
                GatherTargetsAndActivate(app_data);
        }

        void GatherTargetsAndActivate(EffectApplicationData app_data)
        {
            LogicWorld logic_world = m_generator.GetLogicWorld();
            Entity source_entity = logic_world.GetEntityManager().GetObject(app_data.m_source_entity_id);
            if (source_entity == null)
                return;
            if (m_targets == null)
                m_targets = new List<Target>();
            m_generator.GetLogicWorld().GetTargetGatheringManager().BuildTargetList(source_entity, m_data.m_target_gathering_param, m_targets);
            for (int i = 0; i < m_targets.Count; ++i)
            {
                Entity entity = m_targets[i].GetEntity();
                if (entity != null)
                    ActivateOnOneTatget(app_data, entity);
            }
            ClearTargets();
        }

        void ActivateOnOneTatget(EffectApplicationData app_data, Entity target)
        {
            EffectRegistry registry = EntityUtil.GetEffectRegistry(target);
            if (registry == null)
                return;
            if (!registry.CanAddEffect())
                return;
            Effect effect = CreateEffect(target.ID);
            if (effect == null)
                return;
            app_data.m_target_entity_id = target.ID;
            app_data.m_generator_id = m_generator.ID;
            app_data.m_entry_index = m_index;
            EffectDefinitionComponent definition_cmp = effect.GetDefinitionComponent();
            definition_cmp.InitializeApplicationData(app_data);
            if (!registry.AddEffect(effect))
            {
                m_generator.GetLogicWorld().GetEffectManager().DestroyObject(effect.ID);
            }
            else
            {
                m_effect2entity[effect.ID] = target.ID;
            }
        }

        public void RemoveEffect(int effect_id)
        {
            if (m_effect2entity == null)
                return;
            m_effect2entity.Remove(effect_id);
            if (Idle)
                m_generator.CheckIdle();
        }

        public void Deactivate()
        {
            if (m_effect2entity.Count == 0)
                return;

            LogicWorld logic_world = m_generator.GetLogicWorld();
            EntityManager entity_manager = logic_world.GetEntityManager();

            SortedDictionary<int, int> temp = m_effect2entity;
            m_effect2entity = null;
            var enumerator = temp.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Entity entity = entity_manager.GetObject(enumerator.Current.Value);
                EffectRegistry registry = EntityUtil.GetEffectRegistry(entity);
                if (registry != null)
                    registry.RemoveEffect(enumerator.Current.Key);
            }

            temp.Clear();
            m_effect2entity = temp;
        }
    }
}