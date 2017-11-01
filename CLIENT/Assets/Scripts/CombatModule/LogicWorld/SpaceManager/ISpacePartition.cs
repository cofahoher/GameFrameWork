using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ISpacePartition : IDestruct
    {
        void AddEntiy(PositionComponent entity);
        void RemoveEntity(PositionComponent entity);
        void UpdateEntity(PositionComponent entity, Vector3FP new_position);
        List<PositionComponent> CollectEntity_All(int exclude_id = 0);
        List<PositionComponent> CollectEntity_ForwardRectangle(Vector3FP position, Vector2FP direction, FixPoint length, FixPoint width, int exclude_id = 0);
        List<PositionComponent> CollectEntity_SurroundingRing(Vector3FP position, FixPoint outer_radius, FixPoint inner_radius, int exclude_id = 0);
        List<PositionComponent> CollectEntity_ForwardSector(Vector3FP position, Vector2FP facing, FixPoint radius, FixPoint degree, int exclude_id = 0);
    }
}