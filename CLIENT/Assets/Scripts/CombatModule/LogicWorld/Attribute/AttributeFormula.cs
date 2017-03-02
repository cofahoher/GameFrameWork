using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeFormula : IDestruct
    {
        int m_constant = 0;
        IntProgram m_program;

        public AttributeFormula(string formula_string)
        {
            IntProgram program = IntProgram.Create();
            AttributeFormulaVariableInterface face = AttributeFormulaVariableInterface.Create();
            if (program.Compile(formula_string, face))
            {
                if (program.IsConstant())
                {
                    m_constant = program.Evaluate();
                    IntProgram.Recycle(program);
                }
                else
                {
                    m_program = program;
                }
            }
            AttributeFormulaVariableInterface.Recycle(face);
        }

        public void Destruct()
        {
            if (m_program != null)
                IntProgram.Recycle(m_program);
        }

        public int ComputeValue(AttributeFormulaEvaluationContext context)
        {
            if (m_program == null)
                return m_constant;
            AttributeFormulaVariableInterface face = AttributeFormulaVariableInterface.Create();
            face.SetContext(context);
            int result = m_program.Evaluate(face);
            AttributeFormulaVariableInterface.Recycle(face);
            return result;
        }

        public void BuildReferencedList(List<string> output)
        {
            output.Clear();
            //ZZWTODO
        }
    }
}