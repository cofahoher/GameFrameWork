using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectCategory
    {
    }

    public class EffectSystem : Singleton<EffectSystem>
    {
        EffectCategory m_test_category;
        private EffectSystem()
        {
            m_test_category = new EffectCategory();
        }

        public override void Destruct()
        {
        }

        public EffectCategory GetCategory(int id)
        {
            return m_test_category;
        }
    }
}