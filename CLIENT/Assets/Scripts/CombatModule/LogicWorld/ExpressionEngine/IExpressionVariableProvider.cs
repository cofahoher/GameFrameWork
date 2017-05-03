using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ExpressionVariable : IRecyclable, IDestruct
    {
        public static readonly int VID_Object = (int)CRC.Calculate("Object");
        public static readonly int VID_Entity = (int)CRC.Calculate("Entity");
        public static readonly int VID_Player = (int)CRC.Calculate("Player");
        public static readonly int VID_Target = (int)CRC.Calculate("Target");
        public static readonly int VID_Attacker = (int)CRC.Calculate("Attacker");
        public static readonly int VID_Defender = (int)CRC.Calculate("Defender");
        public static readonly int VID_LevelTable = (int)CRC.Calculate("LevelTable");
        public static readonly int VID_Attribute = (int)CRC.Calculate("Attribute");
        public static readonly int VID_Value = (int)CRC.Calculate("Value");
        public static readonly int VID_BaseValue = (int)CRC.Calculate("BaseValue");
        /*
         * 在各种上下文（指IExpressionVariableProvider实现者），都可以这样写公式的一部分
         * max_speed
         * angle
         * Object.angle
         * Object.max_speed
         * Entity.max_speed
         * Attribute.Strength.Value
         * Object.Attribute.Strength.Value
         * 
         * 在AttributeManagerComponent和Attribute上下文，还可以这么写
         * Strength.Value
         * 
         * 在Attribute上下文，还可以这么写
         * Value
         * BaseValue
         */

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