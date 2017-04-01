using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeFormula : IDestruct
    {
        FixPoint m_constant = default(FixPoint);
        ExpressionProgram m_program;

        public AttributeFormula(string formula_string)
        {
            ExpressionProgram program = ExpressionProgram.Create();
            AttributeFormulaVariableProvider variable_provider = AttributeFormulaVariableProvider.Create();
            if (program.Compile(formula_string, variable_provider))
            {
                if (program.IsConstant())
                {
                    m_constant = program.Evaluate();
                    m_program = null;
                    ExpressionProgram.Recycle(program);
                }
                else
                {
                    m_program = program;
                }
            }
            AttributeFormulaVariableProvider.Recycle(variable_provider);
        }

        public void Destruct()
        {
            if (m_program != null)
                ExpressionProgram.Recycle(m_program);
        }

        public FixPoint ComputeValue(AttributeFormulaEvaluationContext context)
        {
            if (m_program == null)
                return m_constant;
            AttributeFormulaVariableProvider face = AttributeFormulaVariableProvider.Create();
            face.SetContext(context);
            FixPoint result = m_program.Evaluate(face);
            AttributeFormulaVariableProvider.Recycle(face);
            return result;
        }

        public void BuildReferencedList(List<string> output)
        {
            output.Clear();
            //ZZWTODO 找出依赖的属性
        }
    }
}