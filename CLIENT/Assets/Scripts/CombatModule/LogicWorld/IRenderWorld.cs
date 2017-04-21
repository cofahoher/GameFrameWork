using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IRenderWorld
    {
        void OnLogicWorldHandleCommand(Command cmd, bool result);
    }
}