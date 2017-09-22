using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ICombatFactory
    {
        IConfigProvider GetConfigProvider();
        LogicWorld CreateLogicWorld();
        RenderWorld CreateRenderWorld();
        ISyncClient CreateSyncClient();
        ISyncServer CreateSyncServer();
        void RegisterCommands();
        WorldCreationContext CreateWorldCreationContext(CombatStartInfo combat_start_info);
    }
}