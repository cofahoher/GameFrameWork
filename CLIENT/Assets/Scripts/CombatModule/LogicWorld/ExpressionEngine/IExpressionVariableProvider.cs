using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ExpressionVariable : IRecyclable, IDestruct
    {
        public static readonly int VID_Object = (int)CRC.Calculate("Object");                   //最好别用，可能你说不清楚是下面四个中的哪个
        public static readonly int VID_Player = (int)CRC.Calculate("Player");                   //所属Player
        public static readonly int VID_Entity = (int)CRC.Calculate("Entity");                   //所属Entity
        //public static readonly int VID_Skill = (int)CRC.Calculate("Skill");                     //所属Skill
        //public static readonly int VID_Effect = (int)CRC.Calculate("Effect");                   //所属Effect
        public static readonly int VID_LevelTable = (int)CRC.Calculate("LevelTable");           //等级数值表
        public static readonly int VID_Attribute = (int)CRC.Calculate("Attribute");             //属性
        public static readonly int VID_Value = (int)CRC.Calculate("Value");                     //属性
        public static readonly int VID_BaseValue = (int)CRC.Calculate("BaseValue");             //属性
        public static readonly int VID_OriginalSource = (int)CRC.Calculate("OriginalSource");   //当前Effect的原始来源
        public static readonly int VID_Source = (int)CRC.Calculate("Source");                   //当前技能施放者/当前Effect的直接来源
        public static readonly int VID_Target = (int)CRC.Calculate("Target");                   //当前技能目标/当前Effect的目标
        public static readonly int VID_Attacker = (int)CRC.Calculate("Attacker");               //当前攻击来源
        public static readonly int VID_Defender = (int)CRC.Calculate("Defender");               //当前攻击目标
        public static readonly int VID_BaseDamage = (int)CRC.Calculate("BaseDamage");           //当前攻击初始伤害

        public List<int> m_variable = new List<int>();

        public void Construct(List<string> raw_variable)
        {
            int count = raw_variable.Count;
            for (int i = 0; i < count; ++i)
                m_variable.Add((int)CRC.Calculate(raw_variable[i]));
        }

        public void Destruct()
        {
            m_variable = null;
        }

        public void Reset()
        {
            m_variable.Clear();
        }

        public int this[int index]
        {
            get
            {
                if (index >= 0 && index < m_variable.Count)
                    return m_variable[index];
                else
                    return 0;
            }
        }

        public int MaxIndex
        {
            get { return m_variable.Count - 1; }
        }
    }

    public interface IExpressionVariableProvider
    {
        FixPoint GetVariable(ExpressionVariable variable, int index);
    }
}