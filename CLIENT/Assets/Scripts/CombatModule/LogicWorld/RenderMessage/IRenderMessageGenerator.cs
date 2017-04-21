using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IRenderMessageGenerator
    {
        bool CanGenerateRenderMessage();
        void AddRenderMessage(RenderMessage render_message);
        void AddSimpleRenderMessage(int type, int entity_id = -1, int simple_data = 0);
        List<RenderMessage> GetAllRenderMessages();
        void ClearRenderMessages();
    }
}