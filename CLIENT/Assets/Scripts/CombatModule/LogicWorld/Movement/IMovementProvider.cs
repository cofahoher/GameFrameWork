﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IMovementProvider : IRecyclable
    {
        void SetCallback(IMovementCallback callback);
        void SetMaxSpeed(FixPoint max_speed);
        void MoveByDirection(Vector3FP direction);
        void MoveAlongPath(List<Vector3FP> path);
        void Update(FixPoint delta_time);
        void FinishMovementWhenTargetInRange(PositionComponent target, FixPoint range);
        Vector3FP GetCurrentDirection();
    }

    public interface IMovementCallback
    {
        ILogicOwnerInfo GetOwnerInfo();
        bool AvoidObstacle();
        void MovementFinished();
    }
}