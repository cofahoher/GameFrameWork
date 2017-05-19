using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectManager : ObjectManager<Effect>
    {
        IDGenerator m_generator_id_generator;
        SortedDictionary<int, EffectGenerator> m_generators = new SortedDictionary<int, EffectGenerator>();

        public EffectManager(LogicWorld logic_world)
            : base(logic_world, IDGenerator.EFFECT_FIRST_ID)
        {
            m_generator_id_generator = new IDGenerator(IDGenerator.EFFECT_GENERATOR_FIRST_ID);
        }

        public override void Destruct()
        {
            m_generator_id_generator = null;
            var enumerator = m_generators.GetEnumerator();
            while (enumerator.MoveNext())
            {
                EffectGenerator generator = enumerator.Current.Value;
                RecyclableObject.Recycle(generator);
            }
            m_generators.Clear();

            base.Destruct();
        }

        protected override Effect CreateObjectInstance()
        {
            return new Effect();
        }

        #region EffectGenerator
        public EffectGenerator CreateGenerator(int generator_id, Entity owner)
        {
            IConfigProvider config_provider = m_logic_world.GetConfigProvider();
            EffectGeneratorData data = config_provider.GetEffectGeneratorData(generator_id);
            if (data == null)
                return null;
            int id = m_generator_id_generator.GenID();
            EffectGenerator generator = RecyclableObject.Create<EffectGenerator>();
            generator.Construct(m_logic_world, id, data);
            m_generators[id] = generator;
            EffectGeneratorRegistry registry = EntityUtil.GetEffectGeneratorRegistry(owner);
            if (registry != null)
                registry.AddGenerator(generator);
            return generator;
        }

        public EffectGenerator GetGenerator(int generator_id)
        {
            EffectGenerator generator;
            if (!m_generators.TryGetValue(generator_id, out generator))
                return null;
            return generator;
        }

        public void DestroyGenerator(int generator_id, int owner_entity_id)
        {
            EffectGenerator generator;
            if (!m_generators.TryGetValue(generator_id, out generator))
                return;
            if (generator.RemoveBySelf && !generator.RemoveSelfWhenIdle)
            {
                generator.RemoveSelfWhenIdle = true;
            }
            else
            {
                generator.ForceDeactivate();
                m_generators.Remove(generator_id);
                Entity owner = m_logic_world.GetEntityManager().GetObject(owner_entity_id);
                EffectGeneratorRegistry registry = EntityUtil.GetEffectGeneratorRegistry(owner);
                if (registry != null)
                    registry.RemoveGenerator(generator_id);
                RecyclableObject.Recycle(generator);
            }
        }
        #endregion
    }
}