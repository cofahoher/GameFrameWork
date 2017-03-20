using System;
using System.Collections.Generic;

namespace BaseUtil
{
    public class NetMessageFactory
    {
		private static Dictionary<int, Type> m_msgtypes = new Dictionary<int, Type>();

        public static void RegisterMessage(Type type)
        {
            NetMessage msg = Activator.CreateInstance(type) as NetMessage;
            if (msg != null)
            {
                m_msgtypes.Add(msg.CLSID, type);
            }
        }
        public static NetMessage CreateMessage(int clsid)
        {
            Type type=null;
            m_msgtypes.TryGetValue(clsid, out type);
            if (type != null)
            {
                NetMessage msg = Activator.CreateInstance(type) as NetMessage;
                return msg;
            }
            else
            {
                return new UnknownMessage(clsid);
            }
        }
    }
}
