using System.Collections;
namespace Combat
{
    public class ObjectCreationContext
    {
        public int m_object_type_id = -1;
        public int m_object_proto_id = -1;
        public ObjectTypeData m_custom_data;
        public string m_name;

        public LogicWorld m_logic_world;
        public System.Type m_class_type;
        public ObjectTypeData m_type_data;
        public ObjectProtoData m_proto_data;

        public int m_object_id = -1;
        public int m_object_proxy_id = -1;
        public int m_owner_id = -1;
        public bool m_is_ai = false;
        public bool m_is_local = false;
    }
}