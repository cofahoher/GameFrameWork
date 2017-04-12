using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ObjectCreationContext
    {
        //配置数据
        public int m_object_proxy_id = -1;
        public int m_object_type_id = -1;
        public int m_object_proto_id = -1;
        public BirthPositionInfo m_birth_info = null;
        public string m_name;
        public System.Object m_custom_data = null;
        //实际配置
        public ObjectTypeData m_type_data = null;
        public ObjectProtoData m_proto_data = null;
        //运行数据
        public LogicWorld m_logic_world = null;
        public int m_object_id = -1;
        public int m_owner_id = -1;
        public bool m_is_ai = false;
        public bool m_is_local = false;

        public void SetProxyIDFromPstid(long pstid)
        {
            /*
             * 如果一种模式没有在关卡中，给予指定的玩家Object，那么可以用pstid做为proxy_id
             * 单人可以继续用LOCAL_PLAYER_PROXYID
             */
            m_object_proxy_id = (int)CRC.Calculate(pstid);
        }
    }
}