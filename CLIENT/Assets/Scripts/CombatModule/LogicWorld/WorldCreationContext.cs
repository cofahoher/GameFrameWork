using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    /*
     * 场景：带地图，决定些寻路等数据
     * 模式：确定需要几个Player玩，Player之间的关系
     * 关卡：关联一个场景和模式，并可以在场景中放置属于某些Player的Object
     * 
     * 服务器开局数据：局外带入的NPC或非NPC的数据，也许就可以是CombatStartInfo
     * CombatStartInfo：把pstid和proxyid对应起来，把模式需要的Player和服务器发送的Player一一对应好
     * WorldCreationContext：结合关卡配置和CombatStartInfo，组织成模式无关的创建上下文
     */
    public class WorldCreationContext
    {
        public int m_level_id = -1;
        public int m_game_mode = -1;
        public int m_world_seed = -1;
        public Dictionary<long, int> m_pstid2proxyid = new Dictionary<long, int>();
        public Dictionary<int, long> m_proxyid2pstid = new Dictionary<int, long>();
        public List<ObjectCreationContext> m_players = new List<ObjectCreationContext>();
        public List<ObjectCreationContext> m_entities = new List<ObjectCreationContext>();

        public static WorldCreationContext CreateWorldCreationContext(CombatStartInfo combat_start_info)
        {
            WorldCreationContext world_context = new WorldCreationContext();
            world_context.m_level_id = combat_start_info.m_level_id;
            world_context.m_game_mode = combat_start_info.m_game_mode;
            world_context.m_world_seed = combat_start_info.m_world_seed;
            world_context.BuildDemoContext();
            return world_context;
        }

        void BuildDemoContext()
        {
            //本地玩家
            m_pstid2proxyid[CombatTester.TEST_LOCAL_PLAYER_PSTID] = PlayerManager.LOCAL_PLAYER_PROXYID;
            m_proxyid2pstid[PlayerManager.LOCAL_PLAYER_PROXYID] = CombatTester.TEST_LOCAL_PLAYER_PSTID;
            ObjectCreationContext obj_context = new ObjectCreationContext();
            obj_context.m_object_proxy_id = PlayerManager.LOCAL_PLAYER_PROXYID;
            obj_context.m_object_type_id = 3;
            obj_context.m_object_proto_id = -1;
            m_players.Add(obj_context);

            //敌人
            long proxy_pstid = PlayerManager.AI_ENEMY_PLAYER_PROXYID;
            m_pstid2proxyid[proxy_pstid] = PlayerManager.AI_ENEMY_PLAYER_PROXYID;
            m_proxyid2pstid[PlayerManager.AI_ENEMY_PLAYER_PROXYID] = proxy_pstid;
            obj_context = new ObjectCreationContext();
            obj_context.m_object_proxy_id = PlayerManager.AI_ENEMY_PLAYER_PROXYID;
            obj_context.m_object_type_id = 2;
            obj_context.m_object_proto_id = -1;
            m_players.Add(obj_context);

            //本地玩家的Entity
            obj_context = new ObjectCreationContext();
            obj_context.m_object_proxy_id = PlayerManager.LOCAL_PLAYER_PROXYID;
            obj_context.m_object_type_id = 101;
            obj_context.m_object_proto_id = 101001;
            obj_context.m_birth_info = new BirthPositionInfo(-1000, 0, 0, 90);
            m_entities.Add(obj_context);

            obj_context = new ObjectCreationContext();
            obj_context.m_object_proxy_id = PlayerManager.AI_ENEMY_PLAYER_PROXYID;
            obj_context.m_object_type_id = 101;
            obj_context.m_object_proto_id = 101002;
            obj_context.m_birth_info = new BirthPositionInfo(1000, 0, 0, 0);
            m_entities.Add(obj_context);
        }
    }
}