using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Formula : IRecyclable
    {
        FixPoint m_constant = FixPoint.Zero;
        ExpressionProgram m_program;

        public void Reset()
        {
            m_constant = FixPoint.Zero;
            if (m_program != null)
            {
                RecyclableObject.Recycle(m_program);
                m_program = null;
            }
        }

        public void CopyFrom(Formula rhs)
        {
            m_constant = rhs.m_constant;
            if (rhs.m_program != null)
            {
                m_program = RecyclableObject.Create<ExpressionProgram>();
                m_program.CopyFrom(rhs.m_program);
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
            ExpressionProgram program = RecyclableObject.Create<ExpressionProgram>();
            if (!program.Compile(formula_string))
                return false;
            if (program.IsConstant())
            {
                m_constant = program.Evaluate(null);
                m_program = null;
                RecyclableObject.Recycle(program);
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

        public bool IsConst()
        {
            return m_program == null;
        }
    }
}