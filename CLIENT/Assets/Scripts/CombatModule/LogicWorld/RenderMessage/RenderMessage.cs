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
        public const int StartMoving = 3;               //SimpleRenderMessage
        public const int StopMoving = 4;                //SimpleRenderMessage
        public const int ChangeHealth = 5;              //ChangeHealthRenderMessage
    }

    public abstract class RenderMessage : IRecyclable, IDestruct
    {
        public static TRenderMessage Create<TRenderMessage>() where TRenderMessage : RenderMessage, new()
        {
            TRenderMessage msg = ResuableObjectFactory<RenderMessage>.Create<TRenderMessage>();
            return msg;
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
            RenderMessage msg = ResuableObjectFactory<RenderMessage>.Create(id);
            return msg;
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

        public virtual void Destruct()
        {
        }

        public abstract void Reset();
    }

    public class SimpleRenderMessage : RenderMessage
    {
        public void Construct(int type, int entity_id)
        {
            m_type = type;
            m_entity_id = entity_id;
        }

        public override void Reset()
        {
            m_entity_id = -1;
            m_type = RenderMessageType.Invalid;
        }
    }
}