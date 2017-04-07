using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Formula : IRecyclable, IDestruct
    {
        #region Create/Recycle
        public static Formula Create()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<Formula>();
        }

        public static void Recycle(Formula instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
        #endregion

        FixPoint m_constant = default(FixPoint);
        ExpressionProgram m_program;

        public void Destruct()
        {
            if (m_program != null)
            {
                ExpressionProgram.Recycle(m_program);
                m_program = null;
            }
        }

        public void Reset()
        {
            m_constant = FixPoint.Zero;
            if (m_program != null)
            {
                ExpressionProgram.Recycle(m_program);
                m_program = null;
            }
        }

        public List<ExpressionVariable> GetAllVariables()
        {
            if (m_program != null)
                return m_program.GetAllVariables();
            else
                return null;
        }

        public bool Compile(string formula_string)
        {
            ExpressionProgram program = ExpressionProgram.Create();
            if (!program.Compile(formula_string))
                return false;
            if (program.IsConstant())
            {
                m_constant = program.Evaluate(null);
                m_program = null;
                ExpressionProgram.Recycle(program);
            }
            else
            {
                m_program = program;
            }
            return true;
        }

        public FixPoint Evaluate(IExpressionVariableProvider variable_provider)
        {
            if (m_program != null)
                return m_program.Evaluate(variable_provider);
            else
                return m_constant;
        }
    }
}