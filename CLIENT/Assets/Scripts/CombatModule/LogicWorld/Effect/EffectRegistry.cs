using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectRegistry : IDestruct
    {
        EffectManagerComponent m_owner_component;
        SortedDictionary<int, Effect> m_effects_by_id = new SortedDictionary<int, Effect>();
        SortedDictionary<int, ClassifiedEffectContainer> m_effects_by_class = new SortedDictionary<int, ClassifiedEffectContainer>();

        public EffectManagerComponent GetOwnerComponent()
        {
            return m_owner_component;
        }

        #region 初始化/销毁
        public EffectRegistry(EffectManagerComponent owner_component)
        {
            m_owner_component = owner_component;
        }

        public void Destruct()
        {
            EffectManager effect_manager = m_owner_component.GetLogicWorld().GetEffectManager();
            var enumerator1 = m_effects_by_id.GetEnumerator();
            while (enumerator1.MoveNext())
                effect_manager.DestroyObject(enumerator1.Current.Key);
            m_effects_by_id.Clear();

            var enumerator2 = m_effects_by_class.GetEnumerator();
            while (enumerator2.MoveNext())
                enumerator2.Current.Value.Destruct();
            m_effects_by_class.Clear();

            m_owner_component = null;
        }
        #endregion

        public bool CanAddEffect()
        {
            return m_owner_component.IsEnable();
        }

        public bool AddEffect(Effect effect)
        {
            EffectDefinitionComponent definition_cmp = effect.GetDefinitionComponent();
            ClassifiedEffectContainer container = GetContainer(definition_cmp.Category);
            if (container == null)
                return false;
            if (!container.AddEffect(effect))
                return false;
            m_effects_by_id[effect.ID] = effect;
            return true;
        }

        public Effect GetEffect(int effect_id)
        {
            Effect effect;
            if (!m_effects_by_id.TryGetValue(effect_id, out effect))
                return null;
            return effect;
        }

        public void RemoveEffect(int effect_id)
        {
            Effect effect;
            if (!m_effects_by_id.TryGetValue(effect_id, out effect))
                return;
            EffectDefinitionComponent definition_cmp = effect.GetDefinitionComponent();
            ClassifiedEffectContainer container = GetContainer(definition_cmp.Category);
            if (container == null)
                return;
            container.RemoveEffect(effect);
            m_effects_by_id.Remove(effect_id);
        }

        ClassifiedEffectContainer GetContainer(int category_id)
        {
            ClassifiedEffectContainer container;
            if (!m_effects_by_class.TryGetValue(category_id, out container))
            {
                EffectCategory category = EffectSystem.Instance.GetCategory(category_id);
                if (category == null)
                    return null;
                container = new ClassifiedEffectContainer(this, category);
                m_effects_by_class[category_id] = container;
            }
            return container;
        }
    }
}