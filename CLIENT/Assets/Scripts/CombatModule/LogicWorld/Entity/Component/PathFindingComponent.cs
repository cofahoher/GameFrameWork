using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class PathFindingComponent : EntityComponent
    {
        //FixPoint m_tolerance = FixPoint.One / FixPoint.Ten;
        //Vector3FP m_destination = new Vector3FP(new FixPoint(99999), FixPoint.Zero, new FixPoint(99999));

        public bool FindPath(Vector3FP destination)
        {
            GridGraph graph = GetLogicWorld().GetGridGraph();
            if (graph == null)
                return false;
            //if (FixPoint.Abs(destination.x - m_destination.x) + FixPoint.Abs(destination.z - m_destination.z) < m_tolerance)
            //    return true;
            LocomotorComponent locomotor_cmp = ParentObject.GetComponent(LocomotorComponent.ID) as LocomotorComponent;
            if (locomotor_cmp == null)
                return false;
            PositionComponent position_cmp = ParentObject.GetComponent(PositionComponent.ID) as PositionComponent;
            if (position_cmp == null)
                return false;
            if (!graph.FindPath(position_cmp.CurrentPosition, destination))
            {
                locomotor_cmp.StopMoving();
                return false;
            }
            List<Vector3FP> path = graph.GetPath();
            if (!locomotor_cmp.MoveAlongPath(path))
                return false;
            //m_destination = destination;
            return true;
        }
    }
}