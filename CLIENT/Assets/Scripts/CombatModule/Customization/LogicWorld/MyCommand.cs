using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class CommandType
    {
        public const int EntityMove = 10;
        public const int EntityTarget = 11;
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

        public void ConstructAsDirection(int entity_id, Vector3FP direction)
        {
            m_entity_id = entity_id;
            m_move_type = DirectionType;
            m_vector = direction;
        }

        public void ConstructAsDestination(int entity_id, Vector3FP destination)
        {
            m_entity_id = entity_id;
            m_move_type = DestinationType;
            m_vector = destination;
        }

        public void ConstructAsStopMove(int entity_id)
        {
            m_entity_id = entity_id;
            m_move_type = StopMoving;
        }

        public override void Reset()
        {
            base.Reset();
            m_move_type = 0;
            m_vector.MakeZero();
        }
    }

    public class EntityTargetCommand : Command
    {
        public int m_target_entity_id = 0;

        public EntityTargetCommand()
        {
            m_type = CommandType.EntityTarget;
        }

        public void Construct(int entity_id, int target_entity_id)
        {
            m_entity_id = entity_id;
            m_target_entity_id = target_entity_id;
        }

        public override void Reset()
        {
            base.Reset();
            m_target_entity_id = 0;
        }
    }
}