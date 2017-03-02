using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum SignalListenerContextType
    {
        Invalid = 0,
        Player,
        Entity,
        PlayerComponent,
        EntityComponent,
        AbilityComponent,
        EffectComponent,
    }

    public class SignalListenerContext
    {
        public SignalListenerContextType m_context_type;
        public int m_listener_id;
        public int m_object_id;
        public int m_ability_id;
        public int m_component_type_id;

        public ISignalListener GetListener(LogicWorld logic_world)
        {
            return null;
        }
    }
}