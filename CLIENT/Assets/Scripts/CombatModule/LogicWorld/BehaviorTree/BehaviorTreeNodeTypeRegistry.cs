using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BehaviorTreeNodeTypeRegistry
    {
        static bool ms_default_btnodes_registered = false;
        static Dictionary<int, System.Type> m_btnodes_id2type = new Dictionary<int, System.Type>();
        static Dictionary<System.Type, int> m_btnodes_type2id = new Dictionary<System.Type, int>();

        public static void Register<TBTNodeType>()
        {
            Register(typeof(TBTNodeType));
        }

        public static void Register(System.Type type)
        {
            int btnode_type_id = (int)CRC.Calculate(type.Name);
#if UNITY_EDITOR
            System.Type existed_type;
            if (m_btnodes_id2type.TryGetValue(btnode_type_id, out existed_type))
            {
                if (type.Name != existed_type.Name)
                    LogWrapper.LogError("BehaviorTreeNodeTypeRegistry, BTNode ", type.FullName, " has same crcid with existed BTNode ", existed_type.FullName);
            }
#endif
            m_btnodes_id2type[btnode_type_id] = type;
            m_btnodes_type2id[type] = btnode_type_id;
        }

        public static BTNode CreateBTNode(int btnode_type_id)
        {
            System.Type type = null;
            if (!m_btnodes_id2type.TryGetValue(btnode_type_id, out type))
                return null;
            return System.Activator.CreateInstance(type) as BTNode;
        }
    }
}