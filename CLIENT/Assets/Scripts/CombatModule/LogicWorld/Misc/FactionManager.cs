using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum FactionRelation
    {
        Ally = 1,
        Enemy = 2,
        Neutral = 3,
    }

    public class FactionManager : IDestruct
    {
        LogicWorld m_logic_world;

        public FactionManager(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
        }

        public void Destruct()
        {
            m_logic_world = null;
        }

        public FactionRelation GetRelationShip(int faction_id_1, int faction_id_2)
        {
            return FactionRelation.Neutral;
        }
    }
}