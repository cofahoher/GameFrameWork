using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class PlayerManager : ObjectManager<Player>
    {
        /*
         * 1、局外的玩家有用于存数据库的persist id
         * 2、局内的Player有动态生成的id
         * 3、在关卡配置时，也需要Player的某个ID，好指代从属关系，所以搞个proxy id
         * 
         * 2和3也许可以用同一个，但是为了更灵活和以防万一，还是把动态生成的和写死的隔离开吧
         * 
         * 每个模式都应该有个创建CombatStartInfo的函数，把persist id和proxy id对应起来
         * 对于多人联机，且没有场景指定归属的entity，proxy id可以动态指定为CRC(pstid)
         */
        public const int ENVIRONMENT_PLAYER_PROXYID = -1;
        public const int AI_ENEMY_PLAYER_PROXYID = -2;
        public const int LOCAL_PLAYER_PROXYID = -3;
        public const int PLAYER_1_PROXYID = -11;
        public const int PLAYER_2_PROXYID = -12;
        public const int PLAYER_3_PROXYID = -13;
        public const int PLAYER_4_PROXYID = -14;
        public const int PLAYER_5_PROXYID = -15;
        public const int PLAYER_6_PROXYID = -16;

        Dictionary<long, int> m_pstid2proxyid = new Dictionary<long, int>();
        Dictionary<int, long> m_proxyid2pstid = new Dictionary<int, long>();
        Dictionary<int, int> m_objectid2proxyid = new Dictionary<int, int>();
        Dictionary<int, int> m_proxyid2objectid = new Dictionary<int, int>();
        int m_local_player_id = 0;
        int m_ai_enemy_player_id = 0;

        public PlayerManager(LogicWorld logic_world)
            : base(logic_world, IDGenerator.PLAYER_FIRST_ID)
        {
        }

        #region 初始化
        protected override Player CreateObjectInstance(ObjectCreationContext context)
        {
            int object_id = context.m_object_id;
            int proxy_id = context.m_object_proxy_id;
            m_objectid2proxyid[object_id] = proxy_id;
            m_proxyid2objectid[proxy_id] = object_id;
            if (context.m_is_local)
                m_local_player_id = object_id;
            if (proxy_id == AI_ENEMY_PLAYER_PROXYID)
                m_ai_enemy_player_id = object_id;
            return new Player();
        }

        public void SetPstidAndProxyid(Dictionary<long, int> pstid2proxyid, Dictionary<int, long> proxyid2pstid)
        {
            m_pstid2proxyid = pstid2proxyid;
            m_proxyid2pstid = proxyid2pstid;
        }
        #endregion

        #region ID
        public int Pstid2Proxyid(long pstid)
        {
            int proxyid = 0;
            m_pstid2proxyid.TryGetValue(pstid, out proxyid);
            return proxyid;
        }
        public int Pstid2Objectid(long pstid)
        {
            int proxyid = 0;
            m_pstid2proxyid.TryGetValue(pstid, out proxyid);
            int objectid = 0;
            m_proxyid2objectid.TryGetValue(proxyid, out objectid);
            return objectid;
        }
        public long Proxyid2Pstid(int proxyid)
        {
            long pstid = 0;
            m_proxyid2pstid.TryGetValue(proxyid, out pstid);
            return pstid;
        }
        public int Proxyid2Objectid(int proxyid)
        {
            int objectid = 0;
            m_proxyid2objectid.TryGetValue(proxyid, out objectid);
            return objectid;
        }
        public long Objectid2Pstid(int objectid)
        {
            int proxyid = 0;
            m_objectid2proxyid.TryGetValue(objectid, out proxyid);
            long pstid = 0;
            m_proxyid2pstid.TryGetValue(proxyid, out pstid);
            return pstid;
        }
        public int Objectid2Proxyid(int objectid)
        {
            int proxyid = 0;
            m_objectid2proxyid.TryGetValue(objectid, out proxyid);
            return proxyid;
        }
        #endregion

        #region Specila
        public int GetLocalPlayerID()
        {
            return m_local_player_id;
        }

        public Player GetLocalPlayer()
        {
            if (m_local_player_id > 0)
                return m_objects[m_local_player_id];
            else
                return null;
        }

        public int GetAIEnemyPlayerID()
        {
            return m_ai_enemy_player_id;
        }

        public Player GetAIEnemyPlayer()
        {
            if (m_ai_enemy_player_id > 0)
                return m_objects[m_ai_enemy_player_id];
            else
                return null;
        }
        #endregion
    }
}