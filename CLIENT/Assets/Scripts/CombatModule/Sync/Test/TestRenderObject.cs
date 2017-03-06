using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class TestRenderObject : IDestruct
    {
        TestObject m_logic_object;

        public TestRenderObject(TestObject logic_object)
        {
            m_logic_object = logic_object;
        }

        public void Destruct()
        {
            m_logic_object = null;
        }
    }
}