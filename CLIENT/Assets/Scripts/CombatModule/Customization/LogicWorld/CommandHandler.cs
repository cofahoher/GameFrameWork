using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class CommandHandler : ICommandHandler
    {
        LogicWorld m_logic_world;

        public CommandHandler(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
        }

        public void Destruct()
        {
        }

        public bool Handle(Command command)
        {
            switch (command.Type)
            {
            case CommandType.EntityMove:
                return HandleEntityMove(command as EntityMoveCommand);
            default:
                return false;
            }
        }

        bool HandleEntityMove(EntityMoveCommand cmd)
        {
            Entity entity = m_logic_world.GetEntityManager().GetObject(cmd.m_entity_id);
            if (entity == null)
                return false;
            LocomotorComponent locomotor_component = entity.GetComponent<LocomotorComponent>();
            if (locomotor_component == null)
                return false;
            if (cmd.m_move_type == EntityMoveCommand.DestinationType)
                locomotor_component.MoveByDestination(cmd.m_vector, LocomotorComponent.StartMovingReason_Command);
            else if (cmd.m_move_type == EntityMoveCommand.DirectionType)
                locomotor_component.MoveByDirection(cmd.m_vector, LocomotorComponent.StartMovingReason_Command);
            else if (cmd.m_move_type == EntityMoveCommand.StopMoving)
                locomotor_component.StopMoving(LocomotorComponent.StopMovingReason_Command);
            return true;
        }
    }
}