using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SignalType
    {
        //命名是：做（什么）
        public const int Invalid = 0;
        public const int StartMoving = 1;
        public const int StopMoving = 2;
        public const int TakeDamage = 3;
        public const int Die = 4;
        public const int ChangeLevel = 5;
    }

    //public abstract class Signal
    //{
    //}
}