using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class RenderMessageType
    {
        public const int CreateEntity = 11;               //SimpleRenderMessage
        public const int DestroyEntity = 12;              //SimpleRenderMessage
        public const int StartMoving = 13;                //SimpleRenderMessage
        public const int StopMoving = 14;                 //SimpleRenderMessage
    }
}