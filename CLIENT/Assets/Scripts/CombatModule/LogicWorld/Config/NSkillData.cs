using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class SkillRelationType
    {
        public static readonly int Seperate = (int)CRC.Calculate("direction");
        //successive
        //sequence
        //public static readonly
    }

    public class NSkillData
    {
        string m_mana_cost;
        string m_cooldown_time;
        public List<int> m_skills = new List<int>();
        public int m_skill_relation;
    }
}