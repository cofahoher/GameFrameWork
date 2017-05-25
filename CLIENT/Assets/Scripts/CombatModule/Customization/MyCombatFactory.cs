using System.Collections;
using System.Collections.Generic;
namespace Combat
{
#if !CONSOLE_CLIENT
    public class MyCombatFactory : ICombatFactory
    {
        public IConfigProvider GetConfigProvider()
        {
            return ConfigProvider.Instance;
        }

        public LogicWorld CreateLogicWorld()
        {
            return new MyLogicWorld();
        }

        public RenderWorld CreateRenderWorld()
        {
            return new MyRenderWorld();
        }

        public ISyncClient CreateSyncClient()
        {
            return new SPSyncClient();
        }

        public ISyncServer CreateSyncServer()
        {
            return new SPSyncServer();
        }

        public void RegisterComponents()
        {
            ComponentTypeRegistry.RegisterDefaultComponents();
//#if COMBAT_CLIENT
//            ComponentTypeRegistry.Register(typeof(ModelComponent), false);
//#endif
        }

        public void RegisterCommands()
        {
            if (!Command.Registered)
                Command.Registered = true;
            Command.Register(CommandType.SyncTurnDone, typeof(SyncTurnDoneCommand));
            Command.Register(CommandType.EntityMove, typeof(EntityMoveCommand));
            Command.Register(CommandType.EntityTarget, typeof(EntityTargetCommand));
        }

        public void RegisterRenderMessages()
        {
            if (!RenderMessage.Registered)
                RenderMessage.Registered = true;
            RenderMessage.Register(RenderMessageType.CreateEntity, typeof(SimpleRenderMessage));
            RenderMessage.Register(RenderMessageType.DestroyEntity, typeof(SimpleRenderMessage));
            RenderMessage.Register(RenderMessageType.StartMoving, typeof(SimpleRenderMessage));
            RenderMessage.Register(RenderMessageType.StopMoving, typeof(SimpleRenderMessage));
            RenderMessage.Register(RenderMessageType.ChangeHealth, typeof(ChangeHealthRenderMessage));
        }

        public WorldCreationContext CreateWorldCreationContext(CombatStartInfo combat_start_info)
        {
            WorldCreationContext world_context = new WorldCreationContext();
            world_context.m_level_id = combat_start_info.m_level_id;
            world_context.m_game_mode = combat_start_info.m_game_mode;
            world_context.m_world_seed = combat_start_info.m_world_seed;

            //本地玩家
            world_context.m_pstid2proxyid[CombatTester.TEST_LOCAL_PLAYER_PSTID] = PlayerManager.LOCAL_PLAYER_PROXYID;
            world_context.m_proxyid2pstid[PlayerManager.LOCAL_PLAYER_PROXYID] = CombatTester.TEST_LOCAL_PLAYER_PSTID;
            ObjectCreationContext obj_context = new ObjectCreationContext();
            obj_context.m_object_proxy_id = PlayerManager.LOCAL_PLAYER_PROXYID;
            obj_context.m_object_type_id = 3;
            obj_context.m_object_proto_id = -1;
            obj_context.m_is_local = true;
            world_context.m_players.Add(obj_context);

            //NPC敌人玩家
            long proxy_pstid = PlayerManager.AI_ENEMY_PLAYER_PROXYID;
            world_context.m_pstid2proxyid[proxy_pstid] = PlayerManager.AI_ENEMY_PLAYER_PROXYID;
            world_context.m_proxyid2pstid[PlayerManager.AI_ENEMY_PLAYER_PROXYID] = proxy_pstid;
            obj_context = new ObjectCreationContext();
            obj_context.m_object_proxy_id = PlayerManager.AI_ENEMY_PLAYER_PROXYID;
            obj_context.m_object_type_id = 2;
            obj_context.m_object_proto_id = -1;
            world_context.m_players.Add(obj_context);

            //本地玩家的Entity
            obj_context = new ObjectCreationContext();
            obj_context.m_object_proxy_id = PlayerManager.LOCAL_PLAYER_PROXYID;
            obj_context.m_object_type_id = 111;
            obj_context.m_object_proto_id = 111001;
            obj_context.m_birth_info = new BirthPositionInfo(new FixPoint(-5), new FixPoint(0), new FixPoint(-5), new FixPoint(90));
            obj_context.m_is_local = true;
            world_context.m_entities.Add(obj_context);

            obj_context = new ObjectCreationContext();
            obj_context.m_object_proxy_id = PlayerManager.LOCAL_PLAYER_PROXYID;
            obj_context.m_object_type_id = 101;
            obj_context.m_object_proto_id = 101002;
            obj_context.m_birth_info = new BirthPositionInfo(new FixPoint(-5), new FixPoint(0), new FixPoint(0), new FixPoint(90));
            obj_context.m_is_local = true;
            obj_context.m_is_ai = true;
            world_context.m_entities.Add(obj_context);

            obj_context = new ObjectCreationContext();
            obj_context.m_object_proxy_id = PlayerManager.LOCAL_PLAYER_PROXYID;
            obj_context.m_object_type_id = 112;
            obj_context.m_object_proto_id = 112001;
            obj_context.m_birth_info = new BirthPositionInfo(new FixPoint(-5), new FixPoint(0), new FixPoint(7), new FixPoint(90));
            obj_context.m_is_local = true;
            obj_context.m_is_ai = true;
            world_context.m_entities.Add(obj_context);

            //NPC敌人玩家Entity
            obj_context = new ObjectCreationContext();
            obj_context.m_object_proxy_id = PlayerManager.AI_ENEMY_PLAYER_PROXYID;
            obj_context.m_object_type_id = 101;
            obj_context.m_object_proto_id = 101001;
            obj_context.m_birth_info = new BirthPositionInfo(new FixPoint(5), new FixPoint(0), new FixPoint(0), new FixPoint(90)); ;
            world_context.m_entities.Add(obj_context);

            return world_context;
        }
    }
#endif
}