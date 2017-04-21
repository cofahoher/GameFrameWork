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
        public const int StopMoving = 0;
        public const int DirectionType = 1;
        public const int DestinationType = 2;

        public int m_move_type = 0; 
        public Vector3FP m_vector;

        public EntityMoveCommand()
        {
            m_type = CommandType.EntityMove;
        }

        public override void Reset()
        {
            base.Reset();
            m_move_type = 0;
            m_vector.MakeZero();
        }
    }
}