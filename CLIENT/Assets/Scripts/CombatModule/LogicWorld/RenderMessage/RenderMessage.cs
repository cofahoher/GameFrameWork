using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class RenderMessageType
    {
        //命名是：做（什么）
        public const int Invalid = 0;
        public const int CreateEntity = 1;              //SimpleRenderMessage
        public const int DestroyEntity = 2;             //SimpleRenderMessage
    }

    public abstract class RenderMessage : IRecyclable
    {
        public static TRenderMessage Create<TRenderMessage>() where TRenderMessage : RenderMessage, new()
        {
            return ResuableObjectFactory<RenderMessage>.Create<TRenderMessage>();
        }
        public static void Recycle(RenderMessage instance)
        {
            ResuableObjectFactory<RenderMessage>.Recycle(instance);
        }

        public static bool Registered
        {
            get { return ResuableObjectFactory<RenderMessage>.Registered; }
            set { ResuableObjectFactory<RenderMessage>.Registered = value; }
        }
        public static void Register(int id, System.Type type)
        {
            ResuableObjectFactory<RenderMessage>.Register(id, type);
        }
        public static RenderMessage Create(int id)
        {
            return ResuableObjectFactory<RenderMessage>.Create(id);
        }

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

        public abstract void Reset();
    }

    public class SimpleRenderMessage : RenderMessage
    {
        public int m_simple_data = 0;

        public void Construct(int type, int entity_id, int simple_data = 0)
        {
            m_type = type;
            m_entity_id = entity_id;
            m_simple_data = simple_data;
        }

        public override void Reset()
        {
            m_entity_id = -1;
            m_type = RenderMessageType.Invalid;
            m_simple_data = 0;
        }
    }
}