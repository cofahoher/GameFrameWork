using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class RenderMessageType
    {
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
}