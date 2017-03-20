using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class RenderMessageType
    {
        public const int Invalid = 0;
    }

    public abstract class RenderMessage
    {
        protected int m_entity_id = -1;
        protected int m_type = RenderMessageType.Invalid;
        public int Type
        {
            get { return m_type; }
            set { m_type = value; }
        }
        public int EntityID
        {
            get { return m_entity_id; }
            set { m_entity_id = value; }
        }
    }

    public class SimpleRenderMessage : RenderMessage, IRecyclable
    {
        public static SimpleRenderMessage Create(int type, int entity_id)
        {
            SimpleRenderMessage msg = ResuableObjectPool<IRecyclable>.Instance.Create<SimpleRenderMessage>();
            msg.Type = type;
            msg.EntityID = entity_id;
            return msg;
        }
        public static void Recycle(SimpleRenderMessage instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
        public void Reset()
        {
            m_entity_id = -1;
            m_type = RenderMessageType.Invalid;
        }
    }
}