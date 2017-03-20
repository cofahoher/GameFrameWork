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
    }
}