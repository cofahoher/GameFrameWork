using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class DamageModifierType
    {
        public static readonly int ChangeAmount = (int)CRC.Calculate("ChangeAmount");
    }

    public class DamageModifierActiveType
    {
        public static readonly int Attack = (int)CRC.Calculate("Attack");
        public static readonly int Defence = (int)CRC.Calculate("Defence");
    }

    public abstract class DamageModifier : IExpressionVariableProvider, IRecyclable
    {
        int m_id = 0;
        int m_active_type = 0;

        Object m_current_source = null;
        Object m_current_target = null;
        FixPoint m_current_basedamage = FixPoint.Zero;

        #region 注册创建回收
        public static TDamageModifier Create<TDamageModifier>() where TDamageModifier : DamageModifier, new()
        {
            return ResuableObjectFactory<DamageModifier>.Create<TDamageModifier>();
        }
        public static void Recycle(DamageModifier instance)
        {
            ResuableObjectFactory<DamageModifier>.Recycle(instance);
        }

        public static bool Registered
        {
            get { return ResuableObjectFactory<DamageModifier>.Registered; }
            set { ResuableObjectFactory<DamageModifier>.Registered = value; }
        }
        public static void Register(int id, System.Type type)
        {
            ResuableObjectFactory<DamageModifier>.Register(id, type);
        }
        public static DamageModifier Create(int id)
        {
            return ResuableObjectFactory<DamageModifier>.Create(id);
        }

        public static void RegisterDefaultModifiers()
        {
            //ZZWTODO 代码自动生成
            if (Registered)
                return;
            Register(DamageModifierType.ChangeAmount, typeof(ChangeAmountDamageModifier));
            Registered = true;
        }
        #endregion

        #region GETTER
        public int ID
        {
            get { return m_id; }
        }
        #endregion

        public void Contruct(int id, Dictionary<string, string> variables)
        {
            m_id = id;
            string value;
            if (variables.TryGetValue("active_type", out value))
                m_active_type = (int)CRC.Calculate(value);
            InitializeVariable(variables);
        }

        protected abstract void InitializeVariable(Dictionary<string, string> variables);

        public virtual void Reset()
        {
            m_id = 0;
            m_active_type = 0;
            OnReset();
        }

        protected abstract void OnReset();

        public FixPoint ApplyToDamage(Damage damage, FixPoint damage_amount, Object self, Object opponent, bool is_attacker)
        {
            if (is_attacker)
            {
                if (m_active_type == DamageModifierActiveType.Defence)
                {
                    return damage_amount;
                }
                else
                {
                    m_current_source = self;
                    m_current_target = opponent;
                }
            }
            else
            {
                if (m_active_type == DamageModifierActiveType.Attack)
                {
                    return damage_amount;
                }
                else
                {
                    m_current_source = opponent;
                    m_current_target = self;
                }
            }
            m_current_basedamage = damage.m_damage_amount;
            damage_amount = CustomApplyToDamage(damage, damage_amount, self, opponent, is_attacker);
            m_current_source = null;
            m_current_target = null;
            return damage_amount;
        }

        public abstract FixPoint CustomApplyToDamage(Damage damage, FixPoint damage_amount, Object self, Object opponent, bool is_attacker);

        public FixPoint GetVariable(ExpressionVariable variable, int index)
        {
            int vid = variable[index];
            if (vid == ExpressionVariable.VID_Target)
            {
                if (m_current_target != null)
                    return m_current_target.GetVariable(variable, index + 1);
                else
                    return FixPoint.Zero;
            }
            else if (vid == ExpressionVariable.VID_Source)
            {
                if (m_current_source != null)
                    return m_current_source.GetVariable(variable, index + 1);
                else
                    return FixPoint.Zero;
            }
            else if (vid == ExpressionVariable.VID_BaseDamage)
            {
                return m_current_basedamage;
            }
            return FixPoint.Zero;
        }
    }

    public class ChangeAmountDamageModifier : DamageModifier
    {
        Formula m_delta_amount = RecyclableObject.Create<Formula>();

        protected override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("delta_amount", out value))
                m_delta_amount.Compile(value);
        }

        protected override void OnReset()
        {
            m_delta_amount.Reset();
        }

        public override FixPoint CustomApplyToDamage(Damage damage, FixPoint damage_amount, Object self, Object opponent, bool is_attacker)
        {
            FixPoint delta_amount = m_delta_amount.Evaluate(this);
            return damage_amount + delta_amount;
        }
    }
}