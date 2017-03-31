using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ExpressionProgram : IRecyclable, IDestruct
    {
        #region Create/Recycle
        public static ExpressionProgram Create()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<ExpressionProgram>();
        }

        public static void Recycle(ExpressionProgram instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
        #endregion

        public ExpressionProgram()
        {
        }

        public void Destruct()
        {
        }

        public void Reset()
        {
        }

        public bool Compile(string formula_string, IExpressionVariableProvider face)
        {
            return false;
        }

        public bool IsConstant()
        {
            return false;
        }

        public FixPoint Evaluate(IExpressionVariableProvider face = null)
        {
            return FixPoint.Zero;
        }
    }
}