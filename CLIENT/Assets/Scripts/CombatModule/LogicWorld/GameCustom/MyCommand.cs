using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class CommandType
    {
        public const int EntityMove = 10;
    }

    public class EntityMoveCommand : Command
    {
        public int m_entity_id = 0;
        public Vector3I m_destination;
        public EntityMoveCommand()
        {
            m_type = CommandType.EntityMove;
        }
    }
}