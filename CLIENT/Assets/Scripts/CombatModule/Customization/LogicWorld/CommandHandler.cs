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
            case CommandType.EntityTarget:
                return HandleEntityTarget(command as EntityTargetCommand);
            default:
                return false;
            }
        }

        bool HandleEntityMove(EntityMoveCommand cmd)
        {
            Entity entity = m_logic_world.GetEntityManager().GetObject(cmd.m_entity_id);
            if (entity == null)
                return false;
            LocomotorComponent locomotor_component = entity.GetComponent(LocomotorComponent.ID) as LocomotorComponent;
            if (locomotor_component == null)
                return false;
            if (cmd.m_move_type != EntityMoveCommand.StopMoving)
            {
                TargetingComponent targeting_component = entity.GetComponent(TargetingComponent.ID) as TargetingComponent;
                if (targeting_component != null)
                    targeting_component.StopTargeting();
            }
            if (cmd.m_move_type == EntityMoveCommand.DestinationType)
            {
                PathFindingComponent pathfinding_component = entity.GetComponent(PathFindingComponent.ID) as PathFindingComponent;
                if (pathfinding_component != null)
                {
                    return pathfinding_component.FindPath(cmd.m_vector);
                }
                else
                {
                    PositionComponent position_component = entity.GetComponent(PositionComponent.ID) as PositionComponent;
                    List<Vector3FP> path = new List<Vector3FP>();
                    path.Add(position_component.CurrentPosition);
                    path.Add(cmd.m_vector);
                    locomotor_component.MoveAlongPath(path);
                }
            }
            else if (cmd.m_move_type == EntityMoveCommand.DirectionType)
            {
                locomotor_component.MoveByDirection(cmd.m_vector);
            }
            else if (cmd.m_move_type == EntityMoveCommand.StopMoving)
            {
                locomotor_component.StopMoving();
            }
            return true;
        }

        bool HandleEntityTarget(EntityTargetCommand cmd)
        {
            Entity entity = m_logic_world.GetEntityManager().GetObject(cmd.m_entity_id);
            if (entity == null)
                return false;
            Entity target = m_logic_world.GetEntityManager().GetObject(cmd.m_target_entity_id);
            if (target == null)
                return false;
            TargetingComponent targeting_component = entity.GetComponent(TargetingComponent.ID) as TargetingComponent;
            if (targeting_component == null)
                return false;
            targeting_component.StartTargeting(target);
            return true;
        }
    }
}