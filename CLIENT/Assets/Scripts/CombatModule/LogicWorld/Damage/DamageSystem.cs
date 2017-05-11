using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class DamageSystem : Singleton<DamageSystem>, IExpressionVariableProvider
    {
        Dictionary<int, Formula> m_damage_type_formula = new Dictionary<int, Formula>();
        FixPoint m_value;
        Entity m_attacker = null;
        Entity m_defender = null;

        private DamageSystem()
        {
        }

        public override void Destruct()
        {
        }

        public void RegisterDamageType(int damage_type, string formula_string)
        {
            Formula formula = new Formula();
            formula.Compile(formula_string);
            m_damage_type_formula[damage_type] = formula;
        }

        public FixPoint CalculateDamageAmount(int damage_type, FixPoint damage_amount, Entity attacker, Entity defender)
        {
            m_value = damage_amount;
            m_attacker = attacker;
            m_defender = defender;
            Formula formula;
            if (m_damage_type_formula.TryGetValue(damage_type, out formula))
                m_value = formula.Evaluate(this);
            m_attacker = null;
            m_defender = null;
            return m_value;
        }

        public FixPoint GetVariable(ExpressionVariable variable, int index)
        {
            int vid = variable[index];
            if (index == variable.MaxIndex)
            {
                if (vid == ExpressionVariable.VID_BaseDamage)
                    return m_value;
            }
            else if (vid == ExpressionVariable.VID_Attacker)
            {
                if (m_attacker != null)
                    return m_attacker.GetVariable(variable, index + 1);
            }
            else if (vid == ExpressionVariable.VID_Defender)
            {
                if (m_defender != null)
                    return m_defender.GetVariable(variable, index + 1);
            }
            return FixPoint.Zero;
        }
    }
}