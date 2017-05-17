using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ISpaceManager : IDestruct
    {
        void AddEntiy(PositionComponent entity);
        void RemoveEntity(PositionComponent entity);
        void UpdateEntity(PositionComponent entity, Vector3FP new_position);
        List<int> CollectEntity_ForwardArea(Vector3FP position, Vector2FP direction, FixPoint length, FixPoint width, int exclude_id = 0);
        List<int> CollectEntity_CircleArea(Vector3FP position, FixPoint radius, int exclude_id = 0);
    }
}