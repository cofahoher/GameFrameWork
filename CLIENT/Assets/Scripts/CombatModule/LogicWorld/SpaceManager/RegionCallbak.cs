using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class RegionCallback : IRecyclable
    {
        LogicWorld m_logic_world;

        public RegionCallback(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
        }

        public void Reset()
        {
        }
    }
}