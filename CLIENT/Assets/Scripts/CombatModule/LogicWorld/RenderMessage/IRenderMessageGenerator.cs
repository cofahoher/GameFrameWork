using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IRenderMessageGenerator
    {
        bool CanGenerateRenderMessage();
        void AddRenderMessage(RenderMessage render_message);
        List<RenderMessage> GetAllRenderMessages();
        void ClearRenderMessages();
    }
}