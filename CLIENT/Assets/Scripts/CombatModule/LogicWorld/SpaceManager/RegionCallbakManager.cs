using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class RegionCallbackManager : IDestruct
    {
        LogicWorld m_logic_world;
        IDGenerator m_id_generator = new IDGenerator(IDGenerator.REGION_CALLBACK_FIRST_ID);
        SortedDictionary<int, RegionCallback> m_collection = new SortedDictionary<int, RegionCallback>();

        public RegionCallbackManager(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
        }

        public void Destruct()
        {
        }
    }
}