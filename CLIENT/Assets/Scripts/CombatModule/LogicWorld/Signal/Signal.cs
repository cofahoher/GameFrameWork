using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SignalType
    {
        //命名是：做（什么）
        public const int Invalid = 0;
        //Player
        public const int ChangeFaction = 1;
        public const int AddEntity = 2;
        public const int RemoveEntity = 3;
        public const int KillEntity = 4;
        public const int EntityBeKilled = 5;
        //Entity
        public const int StartMoving = 1001;
        public const int StopMoving = 1002;
        public const int TakeDamage = 1003;
        public const int Die = 1004;
        public const int ChangeLevel = 1005;
        public const int CastNormalAttack = 1006;
        public const int CastSkill = 1007;

    }

    //public abstract class Signal
    //{
    //}
}