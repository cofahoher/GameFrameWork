using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectGenerator : IRecyclable, IDestruct
    {
        LogicWorld m_logic_world;
        public void Reset()
        {
            m_logic_world = null;
        }

        public void Destruct()
        {
            Reset();
        }
    }
}