using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ClassifiedEffectContainer : IDestruct
    {
        EffectRegistry m_registry;
        EffectCategory m_category;
        List<Effect> m_active_effects = new List<Effect>();

        public ClassifiedEffectContainer(EffectRegistry registry, EffectCategory category)
        {
            m_registry = registry;
            m_category = category;
        }

        public void Destruct()
        {
            m_registry = null;
            m_category = null;
            m_active_effects.Clear();
        }

        public bool AddEffect(Effect effect)
        {
            EffectDefinitionComponent definition_cmp = effect.GetDefinitionComponent();
            if (definition_cmp.ExpirationTime <= effect.GetCurrentTime())
            {
                effect.Apply();
                effect.Unapply();
                return false;
            }

            int count = m_active_effects.Count;
            for (int i = 0; i < count; )
            {
                Effect active_effect = m_active_effects[i];
                if (!AreConflicting(effect, active_effect))
                {
                    ++i;
                    continue;
                }
                Effect rejected_effect = PickRejectedEffect(effect, active_effect);
                if (rejected_effect == effect)
                {
                    return false;
                }
                else
                {
                    m_registry.RemoveEffect(active_effect.ID);
                    count = m_active_effects.Count;
                }
            }

            m_active_effects.Add(effect);
            effect.Apply();
            definition_cmp.StartExpirationTask();
            return true;
        }

        bool AreConflicting(Effect effect1, Effect effect2)
        {
            EffectDefinitionComponent cmp1 = effect1.GetDefinitionComponent();
            EffectDefinitionComponent cmp2 = effect2.GetDefinitionComponent();
            if (cmp1.ConflictID == cmp2.ConflictID)
                return true;
            return false;
        }

        Effect PickRejectedEffect(Effect effect1, Effect effect2)
        {
            if (effect1.CreationTime < effect2.CreationTime)
                return effect1;
            return effect2;
        }

        public void RemoveEffect(Effect effect)
        {
            effect.Unapply();
            effect.GetDefinitionComponent().MarkExpired();
            m_active_effects.Remove(effect);
            effect.GetLogicWorld().GetEffectManager().DestroyObject(effect.ID);
        }
    }
}