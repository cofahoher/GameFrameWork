using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EffectManagerComponent : EntityComponent
    {
        EffectGeneratorRegistry m_generator_registry;
        EffectRegistry m_effect_registry;

        #region GETTER
        public EffectGeneratorRegistry GetEffectGeneratorRegistry()
        {
            return m_generator_registry;
        }

        public EffectRegistry GetEffectRegistry()
        {
            return m_effect_registry;
        }
        #endregion

        #region 初始化/销毁
        public EffectManagerComponent()
        {
            m_generator_registry = new EffectGeneratorRegistry(this);
            m_effect_registry = new EffectRegistry(this);
        }

        protected override void OnDestruct()
        {
            m_generator_registry.Destruct();
            m_generator_registry = null;
            m_effect_registry.Destruct();
            m_effect_registry = null;
        }

        public override void OnDeletePending()
        {
            m_generator_registry.OnDeletePending();
            m_effect_registry.OnDeletePending();
        }
        #endregion
    }
}