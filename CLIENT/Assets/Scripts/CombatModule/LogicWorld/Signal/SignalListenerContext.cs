using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum SignalListenerContextType
    {
        Invalid = 0,
        EntityComponent,
    }

    public class SignalListenerContext : IRecyclable
    {
        public SignalListenerContextType m_context_type = SignalListenerContextType.Invalid;
        public int m_listener_id = -1;
        public int m_object_id = -1;
        public int m_ability_id = -1;
        public int m_component_type_id = -1;

        public int ID
        {
            get { return m_listener_id; }
        }

        public void Reset()
        {
            m_context_type = SignalListenerContextType.Invalid;
            m_listener_id = -1;
            m_object_id = -1;
            m_ability_id = -1;
            m_component_type_id = -1;
        }

        public static SignalListenerContext CreateForEntityComponent(int listener_id, int entity_id, int component_type_id)
        {
            SignalListenerContext context = ResuableObjectPool<IRecyclable>.Instance.Create<SignalListenerContext>();
            context.m_context_type = SignalListenerContextType.EntityComponent;
            context.m_listener_id = listener_id;
            context.m_object_id = entity_id;
            context.m_component_type_id = component_type_id;
            return context;
        }

        public static void Recycle(SignalListenerContext instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }

        public ISignalListener GetListener(LogicWorld logic_world)
        {
            switch (m_context_type)
            {
            case SignalListenerContextType.EntityComponent:
                Entity entity = logic_world.GetEntityManager().GetObject(m_object_id);
                if (entity == null)
                    return null;
                Component component = entity.GetComponent(m_component_type_id);
                if (component == null)
                    return null;
                return component as ISignalListener;
            default:
                break;
            }
            return null;
        }
    }
}