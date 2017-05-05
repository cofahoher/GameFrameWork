using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ISpaceManager : IDestruct
    {
        void AddEntiy(PositionComponent entity);
        void RemoveEntity(PositionComponent entity);
        void UpdateEntity(PositionComponent entity, Vector3FP new_position);
        List<int> CollectEntity_Point(Vector3FP position, int exclude_id = 0);
    }
}