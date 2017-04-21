using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Entity : Object
#if ENTITY_RENDER_MESSAGE
        , IRenderMessageGenerator
#endif
    {
#if ENTITY_RENDER_MESSAGE
        //List<RenderMessage> m_render_messages;
#endif
        Player m_owner_player;

        public Entity()
        {
        }

        protected override void OnDestruct()
        {
            m_owner_player = null;
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
#if ENTITY_RENDER_MESSAGE
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

        public void AddSimpleRenderMessage(int type, int entity_id = -1, int simple_data = 0)
        {
            if (m_render_messages == null)
                return;
            SimpleRenderMessage render_message = SimpleRenderMessage.Create(type, entity_id, simple_data);
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
#endif
        #endregion

        protected override void PreInitializeObject(ObjectCreationContext context)
        {
#if ENTITY_RENDER_MESSAGE
            if (context.m_logic_world.CanGenerateRenderMessage())
                m_render_messages = new List<RenderMessage>();
#endif
            PlayerManager player_manager = context.m_logic_world.GetPlayerManager();
            int player_id = player_manager.Proxyid2Objectid(context.m_object_proxy_id);
            context.m_owner_id = player_id;
            m_owner_player = player_manager.GetObject(player_id);
        }
    }
}