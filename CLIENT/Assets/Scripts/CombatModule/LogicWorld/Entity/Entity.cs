using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Entity : Object, IRenderMessageGenerator
    {
        List<RenderMessage> m_render_messages;

        public Entity()
        {
        }

        protected override void PreInitializeObject(ObjectCreationContext context)
        {
            if (context.m_logic_world.CanGenerateRenderMessage())
                m_render_messages = new List<RenderMessage>();
        }

        #region RenderMessage
        public bool CanGenerateRenderMessage()
        {
            return m_render_messages != null;
        }

        public void AddRenderMessage(RenderMessage render_message)
        {
            if (m_render_messages == null)
                return;
            m_render_messages.Add(render_message);
        }

        public List<RenderMessage> GetAllRenderMessages()
        {
            return m_render_messages;
        }

        public void ClearRenderMessages()
        {
            if (m_render_messages != null)
                m_render_messages.Clear();
        }
        #endregion
    }
}