using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Entity : Object, IRenderMessageGenerator
    {
        List<RenderMessage> m_render_messages;
        Player m_owner_player;

        public Entity()
        {
        }

        #region ILogicOwnerInfo
        public override Object GetOwnerObject()
        {
            return m_owner_player;
        }
        public override int GetOwnerPlayerID()
        {
            return m_context.m_owner_id;;
        }
        public override Player GetOwnerPlayer()
        {
            return m_owner_player;
        }
        public override int GetOwnerEntityID()
        {
            return m_context.m_object_id;
        }
        public override Entity GetOwnerEntity()
        {
            return this;
        }
        #endregion

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

        protected override void OnDestruct()
        {
            m_owner_player = null;
        }

        protected override void PreInitializeObject(ObjectCreationContext context)
        {
            if (context.m_logic_world.CanGenerateRenderMessage())
                m_render_messages = new List<RenderMessage>();
            m_owner_player = context.m_logic_world.GetPlayerManager().GetObject(context.m_owner_id);
        }
    }
}