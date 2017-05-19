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

        public int Faction2Index(int faction)
        {
            //ZZWTODO
            return faction;
        }

        public FactionRelation GetRelationShip(int faction_index_1, int faction_index_2)
        {
            //ZZWTODO
            if (faction_index_1 == 0 || faction_index_2 == 0)
                return FactionRelation.Neutral;
            if (faction_index_1 == faction_index_2)
                return FactionRelation.Ally;
            return FactionRelation.Enemy;
        }
    }
}