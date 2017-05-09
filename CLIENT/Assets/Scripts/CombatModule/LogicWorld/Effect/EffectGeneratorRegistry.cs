using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectGeneratorRegistry : IDestruct
    {
        EffectManagerComponent m_owner_component;
        SortedDictionary<int, EffectGenerator> m_generators = new SortedDictionary<int, EffectGenerator>();

        public EffectManagerComponent GetOwnerComponent()
        {
            return m_owner_component;
        }

        public EffectGeneratorRegistry(EffectManagerComponent owner_component)
        {
            m_owner_component = owner_component;
        }

        public void Destruct()
        {
            EffectManager effect_manager = m_owner_component.GetLogicWorld().GetEffectManager();
            var enumerator = m_generators.GetEnumerator();
            while (enumerator.MoveNext())
                effect_manager.DestroyGenerator(enumerator.Current.Key, 0);
            m_generators.Clear();
            m_owner_component = null;
        }

        public void AddGenerator(EffectGenerator generator)
        {
            m_generators[generator.ID] = generator;
        }

        public EffectGenerator GetGenerator(int id)
        {
            EffectGenerator generator;
            if (!m_generators.TryGetValue(id, out generator))
                return null;
            return generator;
        }

        public void RemoveGenerator(int id)
        {
            m_generators.Remove(id);
        }
    }
}