using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class FactionRelation
    {
        //元关系
        public static readonly int Ally = (int)CRC.Calculate("Ally");
        public static readonly int Enemy = (int)CRC.Calculate("Enemy");
        public static readonly int Neutral = (int)CRC.Calculate("Neutral");
        //复合关系
        public static readonly int NotAlly = (int)CRC.Calculate("NotAlly");
        public static readonly int NotEnemy = (int)CRC.Calculate("NotEnemy");
        public static readonly int NotNeutral = (int)CRC.Calculate("NotNeutral");
        public static readonly int All = (int)CRC.Calculate("All");
        public static readonly int None = (int)CRC.Calculate("None");

        public static bool IsFactionSatisfied(int meta_faction, int composed_faction)
        {
            if (meta_faction == composed_faction || composed_faction == All)
                return true;
            else if (composed_faction == NotAlly)
            {
                if (meta_faction == Ally)
                    return false;
                else
                    return true;
            }
            else if (composed_faction == NotEnemy)
            {
                if (meta_faction == Enemy)
                    return false;
                else
                    return true;
            }
            else if (composed_faction == NotNeutral)
            {
                if (meta_faction == Neutral)
                    return false;
                else
                    return true;
            }
            return false;
        }
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

        public int GetRelationShip(int faction_index_1, int faction_index_2)
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