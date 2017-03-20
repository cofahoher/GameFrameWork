using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IRenderMessageProcessor : IDestruct
    {
        void Process(RenderMessage render_message);
    }

    public class DummyRenderMessageProcessor : IRenderMessageProcessor
    {
        public DummyRenderMessageProcessor()
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError("YOU Created a DummyRenderMessageProcessor. Are You SURE?");
#endif
        }

        public void Process(RenderMessage render_message)
        {
        }

        public void Destruct()
        {
        }
    }
}