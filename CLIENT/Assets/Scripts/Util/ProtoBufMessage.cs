#define PROTOBUF_OPTIMIZE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BaseUtil
{
    public sealed class ProtoBufAttribute : System.Attribute
    {
        private int m_index;
        public int Index { get { return m_index; } set { m_index = value; } }
        public ProtoBufAttribute()
        {
            m_index = -1;
        }
    }
}
