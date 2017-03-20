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

        public void Handle(Command command)
        {
            switch (command.Type)
            {
            case CommandType.EntityMove:
                HandleEntityMove(command as EntityMoveCommand);
                break;
            default:
                break;
            }
        }

        void HandleEntityMove(EntityMoveCommand cmd)
        {
            Entity entity = m_logic_world.GetEntityManager().GetObject(cmd.m_entity_id);
            if (entity == null)
                return;
            LocomotorComponent locomotor_component = entity.GetComponent<LocomotorComponent>();
            if (locomotor_component == null)
                return;
            locomotor_component.MoveByDestination(cmd.m_destination);
        }
    }
}