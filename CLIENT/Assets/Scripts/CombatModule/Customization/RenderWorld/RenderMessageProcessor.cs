using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class RenderMessageProcessor : IRenderMessageProcessor
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
                    ProcessRenderMessage_StartMoving(msg as SimpleRenderMessage);
                    break;
                case RenderMessageType.StopMoving:
                    ProcessRenderMessage_StopMoving(msg as SimpleRenderMessage);
                    break;
                case RenderMessageType.ChangeDirection:
                    ProcessRenderMessage_ChangeDirection(msg as ChangeDirectionRenderMessage);
                    break;
                case RenderMessageType.FindPath:
                    ProcessRenderMessage_FindPath(msg as SimpleRenderMessage);
                    break;
                case RenderMessageType.CreateEntity:
                    ProcessRenderMessage_CreateEntity(msg.EntityID);
                    break;
                case RenderMessageType.DestroyEntity:
                    ProcessRenderMessage_DestroyEntity(msg.EntityID);
                    break;
                case RenderMessageType.ChangeHealth:
                    ProcessRenderMessage_ChangeHealth(msg as ChangeHealthRenderMessage);
                    break;
                case RenderMessageType.Die:
                    ProcessRenderMessage_Die(msg.EntityID);
                    break;
                case RenderMessageType.Hide:
                    ProcessRenderMessage_Hide(msg.EntityID);
                    break;
                case RenderMessageType.PlayAnimation:
                    ProcessRenderMessage_PlayAnimation(msg as PlayAnimationRenderMessage);
                    break;
                case RenderMessageType.TakeDamage:
                    ProcessRenderMessage_TakeDamage(msg as TakeDamageRenderMessage);
                    break;
                default:
                    break;
            }
            RenderMessage.Recycle(msg);
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

        void ProcessRenderMessage_StartMoving(SimpleRenderMessage msg)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(msg.EntityID);
            if (render_entity == null)
                return;
            ModelComponent model_component = render_entity.GetComponent(ModelComponent.ID) as ModelComponent;
            if (model_component == null)
                return;
            PredictLogicComponent predic_component = render_entity.GetComponent(PredictLogicComponent.ID) as PredictLogicComponent;
            if (predic_component == null || !predic_component.HasMovementPredict)
            {
                model_component.UpdateAngle();
                AnimationComponent animation_component = render_entity.GetComponent(AnimationComponent.ID) as AnimationComponent;
                if (animation_component != null)
                    animation_component.PlayerAnimation(AnimationName.RUN, true);
                AnimatorComponent animator_component = render_entity.GetComponent(AnimatorComponent.ID) as AnimatorComponent;
                if (animator_component != null)
                    animator_component.PlayAnimation(AnimationName.RUN);
            }
            m_render_world.RegisterMovingEntity(model_component);
        }

        void ProcessRenderMessage_StopMoving(SimpleRenderMessage msg)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(msg.EntityID);
            if (render_entity == null)
                return;
            ModelComponent model_component = render_entity.GetComponent(ModelComponent.ID) as ModelComponent;
            if (model_component == null)
                return;
            PredictLogicComponent predic_component = render_entity.GetComponent(PredictLogicComponent.ID) as PredictLogicComponent;
            if (predic_component == null || !predic_component.HasMovementPredict)
            {
                AnimationComponent animation_component = render_entity.GetComponent(AnimationComponent.ID) as AnimationComponent;
                if (animation_component != null)
                    animation_component.PlayerAnimation(AnimationName.IDLE, true);
                AnimatorComponent animator_component = render_entity.GetComponent(AnimatorComponent.ID) as AnimatorComponent;
                if (animator_component != null)
                    animator_component.PlayAnimation(AnimationName.IDLE);
            }
            m_render_world.UnregisterMovingEntity(model_component);
        }

        void ProcessRenderMessage_ChangeDirection(ChangeDirectionRenderMessage msg)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(msg.EntityID);
            if (render_entity == null)
                return;
            PredictLogicComponent predic_component = render_entity.GetComponent(PredictLogicComponent.ID) as PredictLogicComponent;
            if (predic_component != null && predic_component.HasMovementPredict)
                return;
            ModelComponent model_component = render_entity.GetComponent(ModelComponent.ID) as ModelComponent;
            if (model_component == null)
                return;
            model_component.UpdateAngle();
        }

        void ProcessRenderMessage_FindPath(SimpleRenderMessage msg)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(msg.EntityID);
            if (render_entity == null)
                return;
            PredictLogicComponent predic_component = render_entity.GetComponent(PredictLogicComponent.ID) as PredictLogicComponent;
            if (predic_component == null)
                return;
            predic_component.OnLogicFindPath();
        }

        void ProcessRenderMessage_ChangeHealth(ChangeHealthRenderMessage msg)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(msg.EntityID);
            if (render_entity == null)
                return;
        }

        void ProcessRenderMessage_Die(int entity_id)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(entity_id);
            if (render_entity == null)
                return;
            AnimationComponent animation_component = render_entity.GetComponent(AnimationComponent.ID) as AnimationComponent;
            if (animation_component != null)
                animation_component.PlayerAnimation(AnimationName.DIE);
        }

        void ProcessRenderMessage_Hide(int entity_id)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(entity_id);
            if (render_entity == null)
                return;
        }

        void ProcessRenderMessage_PlayAnimation(PlayAnimationRenderMessage msg)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(msg.EntityID);
            if (render_entity == null)
                return;
            AnimationComponent animation_component = render_entity.GetComponent(AnimationComponent.ID) as AnimationComponent;
            if (animation_component != null)
            {
                if (msg.m_animation_name_2 == null)
                {
                    animation_component.PlayerAnimation(msg.m_animation_name, msg.m_loop);
                }
                else
                {
                    animation_component.PlayerAnimation(msg.m_animation_name, false);
                    animation_component.QueueAnimation(msg.m_animation_name_2, msg.m_loop);
                }
                if (!msg.m_loop)
                    animation_component.QueueAnimation(AnimationName.IDLE, true);
            }
            AnimatorComponent animator_component = render_entity.GetComponent(AnimatorComponent.ID) as AnimatorComponent;
            if (animator_component != null)
            {
                animator_component.PlayAnimation(msg.m_animation_name);
            }
        }

        void ProcessRenderMessage_TakeDamage(TakeDamageRenderMessage msg)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(msg.EntityID);
            if (render_entity == null)
                return;
            //ZZWTODO
        }
    }
}