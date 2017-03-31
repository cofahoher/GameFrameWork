using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class RenderMessageProcessor: IRenderMessageProcessor
    {
        RenderWorld m_render_world;
        LogicWorld m_logic_world;
        RenderEntityManager m_render_entity_manager;

        public RenderMessageProcessor(RenderWorld render_world)
        {
            m_render_world = render_world;
            m_logic_world = render_world.GetLogicWorld();
            m_render_entity_manager = render_world.GetRenderEntityManager();
        }

        public void Destruct()
        {
            m_render_world = null;
            m_logic_world = null;
            m_render_entity_manager = null;
        }

        public void Process(RenderMessage msg)
		{
            switch (msg.Type)
            {
            case RenderMessageType.StartMoving:
                ProcessRenderMessage_StartMoving(msg.EntityID);
                SimpleRenderMessage.Recycle(msg as SimpleRenderMessage);
                break;
            case RenderMessageType.StopMoving:
                ProcessRenderMessage_StopMoving(msg.EntityID);
                SimpleRenderMessage.Recycle(msg as SimpleRenderMessage);
                break;
            case RenderMessageType.CreateEntity:
                ProcessRenderMessage_CreateEntity(msg.EntityID);
                SimpleRenderMessage.Recycle(msg as SimpleRenderMessage);
                break;
            case RenderMessageType.DestroyEntity:
                ProcessRenderMessage_DestroyEntity(msg.EntityID);
                SimpleRenderMessage.Recycle(msg as SimpleRenderMessage);
                break;
            default:
                break;
            }
        }

        void ProcessRenderMessage_StartMoving(int entity_id)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(entity_id);
            if (render_entity == null)
                return;
            ModelComponent model_component = render_entity.GetComponent<ModelComponent>();
            if (model_component == null)
                return;
            AnimationComponent animation_component = render_entity.GetComponent<AnimationComponent>();
            if (animation_component != null)
                animation_component.PlayerAnimation(AnimationName.RUN, true);
            AnimatorComponent animator_component = render_entity.GetComponent<AnimatorComponent>();
            if (animator_component != null)
                animator_component.SetParameter(AnimatorParameter.MOVING, true);
            m_render_world.RegisterMovingEntity(model_component);
        }

        void ProcessRenderMessage_StopMoving(int entity_id)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(entity_id);
            if (render_entity == null)
                return;
            ModelComponent model_component = render_entity.GetComponent<ModelComponent>();
            if (model_component == null)
                return;
            AnimationComponent animation_component = render_entity.GetComponent<AnimationComponent>();
            if (animation_component != null)
                animation_component.PlayerAnimation(AnimationName.IDLE, true);
            AnimatorComponent animator_component = render_entity.GetComponent<AnimatorComponent>();
            if (animator_component != null)
                animator_component.SetParameter(AnimatorParameter.MOVING, false);
            m_render_world.UnregisterMovingEntity(model_component);
        }

        void ProcessRenderMessage_CreateEntity(int entity_id)
        {
            Entity entity = m_logic_world.GetEntityManager().GetObject(entity_id);
            if (entity == null)
                return;
            m_render_entity_manager.CreateObject(entity.GetCreationContext());
        }

        void ProcessRenderMessage_DestroyEntity(int entity_id)
        {
            m_render_entity_manager.DestroyObject(entity_id);
        }
    }
}