using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SignalType
    {
        public const int Invalid = 0;
        public const int StartMoving = 1;
        public const int StopMoving = 2;
    }

    public abstract class Signal
    {
    }
}