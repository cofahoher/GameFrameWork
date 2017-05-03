using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class RenderMessageType
    {
        //命名是：做（什么）
        public const int StartMoving = 101;               //SimpleRenderMessage
        public const int StopMoving = 102;                //SimpleRenderMessage
        public const int ChangeDirection = 103;           //ChangeDirectionRenderMessage
        public const int ChangeHealth = 110;              //ChangeHealthRenderMessage
        public const int Die = 111;                       //SimpleRenderMessage
        public const int Hide = 112;                      //SimpleRenderMessage
        public const int PlayAnimation = 120;             //SimpleRenderMessage
    }

    public class ChangeDirectionRenderMessage : RenderMessage
    {
        public FixPoint m_new_angle = FixPoint.Zero;

        public ChangeDirectionRenderMessage()
        {
            m_type = RenderMessageType.ChangeDirection;
        }

        public void Construct(int entity_id, FixPoint new_angle)
        {
            m_entity_id = entity_id;
            m_new_angle = new_angle;
        }

        public override void Reset()
        {
            m_entity_id = -1;
            m_new_angle = FixPoint.Zero;
        }
    }

    public class ChangeHealthRenderMessage : RenderMessage
    {
        public FixPoint m_delta_health = FixPoint.Zero;
        public FixPoint m_current_health = FixPoint.Zero;

        public ChangeHealthRenderMessage()
        {
            m_type = RenderMessageType.ChangeHealth;
        }

        public void Construct(int entity_id, FixPoint delta_health, FixPoint current_health)
        {
            m_entity_id = entity_id;
            m_delta_health = delta_health;
            m_current_health = current_health;
        }

        public override void Reset()
        {
            m_entity_id = -1;
            m_delta_health = FixPoint.Zero;
            m_current_health = FixPoint.Zero;
        }
    }

    public class PlayAnimationRenderMessage : RenderMessage
    {
        public string m_animation_name;
        public string m_animation_name_2;
        public bool m_loop;

        public PlayAnimationRenderMessage()
        {
            m_type = RenderMessageType.PlayAnimation;
        }

        public void Construct(int entity_id, string animation_name, string animation_name_2 = null, bool loop = false)
        {
            m_entity_id = entity_id;
            m_animation_name = animation_name;
            m_animation_name_2 = animation_name_2;
            m_loop = loop;
        }

        public override void Reset()
        {
            m_entity_id = -1;
        }
    }
}