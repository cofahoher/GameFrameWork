using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ICommandHandler : IDestruct
    {
        void Handle(Command command);
    }

    public class DummyCommandHandler : ICommandHandler
    {
        public DummyCommandHandler()
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError("YOU Created a DummyCommandHandler. Are You SURE?");
#endif
        }

        public void Handle(Command command)
        {
        }

        public void Destruct()
        {
        }
    }
}