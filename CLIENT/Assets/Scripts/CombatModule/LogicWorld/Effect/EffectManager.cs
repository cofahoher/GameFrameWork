using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectManager : IDestruct
    {
        LogicWorld m_logic_world;
        IDGenerator m_generator_id_generator;
        IDGenerator m_effect_id_generator;
        SortedDictionary<int, EffectGenerator> m_generators = new SortedDictionary<int, EffectGenerator>();
        SortedDictionary<int, Effect> m_effects = new SortedDictionary<int, Effect>();

        public EffectManager(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
            m_generator_id_generator = new IDGenerator(IDGenerator.EFFECT_GENERATOR_FIRST_ID);
            m_effect_id_generator = new IDGenerator(IDGenerator.EFFECT_FIRST_ID);
        }

        public void Destruct()
        {
            m_logic_world = null;
            m_generator_id_generator = null;
            m_effect_id_generator = null;
            m_generators.Clear();
            m_effects.Clear();
        }

        #region EffectGenerator
        public EffectGenerator CreateGenerator(ObjectCreationContext object_context, Entity SkillOwner)
        {
            return null;
        }
        #endregion
    }
}